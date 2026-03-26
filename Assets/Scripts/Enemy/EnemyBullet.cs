using UnityEngine;

namespace Enemy
{
    public class EnemyBullet : MonoBehaviour, IContainSender
    {
        public float speed = 10f;
        public float lifeTime = 5f;

        private Vector3 direction;

        public ISender Sender { get; set; }

        public void SetSender(ISender s)
        {
            Sender = s;
        }

        // 在生成子弹时调用，设置目标方向
        public void SetDirection(Vector3 targetPosition)
        {
            direction = (targetPosition - transform.position).normalized;
            // 让子弹朝向目标
            transform.forward = direction;
            //Debug.Log($"Bullet direction set to: {direction}");
        }

        private void Update()
        {
            transform.position += direction * (speed * Time.deltaTime);
            lifeTime -= Time.deltaTime;
            if (lifeTime <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // 在这里添加碰到角色后的效果，比如扣血、播放特效等
                // 例如：other.GetComponent<PlayerHealth>()?.TakeDamage(damage);

                Destroy(gameObject);
            }
        }
    }
}
