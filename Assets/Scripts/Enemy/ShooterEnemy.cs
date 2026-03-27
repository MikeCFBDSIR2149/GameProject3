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

        protected override void OnPlayerDetected()
        {
            if (player == null) return;

            float distance = Vector3.Distance(transform.position, player.position);
            if (distance > shootDistance)
            {
                agent.SetDestination(player.position);
            }
            else
            {
                agent.SetDestination(transform.position); // 停止移动
                transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z)); // 水平朝向主角

                shootTimer += Time.deltaTime;
                if (shootTimer >= shootInterval)
                {
                    Shoot();
                    shootTimer = 0f;
                }
            }
        }

        private void Shoot()
        {
            if (firePoint != null)
            {
                GameObject bullet = ObjectPoolManager.Instance.Get(bulletPoolKey, transform.position, Quaternion.identity);
                EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
                if (bulletScript == null)
                    return;
                Vector3 velocity = (player.position - firePoint.position).normalized * bulletSpeed;
                bulletScript.Init(velocity);
                // Set Sender
                bulletScript.SetSender(this);
            }
            else
            {
                Debug.LogWarning("[PlayerGun] No PlayerCameraDetector found!");
            }
        }
    }
}
