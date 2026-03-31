using UI;
using UnityEngine;

namespace Enemy
{
    public class ShooterEnemy : EnemyController
    {
        public float shootDistance = 8f;
        public float shootInterval = 1.5f;
        public Transform firePoint;
        public string bulletPoolKey = "EnemyBullet";
        public float bulletSpeed = 10f;

        private float shootTimer = 0f;
        private bool isPaused = false;

        private void OnEnable()
        {
            if (GameplayManager.Instance != null)
                GameplayManager.Instance.OnStatusChanged += OnStatusChanged;
        }

        private void OnDisable()
        {
            if (GameplayManager.Instance != null)
                GameplayManager.Instance.OnStatusChanged -= OnStatusChanged;
        }

        private void OnStatusChanged(EGameplayStatus status)
        {
            isPaused = (status == EGameplayStatus.Paused);

            // 如果进入暂停，建议立刻停下（不然还会沿着旧路径走一段）
            if (isPaused && agent != null && agent.isActiveAndEnabled)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
            else if (!isPaused && agent != null && agent.isActiveAndEnabled)
            {
                // 恢复时不强制走，后续逻辑会 TrySetDestination
                agent.isStopped = false;
            }
        }

        protected override void OnPlayerDetected()
        {
            if (isPaused) return;
            if (player == null) return;

            float distance = Vector3.Distance(transform.position, player.position);

            // 远：追击（会受 canMove 控制）
            if (distance > shootDistance)
            {
                TrySetDestination(player.position);
                return;
            }

            // 近：停下并射击
            StopAgentMovement();

            // 水平朝向主角（不影响 canMove）
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

            shootTimer += Time.unscaledDeltaTime;
            if (shootTimer >= shootInterval)
            {
                Shoot();
                shootTimer = 0f;
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
                Debug.LogWarning("[ShooterEnemy] firePoint is null!");
                return;
            }

            GameObject bullet = ObjectPoolManager.Instance.Get(bulletPoolKey, transform.position, Quaternion.identity);
            EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
            if (bulletScript == null) return;

            // 注意：这里你用的是 firePoint.position 来算方向，但生成位置用的是 transform.position
            // 如果你希望更准确，生成位置也可以用 firePoint.position（按你项目需求来）
            Vector3 velocity = (player.position - firePoint.position).normalized * bulletSpeed;

            bulletScript.Init(velocity);
            bulletScript.SetSender(this);
        }
    }
}