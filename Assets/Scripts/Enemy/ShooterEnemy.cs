using UnityEngine;

namespace Enemy
{
    public class ShooterEnemy : EnemyController
    {
        public float shootDistance = 8f;
        public float shootInterval = 1.5f;
        public GameObject bulletPrefab;
        public Transform firePoint;

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

        void Shoot()
        {
            if (bulletPrefab != null && firePoint != null)
            {
                GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
                Bullet bullet = bulletObj.GetComponent<Bullet>();
                if (bullet != null)
                {
                    //Debug.Log($"Bullet direction set to: {player.position}");
                    bullet.SetDirection(player.position);
                }
            }
        }
    }
}
