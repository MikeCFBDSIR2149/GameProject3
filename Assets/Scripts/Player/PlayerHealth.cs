using System;
using CharacterUniversal;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Health))]
    public class PlayerHealth : MonoBehaviour
    {
        private Health _health;
        private bool _isDead;

        public float CurrentHealth => _health != null ? _health.CurrentHealth : 0f;
        public float MaxHealth => _health != null ? _health.MaxHealth : 0f;
        public bool IsDead => _isDead;

        public event Action OnPlayerDied;

        private void Awake()
        {
            TryGetComponent(out _health);
            if (_health == null)
            {
                Debug.LogError($"[PlayerHealth] Missing Health component on {gameObject.name}");
            }
            // 初始化为未死亡
            _isDead = false;
        }

        private void OnEnable()
        {
            if (_health != null)
            {
                _health.OnDied += HandleDied;
            }
        }

        private void OnDisable()
        {
            if (_health != null)
            {
                _health.OnDied -= HandleDied;
            }
        }

        private void HandleDied()
        {
            if (_isDead)
                return;

            _isDead = true;
            Debug.Log($"[PlayerHealth] Player died: {gameObject.name}");
            GameplayManager.Instance?.RequestGameOver();
            OnPlayerDied?.Invoke();
        }
    }
}


