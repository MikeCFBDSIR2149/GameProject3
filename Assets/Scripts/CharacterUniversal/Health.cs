using UnityEngine;

namespace CharacterUniversal
{
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth;
        public float CurrentHealth { get; private set; }

        private void Start()
        {
            CurrentHealth = maxHealth;
        }
        
        public void TakeDamage(float damage)
        {
            CurrentHealth -= damage;
            Debug.Log($"Name: {gameObject.name}, Health: {CurrentHealth}");
        }
    }
}
