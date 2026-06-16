using System.Collections;
using Enemy.Melee;
using UnityEngine;

namespace Enemy
{
    /// <summary>
    /// 远近切换敌人：
    /// - 远：追击并在射击距离内射击
    /// - 近：切换为近战形态（拿出近战武器），停下并进行“转一圈”攻击（协程版）
    ///
    /// 近战攻击期间：
    /// - MeleeAttackHitbox Arm：可造成伤害、可被弹反（子弹时间里高亮圈由 MeleeAttackHighlight 控制）
    /// - 被弹反：打断攻击协程 + 关闭 hitbox + 击退 + 硬直
    /// </summary>
    public class HybridEnemy : EnemyController
    {
        public enum ECombatMode
        {
            Ranged,
            Melee
        }

        [Header("Switch Distance")]
        [Tooltip("进入近战形态的距离（<= 该距离切近战）")]
        public float meleeEnterDistance = 2.0f;

        [Tooltip("退出近战形态的距离（>= 该距离切回远程）。建议比 Enter 大一些，避免频繁抖动切换")]
        public float meleeExitDistance = 2.6f;

        [Header("Ranged")]
        public float shootDistance = 8f;
        public float shootInterval = 1.5f;
        public Transform firePoint;
        public string bulletPoolKey = "EnemyBullet";
        public float bulletSpeed = 10f;

        [Header("Weapon Visual")]
        [Tooltip("近战武器物体（例如刀/棍模型）。进入近战时 SetActive(true)，否则隐藏")]
        public GameObject meleeWeaponObject;

        [Tooltip("远程武器物体（例如枪模型）。进入远程时 SetActive(true)，否则隐藏")]
        public GameObject rangedWeaponObject;

        [Header("Melee Attack (Coroutine)")]
        [Tooltip("近战攻击判定盒（子物体 Trigger Collider + MeleeAttackHitbox 脚本）")]
        public MeleeAttackHitbox meleeHitbox;

        [Tooltip("近战攻击间隔：两次攻击开始之间的间隔")]
        public float meleeAttackInterval = 1.2f;

        [Tooltip("一次近战攻击动作持续时间（也是可弹反窗口）")]
        public float meleeAttackDuration = 0.6f;

        [Tooltip("一次攻击动作总共旋转角度（360=转一圈）")]
        public float meleeSpinDegrees = 360f;

        [Header("Parry Reaction")]
        public float parryKnockbackDistance = 2.0f;
        public float parryStunTime = 0.8f;

        private float _shootTimer;
        private float _meleeIntervalTimer;

        private ECombatMode _mode = ECombatMode.Ranged;

        // melee attack coroutine state
        private Coroutine _meleeAttackCoroutine;
        // private bool _isMeleeAttacking;
        private float _meleeAttackTime;

        // parry stun
        private float _parryStunTimer;

        protected override void Start()
        {
            base.Start();
            ApplyWeaponVisual(_mode);

            // 防呆：初始关闭判定盒，避免开局就能打到玩家/出现高亮
            if (meleeHitbox != null)
                meleeHitbox.Disarm();
        }

        protected override void OnPlayerDetected()
        {
            if (player == null) return;

            float dist = Vector3.Distance(transform.position, player.position);

            UpdateCombatModeByDistance(dist);

            if (_mode == ECombatMode.Melee)
                DoMelee(dist);
            else
                DoRanged(dist);
        }

        private void UpdateCombatModeByDistance(float dist)
        {
            if (_mode == ECombatMode.Ranged)
            {
                if (dist <= meleeEnterDistance)
                    SetMode(ECombatMode.Melee);
            }
            else
            {
                if (dist >= meleeExitDistance)
                    SetMode(ECombatMode.Ranged);
            }
        }

        private void SetMode(ECombatMode newMode)
        {
            if (_mode == newMode) return;

            _mode = newMode;

            // 切换形态时，重置计时，避免刚切过去就立刻开枪/攻击
            _shootTimer = 0f;
            _meleeIntervalTimer = 0f;

            // 切到远程：强制停止近战攻击，避免 hitbox 残留
            if (newMode != ECombatMode.Melee)
            {
                StopMeleeAttack();
            }

            ApplyWeaponVisual(_mode);
            OnModeChanged(newMode);
        }

        protected virtual void OnModeChanged(ECombatMode newMode)
        {
            // 预留：未来可以接 Animator / VFX / SFX
        }

        private void ApplyWeaponVisual(ECombatMode mode)
        {
            if (meleeWeaponObject != null)
                meleeWeaponObject.SetActive(mode == ECombatMode.Melee);

            if (rangedWeaponObject != null)
                rangedWeaponObject.SetActive(mode == ECombatMode.Ranged);
        }

