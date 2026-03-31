using UI;
using UnityEngine;

namespace Enemy
{
    public class MeleeEnemy : EnemyController
    {
        public float attackDistance = 1.5f;

        protected override void OnPlayerDetected()
        {
            if (player == null) return;

            float distance = Vector3.Distance(transform.position, player.position);

            // 远：追击（受 canMove 控制）
            if (distance > attackDistance)
            {
                TrySetDestination(player.position);
                return;
            }

            // 近：停下（后续你可以在这里加近战攻击逻辑）
            StopAgentMovement();
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