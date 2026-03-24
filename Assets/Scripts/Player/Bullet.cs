using UnityEngine;

namespace Player
{
    public class Bullet : MonoBehaviour
    {
        public Rigidbody rb;
        public string referencePoolKey;

        public void Init(Vector3 velocity)
        {
            if (rb)
            {
                rb.linearVelocity = velocity;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag("Player"))
            {
                ObjectPoolManager.Instance.Dispose(referencePoolKey, gameObject);
            }
        }
    }
}
