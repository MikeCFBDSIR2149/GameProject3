using UnityEngine;

namespace Enemy
{
    public class EnemyBullet : MonoBehaviour, IContainSender
    {
        private Vector3 direction;
        public Rigidbody rb;
        public string referencePoolKey;
        
        public ISender Sender { get; set; }

        public void SetSender(ISender s)
        {
            Sender = s;
        }

        public void Init(Vector3 velocity)
        {
            if (rb)
            {
                rb.linearVelocity = velocity;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Destroy(gameObject);
            }
        }
    }
}
