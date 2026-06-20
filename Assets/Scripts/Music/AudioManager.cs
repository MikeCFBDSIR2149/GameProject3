using UnityEngine;
using UserOptions;

namespace Music
{
    public class AudioManager : MonoSingleton<AudioManager>, ISyncFromOptions
    {
        private MusicBuilder _currentMusic;
        private bool _pausedByBulletTime;
        private float _masterVolume = 1f; // 0..1
        private float _soundEffectsVolume = 1f;

        protected override void Awake()
        {
            base.Awake();
            // Keep across scenes
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            if (OptionsManager.Instance != null)
            {
                OptionsManager.Instance.OnOptionsChanged += SyncFromOptions;
            }

            // Ensure we apply current options if available
            SyncFromOptions();
        }

        private void OnDisable()
        {
            if (OptionsManager.Instance != null)
            {
                OptionsManager.Instance.OnOptionsChanged -= SyncFromOptions;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (OptionsManager.Instance != null)
            {
                OptionsManager.Instance.OnOptionsChanged -= SyncFromOptions;
            }
        }

        public float GetSoundEffectsVolume()
        {
            return _soundEffectsVolume;
        }

        public void SyncFromOptions()
        {
            if (OptionsManager.Instance == null)
                return;

            float raw = OptionsManager.Instance.GetOption(EUserOptionKey.MainVolume);
            // Assume options mainVolume is 0..100, map to 0..1. If user uses 0..1 already, clamp will still work.
            _masterVolume = Mathf.Clamp01(raw / 100f);
            raw = OptionsManager.Instance.GetOption(EUserOptionKey.SoundEffectsVolume);
            _soundEffectsVolume = Mathf.Clamp01(raw / 100f);
            ApplyMasterVolumeToAll();
        }

        private void ApplyMasterVolumeToAll()
        {
            ApplyVolumeTo(_currentMusic);
        }

        private void ApplyVolumeTo(MusicBuilder mb)
        {
            if (mb == null || mb.AudioSource == null)
                return;

            mb.AudioSource.volume = _masterVolume;
        }

        public void RegisterMusic(MusicBuilder mb)
        {
            if (mb == null)
                return;

            if (_currentMusic == mb)
            {
                ApplyVolumeTo(mb);
                return;
            }

            if (_currentMusic != null && _currentMusic.AudioSource != null)
            {
                _currentMusic.AudioSource.Stop();
            }

            _pausedByBulletTime = false;
            _currentMusic = mb;
            ApplyVolumeTo(mb);
        }

        public void UnregisterMusic(MusicBuilder mb)
        {
            if (mb == null)
                return;

            if (_currentMusic == mb)
            {
                _currentMusic = null;
                _pausedByBulletTime = false;
            }
        }

        public void PlayMusic(MusicBuilder mb)
        {
            if (mb == null || mb.AudioSource == null || mb.AudioSource.clip == null)
                return;

            if (_currentMusic != mb)
            {
                RegisterMusic(mb);
            }

            if (mb.AudioSource.clip != null && !mb.AudioSource.isPlaying)
            {
                mb.AudioSource.Play();
            }

            if (_currentMusic == mb)
            {
                _pausedByBulletTime = false;
            }
        }

        public void StopMusic(MusicBuilder mb)
        {
            if (mb == null || mb.AudioSource == null)
                return;

            mb.AudioSource.Stop();
            if (_currentMusic == mb)
            {
                _pausedByBulletTime = false;
            }
        }

        public void PauseMusic(MusicBuilder mb)
        {
            if (mb == null || mb.AudioSource == null)
                return;

            if (mb.AudioSource.isPlaying)
            {
                mb.AudioSource.Pause();
                if (_currentMusic == mb)
                {
                    _pausedByBulletTime = true;
                }
            }
        }

        public void ResumeMusic(MusicBuilder mb)
        {
            if (mb == null || mb.AudioSource == null)
                return;

            if (_currentMusic == mb && _pausedByBulletTime)
            {
                if (mb.AudioSource.timeSamples > 0)
                {
                    mb.AudioSource.UnPause();
                }
                else
                {
                    mb.AudioSource.Play();
                }

                _pausedByBulletTime = false;
            }
        }

        // Called by MusicBuilder when gameplay status changes to let AudioManager decide what to do
        public void HandleGameplayStatusFor(MusicBuilder mb, EGameplayStatus status)
        {
            if (mb == null || mb.AudioSource == null)
                return;

            if (!mb.PauseDuringBulletTime || mb.AudioSource.clip == null)
                return;

            if (status == EGameplayStatus.BulletTime)
            {
                if (mb.AudioSource.isPlaying)
                {
                    PauseMusic(mb);
                }

                return;
            }

            // If we had paused this music due to bullet time, resume it
            if (_currentMusic == mb && _pausedByBulletTime)
            {
                ResumeMusic(mb);
            }
        }
    }
}
