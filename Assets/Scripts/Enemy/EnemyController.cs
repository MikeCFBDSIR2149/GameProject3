using UnityEngine;
using UnityEngine.AI;

namespace Enemy
{
    public class EnemyController : MonoBehaviour, ISender
    {
        public float detectRange = 10f;         // 感知主角的范围
        public float patrolRadius = 5f;         // 巡逻半径
        public float patrolWaitTime = 2f;       // 每次巡逻等待时间

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

        protected virtual void OnPlayerDetected()
        {
            // 子类重写：靠近或攻击主角
            agent.SetDestination(player.position);
        }

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
                agent.SetDestination(patrolTarget);
            }
        }

        void SetNewPatrolTarget()
        {
            Vector2 randomPoint = Random.insideUnitCircle * patrolRadius;
            patrolTarget = patrolCenter + new Vector3(randomPoint.x, 0, randomPoint.y);
        }

        public Vector3 GetWorldPosition()
        {
            return transform.position;
        }
    }
}
