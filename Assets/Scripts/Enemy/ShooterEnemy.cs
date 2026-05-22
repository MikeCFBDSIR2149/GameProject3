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
        public float aimConeAngle = 8f;          // 散射半角（度）
        public bool horizontalOnly = false; 
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
            isPaused = (status == EGameplayStatus.Paused || status == EGameplayStatus.GameOver);

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

            shootTimer += Time.deltaTime;
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

            Vector3 baseDir = (player.position - firePoint.position).normalized;

            // 在 baseDir 周围做随机偏移
            Vector3 shootDir = GetRandomDirectionInCone(baseDir, aimConeAngle, horizontalOnly);

            Vector3 velocity = shootDir * bulletSpeed;

            bulletScript.Init(velocity);
            bulletScript.SetSender(this);
        }
        private Vector3 GetRandomDirectionInCone(Vector3 forward, float coneHalfAngleDeg, bool horizontalOnly)
        {
            if (forward.sqrMagnitude < 1e-6f) forward = transform.forward;

            if (horizontalOnly)
            {
                // 只在水平面散射：把 forward 投影到XZ
                forward.y = 0f;
                if (forward.sqrMagnitude < 1e-6f) forward = transform.forward;
                forward.Normalize();

                float angle = Random.Range(-coneHalfAngleDeg, coneHalfAngleDeg);
                // 绕Y轴偏转
                Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
                return (rot * forward).normalized;
            }
            else
            {
                // 3D圆锥散射：随机一个轴向角度 + 随机一个圆周角
                float coneRad = coneHalfAngleDeg * Mathf.Deg2Rad;

                // 关键：为了“均匀”分布在圆锥内，用 cos(theta) 插值
                float u = Random.value;
                float v = Random.value;

                float cosTheta = Mathf.Lerp(1f, Mathf.Cos(coneRad), u);
                float sinTheta = Mathf.Sqrt(1f - cosTheta * cosTheta);
                float phi = 2f * Mathf.PI * v;

                // 在局部空间（forward=Z轴）构造方向
                Vector3 localDir = new Vector3(
                    sinTheta * Mathf.Cos(phi),
                    sinTheta * Mathf.Sin(phi),
                    cosTheta
                );

                // 把局部Z轴对齐到 forward
                Quaternion toForward = Quaternion.FromToRotation(Vector3.forward, forward.normalized);
                return (toForward * localDir).normalized;
            }
        }
    }
}