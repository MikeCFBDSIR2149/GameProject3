using System;
using UnityEngine;

namespace Player
{
    public class PlayerBulletTracking : PlayerBullet
    {
        private ISender _target;
        private bool isInitialized;

        public void Init(Vector3 velocity, ISender target)
        {
            base.Init(velocity);
            if (target == null)
            {
                // TODO：如果没有追踪目标
                ObjectPoolManager.Instance.Dispose(referencePoolKey, gameObject);
            }
            Debug.Log("New Player Bullet Tracking!!");
            _target = target;
            isInitialized = true;
        }

        private void Update()
        {
            if (!isInitialized)
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
            if (other.gameObject.GetComponent<PlayerBullet>())
            {
                return;
            }
            if (!other.gameObject.CompareTag("Player"))
            {
                ObjectPoolManager.Instance.Dispose(referencePoolKey, gameObject);
                Debug.Log($"Attack Back! {other.gameObject.name}");
            }
        }
    }
}
