using Music;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Common
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public class ButtonClickSound : MonoBehaviour
    {
        [Header("Click Sound")]
        [SerializeField] private AudioClip clickClip;
        [SerializeField, Range(0f, 1f)] private float volume = 1f;

        [Header("Options")]
        [SerializeField] private bool playOnClick = true;

        private Button _button;
        private AudioSource _audioSource;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _audioSource = GetOrCreateAudioSource();
        }

        private void OnEnable()
        {
            if (_button == null)
                _button = GetComponent<Button>();

            if (_button != null)
                _button.onClick.AddListener(HandleClick);
        }

        private void OnDisable()
        {
            if (_button != null)
                _button.onClick.RemoveListener(HandleClick);
        }

        private void HandleClick()
        {
            if (!playOnClick)
                return;

            if (clickClip == null)
                return;

            if (_audioSource == null)
                _audioSource = GetOrCreateAudioSource();

            if (_audioSource != null)
            {
                // Debug.Log(AudioManager.Instance.GetSoundEffectsVolume());
                _audioSource.PlayOneShot(clickClip, AudioManager.Instance != null ? AudioManager.Instance.GetSoundEffectsVolume() : volume);
            }
        }

        private AudioSource GetOrCreateAudioSource()
        {
            AudioSource source = GetComponent<AudioSource>();
            if (source == null)
            {
                source = gameObject.AddComponent<AudioSource>();
            }

            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;   // 2D 音效
            source.dopplerLevel = 0f;
            source.volume = AudioManager.Instance != null ? AudioManager.Instance.GetSoundEffectsVolume() : volume;
            // Debug.Log(source.volume);

            return source;
        }
    }
}