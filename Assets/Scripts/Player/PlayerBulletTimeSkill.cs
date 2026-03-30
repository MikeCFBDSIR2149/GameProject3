using UnityEngine;
using System.Collections;

namespace Player
{
    public class PlayerBulletTimeSkill : MonoBehaviour
    {
        [Header("Bullet Time Settings")]
        public float bulletTimeDuration = 2f;
        public InputController inputController;
        private bool isBulletTimeActive = false;
        private Coroutine _bulletTimeCoroutine;

        private bool isPaused = false;

        private void OnEnable()
        {
            if (inputController != null)
                inputController.OnBulletTimeSkillInputChanged += TryActivateBulletTime;
            if (GameplayManager.Instance != null)
                GameplayManager.Instance.OnStatusChanged += OnGameplayStatusChanged;
        }

        private void OnDisable()
        {
            if (inputController != null)
                inputController.OnBulletTimeSkillInputChanged -= TryActivateBulletTime;
            if (GameplayManager.Instance != null)
                GameplayManager.Instance.OnStatusChanged -= OnGameplayStatusChanged;
        }

        private void OnGameplayStatusChanged(EGameplayStatus status)
        {
            isPaused = (status == EGameplayStatus.Paused);
        }

        private void TryActivateBulletTime()
        {
            if (!isBulletTimeActive)
            {
                _bulletTimeCoroutine = StartCoroutine(BulletTimeRoutine());
            }
        }

        private IEnumerator BulletTimeRoutine()
        {
            isBulletTimeActive = true;
            GameplayManager.Instance.SetGameplayStatus(EGameplayStatus.BulletTime);
            float elapsed = 0f;
            while (elapsed < bulletTimeDuration)
            {
                // 如果暂停，协程挂起，不累计时间
                while (isPaused)
                {
                    yield return null;
                }
                // 非暂停时累计时间
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            GameplayManager.Instance.SetGameplayStatus(EGameplayStatus.Default);
            isBulletTimeActive = false;
        }
    }
}
