using System.Collections.Generic;
using CharacterUniversal;
using UnityEngine;

namespace Player
{
    public class PlayerAttackBack : MonoBehaviour
    {
        public string bulletTrackingPoolKey = "PlayerBulletTracking";
        public float bulletSpeed = 20f;
        private readonly Queue<PlayerBulletTracking> _readyBulletTrackingBullets = new Queue<PlayerBulletTracking>();

        private void OnEnable()
        {
            if (GameplayManager.Instance != null)
                GameplayManager.Instance.OnStatusChanged += OnStatusChanged;
        }

        private void OnDisable()
        {
            if (GameplayManager.Instance == null)
                return;
            GameplayManager.Instance.OnStatusChanged -= OnStatusChanged;
        }

        public virtual void RegisterBulletReturn(ISender sender, Vector3 spawnPosition)
        {
            if (sender == null)
                return;

            if (GameplayManager.Instance != null && GameplayManager.Instance.Status == EGameplayStatus.GameOver)
                return;

            GameObject bullet = ObjectPoolManager.Instance.Get(bulletTrackingPoolKey, spawnPosition, Quaternion.identity);
            if (!bullet)
                return;

            PlayerBulletTracking bulletScript = bullet.GetComponent<PlayerBulletTracking>();
            if (bulletScript == null)
            {
                ObjectPoolManager.Instance.Dispose(bulletTrackingPoolKey, bullet);
                return;
            }

            bulletScript.Prepare(sender);
            _readyBulletTrackingBullets.Enqueue(bulletScript);

            if (GameplayManager.Instance != null && GameplayManager.Instance.Status == EGameplayStatus.Default)
                ReleaseAllQueuedBullets();
        }

        private void OnStatusChanged(EGameplayStatus gameplayStatus)
        {
            if (gameplayStatus == EGameplayStatus.Default && _readyBulletTrackingBullets.Count > 0)
            {
                ReleaseAllQueuedBullets();
                return;
            }

            if (gameplayStatus == EGameplayStatus.GameOver)
            {
                ClearQueuedBullets();
            }
        }

        private void ReleaseAllQueuedBullets()
        {
            while (_readyBulletTrackingBullets.Count > 0)
            {
                PlayerBulletTracking bulletScript = _readyBulletTrackingBullets.Dequeue();
                bulletScript?.Launch(bulletSpeed);
            }
        }

        private void ClearQueuedBullets()
        {
            while (_readyBulletTrackingBullets.Count > 0)
            {
                PlayerBulletTracking bulletScript = _readyBulletTrackingBullets.Dequeue();
                if (bulletScript != null)
                {
                    ObjectPoolManager.Instance.Dispose(bulletTrackingPoolKey, bulletScript.gameObject);
                }
            }
        }
    }
}
