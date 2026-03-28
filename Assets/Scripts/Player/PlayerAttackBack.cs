using System;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class PlayerAttackBack : MonoBehaviour
    {
        public string bulletTrackingPoolKey = "PlayerBulletTracking";
        public float bulletSpeed = 20f;
        private readonly Queue<ISender> readyBulletTargets = new Queue<ISender>();
        
        private void OnEnable()
        {
            GameplayManager.Instance.OnStatusChanged += AttackBack;
        }

        private void OnDisable()
        {
            if (GameplayManager.Instance == null)
                return;
            GameplayManager.Instance.OnStatusChanged -= AttackBack;
        }


        public void RegisterBulletReturn(ISender sender)
        {
            if (sender == null)
                return;
            readyBulletTargets.Enqueue(sender);
            Debug.Log($"BulletBack Count: {readyBulletTargets.Count}");
        }

        private void AttackBack(EGameplayStatus gameplayStatus)
        {
            if (gameplayStatus != EGameplayStatus.Default)
                return;
            while (readyBulletTargets.Count > 0)
            {
                ISender sender = readyBulletTargets.Dequeue();
                GameObject bullet =
                    ObjectPoolManager.Instance.Get(bulletTrackingPoolKey, transform.position, Quaternion.identity);
                PlayerBulletTracking bulletScript = bullet.GetComponent<PlayerBulletTracking>();
                if (bulletScript == null)
                    return;
                Vector3 targetPosition = sender.GetWorldPosition();
                Vector3 velocity = (targetPosition - transform.position).normalized * bulletSpeed;
                bulletScript.Init(velocity, sender);
            }
        }
    }
}
