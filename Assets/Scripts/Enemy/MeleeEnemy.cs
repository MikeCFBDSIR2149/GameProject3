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
            if (distance > attackDistance)
            {
                agent.SetDestination(player.position);
            }
            else
            {
                agent.SetDestination(transform.position); // 停止移动
                // 在这里可以添加近战攻击逻辑
            }
        }
    }
}
