using UnityEngine;
using System.Collections;

namespace Player
{
    public class PlayerBulletTimeSkill : MonoBehaviour
    {
        [Header("Input")]
        public InputController inputController;

        [Header("Optional Duration Limit (set <=0 to ignore)")]
        public float bulletTimeDuration = 0f;

        [Header("Energy")]
        public BulletTimeEnergy energy;

        protected bool _isBulletTimeActive;
        protected Coroutine _bulletTimeCoroutine;
        protected bool _isPaused;

        private void Awake()
        {
            if (energy == null)
                energy = GetComponent<BulletTimeEnergy>();
        }

        protected void OnEnable()
        {
            if (energy == null)
                energy = GetComponent<BulletTimeEnergy>();

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
            // 能量归零时，如果正在子弹时间，强制结束
            if (!_isBulletTimeActive)
                return;

            AbortBulletTime();

            if (GameplayManager.Instance != null)
                GameplayManager.Instance.SetGameplayStatus(EGameplayStatus.Default, true);
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
                    GameplayManager.Instance.SetGameplayStatus(EGameplayStatus.Default, true);

                return;
            }

            if (energy == null)
                energy = GetComponent<BulletTimeEnergy>();

            // 训练模式不引入子弹时间限制
            if (energy == null)
            {
                _bulletTimeCoroutine = StartCoroutine(BulletTimeRoutine());
                return;
            }

            if (!energy.CanStartBulletTime)
                return;

            if (!energy.TrySpendStartCost())
                return;

            _bulletTimeCoroutine = StartCoroutine(BulletTimeRoutine());
        }

        protected IEnumerator BulletTimeRoutine()
        {
            _isBulletTimeActive = true;

            if (GameplayManager.Instance != null)
                GameplayManager.Instance.SetGameplayStatus(EGameplayStatus.BulletTime, true);

            float elapsed = 0f;

            while (true)
            {
                while (_isPaused)
                    yield return null;

                if (bulletTimeDuration > 0f)
                {
                    elapsed += Time.unscaledDeltaTime;
                    if (elapsed >= bulletTimeDuration)
                        break;
                }

                // 只要能量真正到 0，就结束子弹时间
                if (energy != null && energy.CurrentEnergy <= 0f)
                    break;

                yield return null;
            }

            if (GameplayManager.Instance != null)
                GameplayManager.Instance.SetGameplayStatus(EGameplayStatus.Default, true);

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