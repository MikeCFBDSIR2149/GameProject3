using CharacterUniversal;
using UnityEngine;

namespace Enemy
{
    public class EnemyBullet : MonoBehaviour, IContainSender
    {
        [Header("Physics")]
        [SerializeField] private Rigidbody rb;
        [Header("Pool")]
        [Tooltip("必须与 ObjectPool.poolKey 一致，否则无法回收到对象池")]
        public string referencePoolKey = "EnemyBullet";

        [Header("Lifetime")]
        [SerializeField] private float lifeTime = 5f;
        public ISender Sender { get; set; }

        private float _lifeTimer;

        private void OnEnable()
        {
            // 复用对象时重置计时
            _lifeTimer = 0f;

            // 保险：避免上一次的速度残留（看你需求，可保留/可清空）
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        private void Update()
        {
            // 超时回收，避免子弹飞远了永远不回池
            _lifeTimer += Time.deltaTime;
            if (_lifeTimer >= lifeTime)
            {
                ReturnToPool();
            }
        }

        public void SetSender(ISender s)
        {
            Sender = s;
        }

        public void Init(Vector3 velocity)
        {
            if (rb != null)
            {
                rb.linearVelocity = velocity;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // 这里留扩展：命中效果/伤害（你可以在这里调用 Player 的受击/扣血）
                // 比如：other.GetComponent<PlayerHealth>()?.TakeDamage(damage, Sender);

                ReturnToPool();
            }
            
        }

        private void ReturnToPool()
        {
            // 如果没填 key，降级为直接隐藏（至少别 Destroy）
            if (!string.IsNullOrEmpty(referencePoolKey) && ObjectPoolManager.Instance != null)
            {
                ObjectPoolManager.Instance.Dispose(referencePoolKey, gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}