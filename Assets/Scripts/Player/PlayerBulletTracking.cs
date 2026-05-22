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

        protected override void Awake()
        {
            base.Awake();
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
            BulletTrailComponent?.StopTrail();
            BulletTrailComponent?.ResetTrail();
            SetFrozenState(true);
        }

        public void Launch(float bulletSpeed)
        {
            if (!_isPrepared)
                return;

            if (!HasValidTarget())
            {
                ObjectPoolManager.Instance.Dispose(referencePoolKey, gameObject);
                return;
            }

            Debug.Log("New Player Bullet Tracking!!");
            SetFrozenState(false);
            Vector3 velocity = (_target.GetWorldPosition() - transform.position).normalized * bulletSpeed;
            Init(velocity);
            BulletTrailComponent?.ResetTrail();
            BulletTrailComponent?.StartTrail();
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
            if (!HasValidTarget())
            {
                ObjectPoolManager.Instance.Dispose(referencePoolKey, gameObject);
            }
            else
            {
                // Debug.Log("Tracking");
                rb.linearVelocity = (_target.GetWorldPosition() - transform.position).normalized * rb.linearVelocity.magnitude;
            }
        }

        private bool HasValidTarget()
        {
            return _target != null && _target.IsAlive;
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
            BulletTrailComponent?.StopTrail();
            BulletTrailComponent?.ResetTrail();
            _isPrepared = false;
            _isLaunched = false;
            _target = null;
        }
    }
}
