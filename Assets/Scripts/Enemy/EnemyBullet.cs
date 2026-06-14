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
        [SerializeField] private float damage = 10f;

        public ISender Sender { get; set; }

        private float _lifeTimer;
        private BulletTrail _bulletTrail;

        private void Awake()
        {
            TryGetComponent(out _bulletTrail);
        }

        private void OnEnable()
        {
            // 复用对象时重置计时
            _lifeTimer = 0f;

            // 保险：避免上一次的速度残留
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            _bulletTrail?.StopTrail();
            _bulletTrail?.ResetTrail();
        }

        private void OnDisable()
        {
            _bulletTrail?.StopTrail();
            _bulletTrail?.ResetTrail();
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

            _bulletTrail?.ResetTrail();
            _bulletTrail?.StartTrail();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null) return;

            // 命中玩家：造成伤害并回收
            if (other.CompareTag("Player"))
            {
                other.GetComponentInParent<IDamageable>()?.TakeDamage(damage);
                ReturnToPool();
                return;
            }

            // 命中墙：直接回收
            if (other.CompareTag("Wall"))
            {
                ReturnToPool();
                return;
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