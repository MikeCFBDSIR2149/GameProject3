using CharacterUniversal;
using UnityEngine;

namespace Player
{
    public class PlayerBullet : MonoBehaviour
    {
        public Rigidbody rb;
        public string referencePoolKey;
        private BulletTrail _bulletTrail;

        protected BulletTrail BulletTrailComponent => _bulletTrail;

        protected virtual void Awake()
        {
            TryGetComponent(out _bulletTrail);
        }

        private void OnEnable()
        {
            if (rb)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            BulletTrailComponent?.StopTrail();
            BulletTrailComponent?.ResetTrail();
        }

        public void Init(Vector3 velocity)
        {
            if (rb)
            {
                rb.linearVelocity = velocity;
            }

            BulletTrailComponent?.ResetTrail();
            BulletTrailComponent?.StartTrail();
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.GetComponent<PlayerBullet>())
            {
                return;
            }
            if (!other.gameObject.CompareTag("Player"))
            {
                ObjectPoolManager.Instance.Dispose(referencePoolKey, gameObject);
            }
        }
    }
}
