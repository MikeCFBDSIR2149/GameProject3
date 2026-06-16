using System.Collections;
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
        
        private Coroutine _lifeCycleCoroutine;

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
            
            if (_lifeCycleCoroutine != null)
            {
                _lifeCycleCoroutine = null;
                StopCoroutine(_lifeCycleCoroutine);
            }
            _lifeCycleCoroutine = StartCoroutine(LifeCycleCoroutine());
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
                _lifeCycleCoroutine = null;
            }
        }

        private IEnumerator LifeCycleCoroutine()
        {
            yield return new WaitForSeconds(5f);
            ObjectPoolManager.Instance.Dispose(referencePoolKey, gameObject);
            _lifeCycleCoroutine = null;
        }
    }
}
