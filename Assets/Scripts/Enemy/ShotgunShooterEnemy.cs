using UnityEngine;

namespace Enemy
{
    public class ShotgunShooterEnemy : EnemyController
    {
        [Header("Shoot")]
        public float shootDistance = 8f;

        [Tooltip("散弹枪射速更慢")]
        public float shootInterval = 2.5f;

        public Transform firePoint;

        [Tooltip("对象池 key，需要和 ObjectPool 的 poolKey 一致；EnemyBullet.referencePoolKey 也建议一致")]
        public string bulletPoolKey = "EnemyBullet";

        public float bulletSpeed = 10f;

        [Header("Shotgun Settings")]
        [Tooltip("一次射出的子弹数量（建议 6）")]
        public int pelletsPerShot = 6;

        [Tooltip("散射半角（度），越大越散")]
        public float spreadAngle = 18f;

        [Tooltip("只在水平面XZ散射（散弹枪常见做法）")]
        public bool horizontalOnly = true;

        private float shootTimer;
        private bool isPaused;

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
            isPaused = GameplayManager.Instance != null && !GameplayManager.Instance.CanPerformGameplayActions;

            if (isPaused && agent != null && agent.isActiveAndEnabled)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
            else if (!isPaused && agent != null && agent.isActiveAndEnabled)
            {
                agent.isStopped = false;
            }
        }

        protected override void OnPlayerDetected()
        {
            if (isPaused) return;
            if (player == null) return;

            float distance = Vector3.Distance(transform.position, player.position);

            // 远：追击
            if (distance > shootDistance)
            {
                TrySetDestination(player.position);
                return;
            }

            // 近：停下并射击
            StopAgentMovement();

            // 水平朝向玩家
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

            shootTimer += Time.deltaTime;
            if (shootTimer >= shootInterval)
            {
                ShootShotgun();
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

        private void ShootShotgun()
        {
            if (firePoint == null)
            {
                Debug.LogWarning("[ShotgunShooterEnemy] firePoint is null!");
                return;
            }

            if (ObjectPoolManager.Instance == null)
            {
                Debug.LogWarning("[ShotgunShooterEnemy] ObjectPoolManager.Instance is null!");
                return;
            }

            Vector3 baseDir = (player.position - firePoint.position).normalized;

            // 一次射多发
            int count = Mathf.Max(1, pelletsPerShot);
            for (int i = 0; i < count; i++)
            {
                GameObject bullet = ObjectPoolManager.Instance.Get(bulletPoolKey, firePoint.position, Quaternion.identity);
                if (bullet == null) continue;

                EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
                if (bulletScript == null) continue;

                Vector3 dir = GetRandomDirectionInCone(baseDir, spreadAngle, horizontalOnly);
                Vector3 velocity = dir * bulletSpeed;

                bulletScript.Init(velocity);
                bulletScript.SetSender(this);
            }
        }

        // 直接复用你 ShooterEnemy 里的算法（复制过来）
        private Vector3 GetRandomDirectionInCone(Vector3 forward, float coneHalfAngleDeg, bool horizontalOnly)
        {
            if (forward.sqrMagnitude < 1e-6f) forward = transform.forward;

            if (horizontalOnly)
            {
                forward.y = 0f;
                if (forward.sqrMagnitude < 1e-6f) forward = transform.forward;
                forward.Normalize();

                float angle = Random.Range(-coneHalfAngleDeg, coneHalfAngleDeg);
                Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
                return (rot * forward).normalized;
            }
            else
            {
                float coneRad = coneHalfAngleDeg * Mathf.Deg2Rad;

                float u = Random.value;
                float v = Random.value;

                float cosTheta = Mathf.Lerp(1f, Mathf.Cos(coneRad), u);
                float sinTheta = Mathf.Sqrt(1f - cosTheta * cosTheta);
                float phi = 2f * Mathf.PI * v;

                Vector3 localDir = new Vector3(
                    sinTheta * Mathf.Cos(phi),
                    sinTheta * Mathf.Sin(phi),
                    cosTheta
                );

                Quaternion toForward = Quaternion.FromToRotation(Vector3.forward, forward.normalized);
                return (toForward * localDir).normalized;
            }
        }
    }
}