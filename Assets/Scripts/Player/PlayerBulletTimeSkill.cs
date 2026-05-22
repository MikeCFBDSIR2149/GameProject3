using UnityEngine;
using System.Collections;

namespace Player
{
    public class PlayerBulletTimeSkill : MonoBehaviour
    {
        [Header("Bullet Time Settings")]
        public float bulletTimeDuration = 2f;
        public InputController inputController;
        private bool _isBulletTimeActive;
        private Coroutine _bulletTimeCoroutine;

        private bool _isPaused;

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

            AbortBulletTime();
        }

        private void OnGameplayStatusChanged(EGameplayStatus status)
        {
            if (status == EGameplayStatus.GameOver)
            {
                AbortBulletTime();
                return;
            }

            _isPaused = (status == EGameplayStatus.Paused);
        }

        private void TryActivateBulletTime()
        {
            if (GameplayManager.Instance != null && GameplayManager.Instance.Status == EGameplayStatus.GameOver)
                return;

            if (!_isBulletTimeActive)
            {
                _bulletTimeCoroutine = StartCoroutine(BulletTimeRoutine());
            }
        }

        private IEnumerator BulletTimeRoutine()
        {
            _isBulletTimeActive = true;
            GameplayManager.Instance.SetGameplayStatus(EGameplayStatus.BulletTime);
            float elapsed = 0f;
            while (elapsed < bulletTimeDuration)
            {
                // 如果暂停，协程挂起，不累计时间
                while (_isPaused)
                {
                    yield return null;
                }
                // 非暂停时累计时间
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            GameplayManager.Instance.SetGameplayStatus(EGameplayStatus.Default);
            _isBulletTimeActive = false;
            _bulletTimeCoroutine = null;
        }

        private void AbortBulletTime()
        {
            if (_bulletTimeCoroutine != null)
            {
                StopCoroutine(_bulletTimeCoroutine);
                _bulletTimeCoroutine = null;
            }

            _isBulletTimeActive = false;
            _isPaused = false;
        }
    }
}
