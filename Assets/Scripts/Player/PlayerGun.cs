using System;
using System.Collections;
using UnityEngine;

namespace Player
{
    public class PlayerGun : MonoBehaviour
    {
        public InputController inputController;
        public PlayerCameraDetector cameraDetector;
        public string bulletPoolKey = "PlayerBullet";
        public float bulletSpeed;

        private Coroutine _attackCooldownCoroutine;
        private bool _allowAttack = true;

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

        private void OnDestroy()
        {
            if (inputController != null)
                inputController.OnAttackInputChanged -= OnAttack;
        }


        protected virtual void OnAttack()
        {
            if (!_allowAttack) return;
            if (cameraDetector != null)
            {
                Vector3 hitPoint = cameraDetector.DetectAimPosition();
                GameObject bullet = ObjectPoolManager.Instance.Get(bulletPoolKey, transform.position, Quaternion.identity);
                PlayerBullet bulletScript = bullet.GetComponent<PlayerBullet>();
                if (bulletScript == null)
                    return;
                Vector3 velocity = (hitPoint - transform.position).normalized * bulletSpeed;
                bulletScript.Init(velocity);
                if (_attackCooldownCoroutine != null)
                {
                    StopCoroutine(_attackCooldownCoroutine);
                }
                _attackCooldownCoroutine = StartCoroutine(CooldownCoroutine());
            }
            else
            {
                Debug.LogWarning("[PlayerGun] No PlayerCameraDetector found!");
            }
        }
        
        private IEnumerator CooldownCoroutine()
        {
            _allowAttack = false;
            yield return new WaitForSecondsRealtime(0.5f);
            _allowAttack = true;
        }
    }
}
