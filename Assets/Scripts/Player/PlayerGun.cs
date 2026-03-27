using System;
using UnityEngine;

namespace Player
{
    public class PlayerGun : MonoBehaviour
    {
        public InputController inputController;
        public PlayerCameraDetector cameraDetector;
        public string bulletPoolKey = "PlayerBullet";
        public float bulletSpeed;
        

        private void OnEnable()
        {
            if (inputController != null)
                inputController.OnAttackInputChanged += OnAttack;
        }

        private void OnDisable()
        {
            if (inputController != null)
                inputController.OnAttackInputChanged -= OnAttack;
        }


        private void OnAttack()
        {
            if (cameraDetector != null)
            {
                Vector3 hitPoint = cameraDetector.DetectAimPosition();
                GameObject bullet = ObjectPoolManager.Instance.Get(bulletPoolKey, transform.position, Quaternion.identity);
                PlayerBullet bulletScript = bullet.GetComponent<PlayerBullet>();
                if (bulletScript == null)
                    return;
                Vector3 velocity = (hitPoint - transform.position).normalized * bulletSpeed;
                bulletScript.Init(velocity);
            }
            else
            {
                Debug.LogWarning("[PlayerGun] No PlayerCameraDetector found!");
            }
        }
    }
}