        private void DoRanged(float dist)
        {
            // 远程逻辑：距离 > shootDistance 时追击；否则停下射击
            if (dist > shootDistance)
            {
                TrySetDestination(player.position);
                return;
            }

            StopAgentMovement();

            // 水平朝向玩家
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

            _shootTimer += Time.deltaTime;
            if (_shootTimer >= shootInterval)
            {
                Shoot();
                _shootTimer = 0f;
            }
        }

        private void DoMelee(float dist)
        {
            // 弹反硬直中：不攻击
            if (_parryStunTimer > 0f)
            {
                _parryStunTimer -= Time.deltaTime;
                StopAgentMovement();
                return;
            }

            // 离得不够近：追一下
            if (dist > meleeEnterDistance)
            {
                TrySetDestination(player.position);
                return;
            }

            StopAgentMovement();

            // 近战时面向玩家（你想“攻击过程中也一直跟随朝向”就保留；想更像挥砍可移到协程开头只 LookAt 一次）
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

            // 攻击协程运行中：不再计时触发下一次攻击
            if (_meleeAttackCoroutine != null)
                return;

            // 间隔计时 -> 到点就开打
            _meleeIntervalTimer += Time.deltaTime;
            if (_meleeIntervalTimer >= meleeAttackInterval)
            {
                _meleeIntervalTimer = 0f;
                StartMeleeAttack();
            }
        }

        private void StartMeleeAttack()
        {
            if (_meleeAttackCoroutine != null) return;
            _meleeAttackCoroutine = StartCoroutine(MeleeAttackRoutine());
        }

        private IEnumerator MeleeAttackRoutine()
        {
            // _isMeleeAttacking = true;
            _meleeAttackTime = 0f;

            if (meleeAttackDuration < 0.05f)
                meleeAttackDuration = 0.05f;

            // 开启攻击判定窗口（也会让你的 MeleeAttackHighlight 在 BulletTime 中出现高亮圈）
            if (meleeHitbox != null)
                meleeHitbox.Arm();

            float degPerSec = meleeSpinDegrees / meleeAttackDuration;

            while (_meleeAttackTime < meleeAttackDuration)
            {
                // 若被弹反进入硬直，立刻结束（保险）
                if (_parryStunTimer > 0f)
                    break;

                // 转圈攻击表现
                transform.Rotate(Vector3.up, degPerSec * Time.deltaTime, Space.World);

                _meleeAttackTime += Time.deltaTime;
                yield return null;
            }

            // 结束时关闭判定盒（同时会让高亮圈消失，避免残留）
            if (meleeHitbox != null)
                meleeHitbox.Disarm();

            // _isMeleeAttacking = false;
            _meleeAttackTime = 0f;
            _meleeAttackCoroutine = null;
        }

        private void StopMeleeAttack()
        {
            if (_meleeAttackCoroutine != null)
            {
                StopCoroutine(_meleeAttackCoroutine);
                _meleeAttackCoroutine = null;
            }

            // _isMeleeAttacking = false;
            _meleeAttackTime = 0f;

            if (meleeHitbox != null)
                meleeHitbox.Disarm();
        }

        /// <summary>
        /// 被近战攻击弹反时调用（由 MeleeAttackHitbox.Parry() 触发）
        /// </summary>
        public void OnMeleeParried(Transform playerTf)
        {
            // 立刻打断近战攻击（协程 + 判定盒）
            StopMeleeAttack();

            // 进入硬直
            _parryStunTimer = parryStunTime;

            // 击退（简单稳定版：直接位移）
            if (playerTf != null)
            {
                Vector3 dir = (transform.position - playerTf.position);
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.0001f)
                {
                    dir.Normalize();
                    transform.position += dir * parryKnockbackDistance;
                }
            }
        }

        private void StopAgentMovement()
        {
            if (agent == null) return;
            if (!agent.isActiveAndEnabled) return;
            if (!agent.isOnNavMesh) return;

            agent.isStopped = true;
            agent.ResetPath();
        }

        private void Shoot()
        {
            if (firePoint == null)
            {
                Debug.LogWarning("[HybridEnemy] firePoint is null!");
                return;
            }

            TriggerShootAnim();
            if (ObjectPoolManager.Instance == null) return;

            GameObject bullet = ObjectPoolManager.Instance.Get(bulletPoolKey, firePoint.position, Quaternion.identity);
            if (bullet == null) return;

            EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
            if (bulletScript == null) return;

            Vector3 dir = (player.position - firePoint.position).normalized;
            bulletScript.Init(dir * bulletSpeed);
            bulletScript.SetSender(this);
        }

        private void OnValidate()
        {
            // Inspector 防呆：Exit 必须 >= Enter，否则会非常抖
            if (meleeExitDistance < meleeEnterDistance)
                meleeExitDistance = meleeEnterDistance + 0.1f;

            if (meleeAttackDuration < 0.05f)
                meleeAttackDuration = 0.05f;
        }
    }
}