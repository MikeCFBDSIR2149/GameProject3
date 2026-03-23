using UI;
using UnityEngine;

namespace Enemy
{
    public class Bullet : MonoBehaviour
    {
        
        public float speed = 10f;
        public float lifeTime = 5f;

        private Vector3 direction;

        // 在生成子弹时调用，设置目标方向
        public void SetDirection(Vector3 targetPosition)
        {
            direction = (targetPosition - transform.position).normalized;
            // 让子弹朝向目标
            transform.forward = direction;
            //Debug.Log($"Bullet direction set to: {direction}");
        }

        void Update()
        {
            transform.position += direction * speed * Time.deltaTime;
            lifeTime -= Time.deltaTime;
            if (lifeTime <= 0f)
            {
                Destroy(gameObject);
            }
        }
        void OnTriggerEnter(Collider other)
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
