using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class MusicBulider : MonoBehaviour
{
    [Header("Scene Music")]
    [SerializeField] private AudioClip sceneMusic;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = true;

    [Header("Bullet Time Reaction")]
    [SerializeField] private bool pauseDuringBulletTime = true;

    private AudioSource _audioSource;
    private GameplayManager _gameplayManager;
    private bool _pausedByBulletTime;

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
    }

    private void OnDestroy()
    {
        UnbindGameplayManager();
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

        _pausedByBulletTime = false;
    }

    private void SyncToCurrentGameplayStatus()
    {
        if (_gameplayManager == null)
            return;

        HandleGameplayStatusChanged(_gameplayManager.Status);
    }

    private void HandleGameplayStatusChanged(EGameplayStatus status)
    {
        if (!pauseDuringBulletTime || _audioSource == null || _audioSource.clip == null)
            return;

        if (status == EGameplayStatus.BulletTime)
        {
            if (_audioSource.isPlaying)
            {
                _audioSource.Pause();
                _pausedByBulletTime = true;
            }
            return;
        }

        if (_pausedByBulletTime)
        {
            if (_audioSource.timeSamples > 0)
            {
                _audioSource.UnPause();
            }
            else
            {
                _audioSource.Play();
            }

            _pausedByBulletTime = false;
        }
    }

    public void PlayMusic()
    {
        if (_audioSource == null || sceneMusic == null)
            return;

        if (_audioSource.clip != sceneMusic)
        {
            _audioSource.clip = sceneMusic;
        }

        if (!_audioSource.isPlaying)
        {
            _audioSource.Play();
        }

        _pausedByBulletTime = false;
    }

    public void StopMusic()
    {
        if (_audioSource == null)
            return;

        _audioSource.Stop();
        _pausedByBulletTime = false;
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
            _audioSource.Play();
        }
    }
}