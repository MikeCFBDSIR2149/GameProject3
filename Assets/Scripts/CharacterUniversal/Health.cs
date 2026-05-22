using System;
using UnityEngine;

namespace CharacterUniversal
{
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth;
        public float CurrentHealth { get; private set; }
        public float MaxHealth => maxHealth;
        public bool IsDead => CurrentHealth <= 0f;

        public event Action<float, float> OnHealthChanged;
        public event Action OnDied;

        private void Awake()
        {
            ResetHealth();
        }

        public void ResetHealth()
        {
            CurrentHealth = Mathf.Max(0f, maxHealth);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }
        
        public void TakeDamage(float damage)
        {
            if (damage <= 0f || IsDead)
                return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
            Debug.Log($"Name: {gameObject.name}, Health: {CurrentHealth}");

            if (IsDead)
            {
                OnDied?.Invoke();
            }
        }
    }
}
