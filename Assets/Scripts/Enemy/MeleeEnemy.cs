using System.Collections;
using UI;
using UnityEngine;

namespace Enemy
{
    /// <summary>
    /// MeleeEnemy：靠近时触发近战攻击动画并开启近战判定盒（MeleeAttackHitbox）。
    /// - 支持通过动画事件精确 Arm/Disarm（AnimArm / AnimDisarm）
    /// - 如果不使用动画事件，脚本内部也会按 timing 自动 Arm/Disarm
    /// </summary>
    public class MeleeEnemy : EnemyController
    {
        [Header("Melee")]
        public float attackDistance = 1.5f;

        [Tooltip("两次近战攻击之间的最小间隔（包括前摇 + 有效帧 + 冷却）")]
        public float attackInterval = 1.5f;

        [Tooltip("攻击前摇（如果使用动画事件，可设为 0 并在动画里调用 AnimArm）")]
        public float attackWindup = 0.25f;

        [Tooltip("命中判定（hitbox）保持的时间；如果使用动画事件可设为 0")]
        public float attackActiveTime = 0.2f;

        [Tooltip("攻击后冷却（在 attackInterval 中已包含）")]
        public float attackCooldown = 1.05f;

        [Tooltip("如果 true，则在播放动画时使用 Animation Events 来调用 AnimArm/AnimDisarm；否则使用脚本计时来控制判定")]
        public bool useAnimationEvents = true;

        // 可在 Inspector 指定具体的判定盒（也会自动在子对象里查找）
        public Enemy.Melee.MeleeAttackHitbox meleeHitbox;

        private Coroutine _attackRoutine;
        private bool _isPaused = false;
        private float _lastAttackTime = -999f;

        private void Awake()
        {
            // 自动查找 hitbox（如果没有手动指定）
            if (meleeHitbox == null)
            {
                meleeHitbox = GetComponentInChildren<Enemy.Melee.MeleeAttackHitbox>(true);
            }

            // 确保判定盒默认被禁用（MeleeAttackHitbox.Arm/Disarm 会控制）
            if (meleeHitbox != null)
            {
                meleeHitbox.Disarm();
            }
        }

        private void OnEnable()
        {
            // 与其他 Enemy 一样订阅暂停事件（保持一致行为）
            if (GameplayManager.Instance != null)
                GameplayManager.Instance.OnStatusChanged += OnStatusChanged;
        }

        private void OnDisable()
        {
            if (GameplayManager.Instance != null)
                GameplayManager.Instance.OnStatusChanged -= OnStatusChanged;

            // 停止任何攻击协程并确保判定盒被禁用
            if (_attackRoutine != null)
            {
                StopCoroutine(_attackRoutine);
                _attackRoutine = null;
            }

            if (meleeHitbox != null)
            {
                meleeHitbox.Disarm();
            }
        }

        private void OnStatusChanged(EGameplayStatus status)
        {
            _isPaused = GameplayManager.Instance != null && !GameplayManager.Instance.CanPerformGameplayActions;

            if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
                return;

            if (_isPaused)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
            else
            {
                agent.isStopped = false;
            }
        }

        protected override void OnPlayerDetected()
        {
            if (_isPaused) return;
            if (player == null) return;

            float distance = Vector3.Distance(transform.position, player.position);

            // 远：追击（受 canMove 控制）
            if (distance > attackDistance)
            {
                // 如果玩家走出攻击范围，中断正在进行的攻击并收起判定盒
                if (_attackRoutine != null)
                {
                    StopCoroutine(_attackRoutine);
                    _attackRoutine = null;
                    if (meleeHitbox != null) meleeHitbox.Disarm();
                }

                TrySetDestination(player.position);
                return;
            }

            // 近：停下并发起攻击（如果冷却已到）
            StopAgentMovement();

            // 水平朝向玩家（不改变 Y）
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

            if (Time.time - _lastAttackTime >= attackInterval && _attackRoutine == null)
            {
                _attackRoutine = StartCoroutine(AttackRoutine());
            }
        }

        private IEnumerator AttackRoutine()
        {
            _lastAttackTime = Time.time;

            // Trigger 动画（使用父类接口）
            TriggerMeleeAnim();

            // 如果你使用 Animation Events（更精确）：动画会在合适帧调用 AnimArm/AnimDisarm。
            if (useAnimationEvents)
            {
                // 仅等待动画播放长度或简单等待前摇（避免立刻开始下一次）
                // 这里我们等待 attackInterval 以保证不会重复触发
                yield return new WaitForSeconds(attackInterval);
            }
            else
            {
                // 脚本控制时序：前摇 -> 激活判定 -> 等待有效帧 -> 结束判定 -> 冷却
                if (attackWindup > 0f)
                    yield return new WaitForSeconds(attackWindup);

                if (meleeHitbox != null)
                    meleeHitbox.Arm();

                if (attackActiveTime > 0f)
                    yield return new WaitForSeconds(attackActiveTime);

                if (meleeHitbox != null)
                    meleeHitbox.Disarm();

                // 冷却（确保总时长 >= attackInterval）
                float elapsed = Time.time - _lastAttackTime;
                float remaining = Mathf.Max(0f, attackInterval - elapsed);
                if (remaining > 0f)
                    yield return new WaitForSeconds(remaining);
            }

            _attackRoutine = null;
        }

        /// <summary>
        /// 供 Animation Event 调用：在动画那一帧开启命中判定
        /// （在 Animator 的攻击动画上添加 Event，函数名写 AnimArm）
        /// </summary>
        public void AnimArm()
        {
            if (meleeHitbox != null)
            {
                meleeHitbox.Arm();
            }
        }

        /// <summary>
        /// 供 Animation Event 调用：在动画那一帧关闭命中判定
        /// （在 Animator 的攻击动画上添加 Event，函数名写 AnimDisarm）
        /// </summary>
        public void AnimDisarm()
        {
            if (meleeHitbox != null)
            {
                meleeHitbox.Disarm();
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
    }
}
