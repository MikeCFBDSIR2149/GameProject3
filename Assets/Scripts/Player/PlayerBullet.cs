using UnityEngine;

namespace Player
{
    public class PlayerBullet : MonoBehaviour
    {
        public Rigidbody rb;

        public void Init(Vector3 velocity)
        {
            if (rb)
            {
                rb.linearVelocity = velocity;
            }
        }
    }
}
