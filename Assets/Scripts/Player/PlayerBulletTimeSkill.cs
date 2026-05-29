using UnityEngine;
using System.Collections;

namespace Player
{
    public class PlayerBulletTimeSkill : MonoBehaviour
    {
        [Header("Input")]
        public InputController inputController;

        [Header("Optional Duration Limit (set <=0 to ignore)")]
        public float bulletTimeDuration = 0f;   // 设为 0 表示不使用固定时长，只由能量决定

        [Header("Energy")]
        public BulletTimeEnergy energy;         // 新增：拖拽同物体上的 BulletTimeEnergy

        protected bool _isBulletTimeActive;
        protected Coroutine _bulletTimeCoroutine;
        protected bool _isPaused;

        protected void OnEnable()
        {
            if (inputController != null)
                inputController.OnBulletTimeSkillInputChanged += TryActivateBulletTime;

            if (GameplayManager.Instance != null)
                GameplayManager.Instance.OnStatusChanged += OnGameplayStatusChanged;

            if (energy != null)
                energy.OnDepleted += HandleEnergyDepleted;
        }

        protected void OnDisable()
        {
            if (inputController != null)
                inputController.OnBulletTimeSkillInputChanged -= TryActivateBulletTime;

            if (GameplayManager.Instance != null)
                GameplayManager.Instance.OnStatusChanged -= OnGameplayStatusChanged;

            if (energy != null)
                energy.OnDepleted -= HandleEnergyDepleted;

            AbortBulletTime();
        }

        private void OnGameplayStatusChanged(EGameplayStatus status)
        {
            if (GameplayManager.Instance != null && GameplayManager.Instance.IsTerminalState)
            {
                AbortBulletTime();
                return;
            }

            _isPaused = (status == EGameplayStatus.Paused);
        }

        private void HandleEnergyDepleted()
        {
            // 能量耗尽 -> 如果正在子弹时间，强制关闭
            if (_isBulletTimeActive)
            {
                AbortBulletTime();
                if (GameplayManager.Instance != null)
                    GameplayManager.Instance.SetGameplayStatus(EGameplayStatus.Default);
            }
        }

        protected virtual void TryActivateBulletTime()
        {
            if (GameplayManager.Instance != null && !GameplayManager.Instance.CanPerformGameplayActions)
                return;

            // 再按一次：手动关闭
            if (_isBulletTimeActive)
            {
                AbortBulletTime();
                if (GameplayManager.Instance != null)
                    GameplayManager.Instance.SetGameplayStatus(EGameplayStatus.Default);
                return;
            }

            // 尝试开启：先检查能量脚本
            if (energy != null)
            {
                if (!energy.CanStartBulletTime) return;
                if (!energy.TrySpendStartCost()) return;
            }

            _bulletTimeCoroutine = StartCoroutine(BulletTimeRoutine());
        }

        protected IEnumerator BulletTimeRoutine()
        {
            _isBulletTimeActive = true;
            GameplayManager.Instance.SetGameplayStatus(EGameplayStatus.BulletTime);

            // 如果你还想保留“最长持续时间”，就用这个；否则 duration <= 0 就一直等能量耗尽或手动关闭
            float elapsed = 0f;

            while (true)
            {
                while (_isPaused) yield return null;

                if (bulletTimeDuration > 0f)
                {
                    elapsed += Time.unscaledDeltaTime;
                    if (elapsed >= bulletTimeDuration)
                        break;
                }

                // 如果能量脚本存在且已经不能继续（比如 0 或锁定），也退出
                if (energy != null && !energy.CanStartBulletTime && energy.CurrentEnergy <= 0f)
                    break;

                yield return null;
            }

            GameplayManager.Instance.SetGameplayStatus(EGameplayStatus.Default);
            _isBulletTimeActive = false;
            _bulletTimeCoroutine = null;
        }

        protected void AbortBulletTime()
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