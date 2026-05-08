using UnityEngine;

namespace Enemy
{
    /// <summary>
    /// 远近切换敌人：
    /// - 远：追击并在射击距离内射击
    /// - 近：切换为近战形态（拿出近战武器），停下并可触发近战动画/攻击（预留）
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

        [Header("Melee (visual only for now)")]
        [Tooltip("近战武器物体（例如刀/棍模型）。进入近战时 SetActive(true)，否则隐藏")]
        public GameObject meleeWeaponObject;

        [Tooltip("远程武器物体（例如枪模型）。进入远程时 SetActive(true)，否则隐藏")]
        public GameObject rangedWeaponObject;

        [Tooltip("近战攻击间隔（先预留；未来接动画/判定用）")]
        public float meleeAttackInterval = 1.2f;

        private float _shootTimer;
        private float _meleeTimer;
        private ECombatMode _mode = ECombatMode.Ranged;

        protected override void Start()
        {
            base.Start();

            // 初始武器显示状态
            ApplyWeaponVisual(_mode);
        }

        protected override void OnPlayerDetected()
        {
            if (player == null) return;

            float dist = Vector3.Distance(transform.position, player.position);

            // 1) 先根据距离决定模式（带 hysteresis 防抖）
            UpdateCombatModeByDistance(dist);

            // 2) 执行对应模式行为
            if (_mode == ECombatMode.Melee)
            {
                DoMelee(dist);
            }
            else
            {
                DoRanged(dist);
            }
        }

        private void UpdateCombatModeByDistance(float dist)
        {
            // 防止“临界距离来回抖动”：Enter 用更小距离，Exit 用更大距离
            if (_mode == ECombatMode.Ranged)
            {
                if (dist <= meleeEnterDistance)
                {
                    SetMode(ECombatMode.Melee);
                }
            }
            else // Melee
            {
                if (dist >= meleeExitDistance)
                {
                    SetMode(ECombatMode.Ranged);
                }
            }
        }

        private void SetMode(ECombatMode newMode)
        {
            if (_mode == newMode) return;
            _mode = newMode;

            // 切换形态时，重置计时，避免刚切过去就立刻开枪/攻击
            _shootTimer = 0f;
            _meleeTimer = 0f;

            ApplyWeaponVisual(_mode);

            // 这里预留：未来你可以做形态切换动画
            // e.g. animator.SetTrigger("ToMelee") / animator.SetTrigger("ToRanged");
            OnModeChanged(newMode);
        }

        /// <summary>
        /// 预留给未来加动画/音效/特效用
        /// </summary>
        protected virtual void OnModeChanged(ECombatMode newMode)
        {
            // TODO: add animation hooks here in the future
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
            // 近战逻辑：先简单做成“接近并停下”，未来在这里接动画/伤害判定
            // 如果还没真的贴到（比如你 meleeEnterDistance 设置得稍大），也可以继续追一下
            if (dist > meleeEnterDistance)
            {
                TrySetDestination(player.position);
                return;
            }

            StopAgentMovement();

            // 水平朝向玩家（让拿刀/挥砍方向正确）
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

            // 预留：近战攻击节奏（目前不做伤害，只留触发点）
            _meleeTimer += Time.deltaTime;
            if (_meleeTimer >= meleeAttackInterval)
            {
                _meleeTimer = 0f;
                PerformMeleeAttackPlaceholder();
            }
        }

        /// <summary>
        /// 近战攻击预留钩子：未来你可以在这里触发动画、再由动画事件做伤害判定
        /// </summary>
        protected virtual void PerformMeleeAttackPlaceholder()
        {
            // TODO:
            // 1) animator.SetTrigger("MeleeAttack");
            // 2) 或在这里做一次 OverlapSphere / Raycast 造成伤害
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
        }
    }
}