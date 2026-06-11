using System.Collections;
using CharacterUniversal;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy
{
    public class EnemyController : MonoBehaviour, ISender
    {
        [Header("Health (optional)")]
        [SerializeField] private bool destroyOnDeath = false; // Destroy 或 SetActive(false)
        private Health _health;
        private bool _isDead;

        [Header("Perception")]
        public float detectRange = 10f;         // 感知主角的范围

        [Header("Patrol")]
        public float patrolRadius = 5f;         // 巡逻半径
        public float patrolWaitTime = 2f;       // 每次巡逻等待时间

        [Header("Movement")]
        [SerializeField] protected bool canMove = true;

        protected Transform player;
        protected NavMeshAgent agent;

        protected Vector3 patrolCenter;
        protected Vector3 patrolTarget;
        protected float patrolTimer;

        // =========================
        // Animation (optional)
        // =========================
        [Header("Animation (optional)")]
        [Tooltip("可在 Inspector 手动拖，也可勾 autoFindAnimator 自动在子物体中查找")]
        [SerializeField] protected Animator animator;

        [SerializeField] private bool autoFindAnimator = true;

        [Header("Death Animation (optional)")]
        [Tooltip("开启后：死亡时先触发 Die，再等待 deathAnimWaitTime 后再消失（用于播放死亡动画）。不开启则保持原逻辑：立刻消失")]
        [SerializeField] private bool useDeathAnimation = true;

        [Tooltip("死亡动画等待时间（按你的动画长度调）")]
        [SerializeField] private float deathAnimWaitTime = 1.2f;

        // 参数名约定（统一全敌人 Animator Controller 参数）
        protected static readonly int AnimIsMoving = Animator.StringToHash("IsMoving");
        protected static readonly int AnimMoveSpeed = Animator.StringToHash("MoveSpeed");
        protected static readonly int AnimDie = Animator.StringToHash("Die");
        protected static readonly int AnimShoot = Animator.StringToHash("Shoot");

        // 预留：未来近战/技能等
        protected static readonly int AnimMelee = Animator.StringToHash("Melee");

        private Coroutine _deathRoutine;

        protected virtual void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

            patrolCenter = transform.position;
            SetNewPatrolTarget();

            // 初始化时根据 canMove 同步 agent 状态
            ApplyMoveState();

            // 读取可选血量脚本
            _health = GetComponent<Health>();

            // 动画：自动找 Animator（常见是模型在子物体）
            if (autoFindAnimator && animator == null)
                animator = GetComponentInChildren<Animator>();
        }

        protected virtual void LateUpdate()
        {
            UpdateMoveAnim();
        }

        protected virtual void UpdateMoveAnim()
        {
            if (animator == null) return;

            float speed = 0f;

            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh && !agent.isStopped)
                speed = agent.velocity.magnitude;

            animator.SetBool(AnimIsMoving, speed > 0.05f);
            animator.SetFloat(AnimMoveSpeed, speed);
        }

        protected virtual void Update()
        {
            if (CheckAndHandleDeath()) return;

            if (player != null && Vector3.Distance(transform.position, player.position) < detectRange)
            {
                OnPlayerDetected();
            }
            else
            {
                Patrol();
            }
        }

        protected virtual bool CheckAndHandleDeath()
        {
            if (_isDead) return true;

            if (_health != null && _health.CurrentHealth <= 0f)
            {
                Die();
                return true;
            }

            return false;
        }

        protected virtual void Die()
        {
            if (_isDead) return;
            _isDead = true;

            // 停止移动
            if (agent && agent.isActiveAndEnabled)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }

            // 有动画则优先播动画再消失；没动画则保持原逻辑立刻消失
            bool canPlayDeathAnim = (useDeathAnimation && animator != null && deathAnimWaitTime > 0.01f);

            if (canPlayDeathAnim)
            {
                // 触发死亡动画
                animator.ResetTrigger(AnimShoot);
                animator.ResetTrigger(AnimMelee);
                animator.SetTrigger(AnimDie);

                // 延迟消失（避免立刻 Destroy/Disable 导致看不到动画）
                if (_deathRoutine != null) StopCoroutine(_deathRoutine);
                _deathRoutine = StartCoroutine(DieRoutine(deathAnimWaitTime));
                return;
            }

            // 原有功能：立刻消失
            DisappearNow();
        }

        private IEnumerator DieRoutine(float waitTime)
        {
            // 等待死亡动画播完
            yield return new WaitForSeconds(waitTime);

            DisappearNow();
        }

        private void DisappearNow()
        {
            // 你原有“消失”：Destroy 或 SetActive(false)
            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 外部/子类都可以调用，控制该敌人是否允许移动
        /// </summary>
        public virtual void SetCanMove(bool value)
        {
            canMove = value;
            ApplyMoveState();
        }

        /// <summary>
        /// 把 canMove 的状态真正作用到 NavMeshAgent 上（停止/恢复）
        /// </summary>
        protected virtual void ApplyMoveState()
        {
            if (agent == null) return;

            if (!canMove)
            {
                // 立刻停下，避免沿着旧路径继续走
                if (agent.isActiveAndEnabled)
                {
                    agent.isStopped = true;
                    agent.ResetPath();
                }
            }
            else
            {
                // 恢复移动能力（注意：恢复后是否移动，取决于后续是否调用 SetDestination）
                if (agent.isActiveAndEnabled)
                {
                    agent.isStopped = false;
                }
            }
        }

        /// <summary>
        /// 统一移动入口：所有 SetDestination 都应该通过它进行
        /// </summary>
        protected bool TrySetDestination(Vector3 dest)
        {
            if (!canMove) return false;
            if (agent == null) return false;
            if (!agent.isActiveAndEnabled) return false;

            // agent 不在 NavMesh 上时调用 SetDestination 会报错
            if (!agent.isOnNavMesh) return false;

            // 如果之前被停止过，恢复
            if (agent.isStopped) agent.isStopped = false;

            agent.SetDestination(dest);
            return true;
        }

        /// <summary>
        /// 感知到玩家：默认行为是追玩家（子类可重写）
        /// </summary>
        protected virtual void OnPlayerDetected()
        {
            if (player == null) return;
            TrySetDestination(player.position);
        }

        /// <summary>
        /// 巡逻逻辑（子类可重写）
        /// </summary>
        protected virtual void Patrol()
        {
            if (Vector3.Distance(transform.position, patrolTarget) < 0.5f)
            {
                patrolTimer += Time.deltaTime;
                if (patrolTimer >= patrolWaitTime)
                {
                    SetNewPatrolTarget();
                    patrolTimer = 0f;
                }
            }
            else
            {
                TrySetDestination(patrolTarget);
            }
        }

        protected virtual void SetNewPatrolTarget()
        {
            Vector2 randomPoint = Random.insideUnitCircle * patrolRadius;
            patrolTarget = patrolCenter + new Vector3(randomPoint.x, 0f, randomPoint.y);
        }

        public Vector3 GetWorldPosition()
        {
            return transform.position;
        }

        public bool IsAlive => !_isDead && this != null;

        // =========================
        // Animation trigger points (for subclasses)
        // =========================

        /// <summary>
        /// 子类在“真正射出子弹的瞬间”调用它，触发射击动画。没配 Animator 不会影响任何功能。
        /// </summary>
        protected void TriggerShootAnim()
        {
            if (animator == null) return;
            animator.SetTrigger(AnimShoot);
        }

        /// <summary>
        /// 预留：子类在近战攻击开始瞬间调用，触发近战动画。
        /// </summary>
        protected void TriggerMeleeAnim()
        {
            if (animator == null) return;
            animator.SetTrigger(AnimMelee);
        }

        /// <summary>
        /// 预留：你未来想用自定义Trigger名，也可以走这个通道（例如 "Reload" / "Skill"）。
        /// </summary>
        protected void TriggerAttackAnim(string triggerName)
        {
            if (animator == null) return;
            if (string.IsNullOrEmpty(triggerName)) return;
            animator.SetTrigger(triggerName);
        }
    }
}