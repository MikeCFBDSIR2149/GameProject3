using System;
using CharacterUniversal;
using Render;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(PlayerHealth))]
    public class PlayerHealthEffectInvoker : MonoBehaviour
    {
        [SerializeField] private GlobalVolumeController globalVolumeController;
        [SerializeField] private Health playerHealth;

        private void OnEnable()
        {
            playerHealth.OnHealthChanged += HandleHealthChanged;
        }

        private void OnDisable()
        {
            playerHealth.OnHealthChanged -= HandleHealthChanged;
        }

        private void HandleHealthChanged(float value, float maxValue)
        {
            if (value >= maxValue) return;
            globalVolumeController?.PlayerHealthEffect();
        }
    }
}
