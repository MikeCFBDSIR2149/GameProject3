using System;
using UnityEngine;
using UI;

namespace Player
{
    /// <summary>
    /// 子弹时间能量池：BulletTime 时持续消耗；耗尽后强制关闭，并进入冷却锁定。
    /// </summary>
    public class BulletTimeEnergy : MonoBehaviour
    {
        [Header("Energy")]
        public float maxEnergy = 300f;
        [SerializeField] private float currentEnergy = 300f;

        [Header("Drain / Regen (per second)")]
        public float drainPerSecond = 30f;   // 子弹时间每秒消耗
        public float regenPerSecond = 15f;   // 非子弹时间每秒回复

        [Header("Lock when depleted")]
        public float depletedLockSeconds = 2f;

        [Header("Optional cost")]
        public float startCost = 0f;         // 可选：每次开启瞬间扣一点（不需要就留 0）

        public float CurrentEnergy => currentEnergy;
        public float MaxEnergy => maxEnergy;

        public bool IsDepletedLocked => _lockTimer > 0f;
        public bool CanStartBulletTime => currentEnergy > 0f && !IsDepletedLocked;

        public event Action OnDepleted; // 能量耗尽事件（供 PlayerBulletTimeSkill 订阅 -> 关闭子弹时间）

        private float _lockTimer;

        private bool _doNotUpdate;

        private void Awake()
        {
            currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
        }

        private void Start()
        {
            // 开局推一次 UI
            PushEnergyUI();
        }

        private void OnEnable()
        {
            GameplayManager.Instance.OnStatusChanged += OnGameplayStatusChanged;
        }

        private void OnDisable()
        {
            GameplayManager.Instance.OnStatusChanged -= OnGameplayStatusChanged;
        }
        
        private void OnGameplayStatusChanged(EGameplayStatus newStatus)
        {
            _doNotUpdate = !GameplayManager.Instance.CanPerformGameplayActions;
        }

        private void Update()
        {
            if (_doNotUpdate) return;
            // 用 unscaled，保证慢动作/子弹时间不影响“能量逻辑的真实速度”
            float dt = Time.unscaledDeltaTime;

            if (_lockTimer > 0f)
            {
                _lockTimer -= dt;
                if (_lockTimer < 0f) _lockTimer = 0f;
            }

            bool isBulletTime = GameplayManager.Instance != null &&
                                GameplayManager.Instance.Status == EGameplayStatus.BulletTime;

            float before = currentEnergy;

            if (isBulletTime)
            {
                currentEnergy -= drainPerSecond * dt;

                if (currentEnergy <= 0f)
                {
                    currentEnergy = 0f;

                    // 进入耗尽锁定
                    if (_lockTimer <= 0f)
                        _lockTimer = depletedLockSeconds;

                    // 通知外部“耗尽了” -> 外部负责关闭 BulletTime
                    OnDepleted?.Invoke();
                }
            }
            else
            {
                // 非子弹时间回复
                currentEnergy += regenPerSecond * dt;
                currentEnergy = Mathf.Min(currentEnergy, maxEnergy);
            }

            if (!Mathf.Approximately(before, currentEnergy))
            {
                PushEnergyUI();
            }
        }

        public bool TrySpendStartCost()
        {
            if (startCost <= 0f) return true;

            if (!CanStartBulletTime) return false;

            if (currentEnergy < startCost) return false;

            currentEnergy -= startCost;
            if (currentEnergy < 0f) currentEnergy = 0f;
            PushEnergyUI();
            return true;
        }

        private void PushEnergyUI()
        {
            // 你现有 EnergyUI 监听的事件名就是这个
            UIEventManager.TriggerEvent("OnEnergyChanged", currentEnergy);
        }
    }
}