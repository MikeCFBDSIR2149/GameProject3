using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Render
{
    public class GlobalVolumeController : MonoBehaviour
    {
        [SerializeField] private Volume globalVolume;
        [SerializeField] private float filmGrainIntensityMin;
        [SerializeField] private float filmGrainIntensityMax;
        
        private Coroutine _playerHealthEffectCoroutine;

        private void OnEnable()
        {
            globalVolume.profile.TryGet(out FilmGrain filmGrain);
            filmGrain.active = true;
            filmGrain.intensity.value = filmGrainIntensityMin;
        }

        private void OnDisable()
        {
            globalVolume.profile.TryGet(out FilmGrain filmGrain);
            filmGrain.active = true;
        }

        public void GameOverEffect(bool isGameOver)
        {
            globalVolume.profile.TryGet(out FilmGrain filmGrain);
            filmGrain.active = true;
            filmGrain.intensity.value = isGameOver? filmGrainIntensityMax : filmGrainIntensityMin;
        }

        public void PlayerHealthEffect()
        {
            globalVolume.profile.TryGet(out Vignette vignette);
            _playerHealthEffectCoroutine ??= StartCoroutine(PlayerHealthEffectCoroutine(vignette, 0.2f, 0.4f, 0.15f));
        }

        private IEnumerator PlayerHealthEffectCoroutine(Vignette vignette, float startValue, float endValue, float halfDuration)
        {
            if (halfDuration <= 0f)
            {
                yield break;
            }
            vignette.active = true;
            vignette.intensity.value = startValue;
            float elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                vignette.intensity.value = Mathf.Lerp(startValue, endValue, t);
                yield return null;
            }

            elapsed = 0;
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                vignette.intensity.value = Mathf.Lerp(endValue, startValue, t);
                yield return null;
            }
            
            vignette.active = false;
            _playerHealthEffectCoroutine = null;
        }
    }
}
