using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class PlayerAttackBack : MonoBehaviour
    {
        public string bulletTrackingPoolKey = "PlayerBulletTracking";
        public float bulletSpeed = 20f;
        public float attackBackInterval = 0.2f; // 间隔发射时间（秒）
        private readonly Queue<ISender> readyBulletTargets = new Queue<ISender>();

        private bool isPaused = false;
        private Coroutine attackBackCoroutine;

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

        public void RegisterBulletReturn(ISender sender)
        {
            if (sender == null)
                return;
            readyBulletTargets.Enqueue(sender);
            // Debug.Log($"BulletBack Count: {readyBulletTargets.Count}");
        }

        private void OnStatusChanged(EGameplayStatus gameplayStatus)
        {
            isPaused = (gameplayStatus == EGameplayStatus.Paused);
            if (gameplayStatus == EGameplayStatus.Default && readyBulletTargets.Count > 0)
            {
                if (attackBackCoroutine == null)
                    attackBackCoroutine = StartCoroutine(AttackBackRoutine());
            }
        }

        private IEnumerator AttackBackRoutine()
        {
            while (readyBulletTargets.Count > 0)
            {
                // 暂停时协程挂起
                while (isPaused)
                {
                    yield return null;
                }
                ISender sender = readyBulletTargets.Dequeue();
                GameObject bullet =
                    ObjectPoolManager.Instance.Get(bulletTrackingPoolKey, transform.position, Quaternion.identity);
                PlayerBulletTracking bulletScript = bullet.GetComponent<PlayerBulletTracking>();
                if (bulletScript == null)
                    yield break;
                Vector3 targetPosition = sender.GetWorldPosition();
                Vector3 velocity = (targetPosition - transform.position).normalized * bulletSpeed;
                bulletScript.Init(velocity, sender);
                float elapsed = 0f;
                while (elapsed < attackBackInterval)
                {
                    while (isPaused)
                        yield return null;
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
            }
            attackBackCoroutine = null;
        }
    }
}
