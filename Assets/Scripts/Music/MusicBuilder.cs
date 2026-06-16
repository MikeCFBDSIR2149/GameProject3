using UnityEngine;

namespace Music
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public class MusicBuilder : MonoBehaviour
    {
        [Header("Scene Music")]
        [SerializeField] private AudioClip sceneMusic;
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private bool loop = true;

        [Header("Bullet Time Reaction")]
        [SerializeField] private bool pauseDuringBulletTime = true;

        private AudioSource _audioSource;
        private GameplayManager _gameplayManager;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();

            _audioSource.playOnAwake = false;
            _audioSource.loop = loop;
            _audioSource.spatialBlend = 0f;   // 2D 音乐
            _audioSource.dopplerLevel = 0f;

            if (sceneMusic != null)
            {
                _audioSource.clip = sceneMusic;
            }
        }

        private void Start()
        {
            BindGameplayManager();

            if (playOnStart)
            {
                PlayMusic();
            }

            SyncToCurrentGameplayStatus();
        }

        private void OnEnable()
        {
            BindGameplayManager();
            // register with AudioManager so central manager can control playback/volume
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.RegisterMusic(this);
            }
        }

        private void Update()
        {
            // 如果单例还没准备好，就持续尝试绑定
            if (_gameplayManager == null)
            {
                BindGameplayManager();
            }
        }

        private void OnDisable()
        {
            UnbindGameplayManager();
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.UnregisterMusic(this);
            }
        }

        private void OnDestroy()
        {
            UnbindGameplayManager();
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.UnregisterMusic(this);
            }
        }

        private void BindGameplayManager()
        {
            if (_gameplayManager != null)
                return;

            if (GameplayManager.Instance == null)
                return;

            _gameplayManager = GameplayManager.Instance;
            _gameplayManager.OnStatusChanged += HandleGameplayStatusChanged;
        }

        private void UnbindGameplayManager()
        {
            if (_gameplayManager != null)
            {
                _gameplayManager.OnStatusChanged -= HandleGameplayStatusChanged;
                _gameplayManager = null;
            }

            // _pausedByBulletTime = false;
        }

        private void SyncToCurrentGameplayStatus()
        {
            if (_gameplayManager == null)
                return;

            HandleGameplayStatusChanged(_gameplayManager.Status);
        }

        private void HandleGameplayStatusChanged(EGameplayStatus status)
        {
            // Forward gameplay status handling to the central AudioManager so it controls pause/resume
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.HandleGameplayStatusFor(this, status);
            }
        }

        public void PlayMusic()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusic(this);
            }
        }

        public void StopMusic()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopMusic(this);
            }
        }

        public void SetSceneMusic(AudioClip newClip, bool restartIfPlaying = true)
        {
            sceneMusic = newClip;

            if (_audioSource == null)
                return;

            bool wasPlaying = _audioSource.isPlaying;

            _audioSource.clip = sceneMusic;

            if (restartIfPlaying && wasPlaying && sceneMusic != null)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayMusic(this);
                }
                else
                {
                    _audioSource.Play();
                }
            }
        }

        // Expose necessary internals for AudioManager to control playback/volume
        public AudioSource AudioSource => _audioSource;
        public bool PauseDuringBulletTime => pauseDuringBulletTime;
    }
}