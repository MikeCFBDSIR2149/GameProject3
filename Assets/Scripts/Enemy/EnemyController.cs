using UnityEngine;
using UnityEngine.AI;

namespace Enemy
{
    public class EnemyController : MonoBehaviour, ISender
    {
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

        protected virtual void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

            patrolCenter = transform.position;
            SetNewPatrolTarget();

            // 初始化时根据 canMove 同步 agent 状态
            ApplyMoveState();
        }

        protected virtual void Update()
        {
            if (player != null && Vector3.Distance(transform.position, player.position) < detectRange)
            {
                OnPlayerDetected();
            }
            else
            {
                Patrol();
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
    }
}