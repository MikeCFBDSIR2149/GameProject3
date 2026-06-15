using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Render
{
    public class GlobalVolumeController : MonoBehaviour
    {
        [SerializeField] private Volume globalVolume;

        public void GameOverEffect(bool isGameOver)
        {
            globalVolume.profile.TryGet(out FilmGrain filmGrain);
            filmGrain.active = true;
            filmGrain.intensity.value = isGameOver? 1f : 0f;
        }
    }
}
