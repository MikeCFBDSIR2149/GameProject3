using CharacterUniversal;
using UnityEngine;

namespace Player
{
    public class PlayerBulletTracking : PlayerBullet
    {
        private ISender _target;
        private bool _isPrepared;
        private bool _isLaunched;
        private Collider _cachedCollider;

        private void Awake()
        {
            _cachedCollider = GetComponent<Collider>();
        }

        public void Prepare(ISender target)
        {
            if (target == null)
            {
                // TODO：如果没有追踪目标
                ObjectPoolManager.Instance.Dispose(referencePoolKey, gameObject);
                return;
            }

            _target = target;
            _isPrepared = true;
            _isLaunched = false;
            SetFrozenState(true);
        }

        public void Launch(float bulletSpeed)
        {
            if (!_isPrepared)
                return;

            if (_target == null)
            {
                ObjectPoolManager.Instance.Dispose(referencePoolKey, gameObject);
                return;
            }

            Debug.Log("New Player Bullet Tracking!!");
            SetFrozenState(false);
            Vector3 velocity = (_target.GetWorldPosition() - transform.position).normalized * bulletSpeed;
            Init(velocity);
            _isLaunched = true;
        }

        private void SetFrozenState(bool frozen)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = frozen;
            }

            if (_cachedCollider == null)
                return;

            _cachedCollider.enabled = !frozen;
        }

        private void Update()
        {
            if (!_isPrepared || !_isLaunched)
                return;
            if (_target == null)
            {
                // TODO：如果没有追踪目标
                ObjectPoolManager.Instance.Dispose(referencePoolKey, gameObject);
            }
            else
            {
                // Debug.Log("Tracking");
                rb.linearVelocity = (_target.GetWorldPosition() - transform.position).normalized * rb.linearVelocity.magnitude;
            }
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (!_isLaunched)
                return;

            if (other.GetComponent<PlayerBullet>())
            {
                return;
            }
            if (!other.gameObject.CompareTag("Player"))
            {
                ObjectPoolManager.Instance.Dispose(referencePoolKey, gameObject);
                // Debug.Log($"Attack Back! {other.gameObject.name}");
            }
        }

        private void OnDisable()
        {
            _isPrepared = false;
            _isLaunched = false;
            _target = null;
        }
    }
}
