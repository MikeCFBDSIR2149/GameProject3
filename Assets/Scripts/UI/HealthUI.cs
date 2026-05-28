using System.Globalization;
using CharacterUniversal;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// 滑动条血量UI（上限可为100或任意值）：
    /// - 通过 GameplayManager 提供的 Player 引用绑定 Health
    /// - 订阅 Health.OnHealthChanged 实时更新
    /// - 不依赖外部“手动传值”
    /// </summary>
    public class HealthUI : UIBase
    {
        [Header("References")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private TMP_Text healthText;

        [Header("Display")]
        [SerializeField] private bool showNumbers = true;
        [SerializeField] private bool showAsInt = true;

        // runtime
        private GameplayManager _gameplayManager;
        private Player.Player _boundPlayer;
        private Health _playerHealth;

        private float _current;
        private float _max = 100f;

        public override void OnInit()
        {
            base.OnInit();

            if (healthBar == null)
                healthBar = GetComponentInChildren<Slider>(true);

            if (healthText == null)
                healthText = GetComponentInChildren<TMP_Text>(true);

            // 先用默认值刷新一次（避免UI上空白）
            Apply();
        }

        public override void OnShow(object data = null)
        {
            base.OnShow(data);

            SubscribeToGameplayManager();
            BindToCurrentPlayer();
        }

        private void OnEnable()
        {
            SubscribeToGameplayManager();
            BindToCurrentPlayer();
        }

        private void OnDisable()
        {
            UnsubscribeFromGameplayManager();
            Unbind();
        }

        public override void UpdateUI(object data)
        {
            // 仍保留“外部可推送”的能力（可选）
            // 支持 Vector2(current,max) 或 float(current)
            if (data is Vector2 v2)
            {
                SetHealth(v2.x, v2.y);
                return;
            }

            if (data is float f)
            {
                SetHealth(f, _max);
                return;
            }

            if (data is int i)
            {
                SetHealth(i, _max);
                return;
            }

            if (data is HealthData hd)
            {
                showNumbers = hd.showNumbers;
                SetHealth(hd.currentHealth, hd.maxHealth);
            }
        }

        private void SubscribeToGameplayManager()
        {
            var gameplayManager = GameplayManager.Instance;
            if (_gameplayManager == gameplayManager)
                return;

            UnsubscribeFromGameplayManager();
            _gameplayManager = gameplayManager;

            if (_gameplayManager != null)
            {
                _gameplayManager.OnPlayerChanged += HandlePlayerChanged;
            }
        }

        private void UnsubscribeFromGameplayManager()
        {
            if (_gameplayManager != null)
            {
                _gameplayManager.OnPlayerChanged -= HandlePlayerChanged;
                _gameplayManager = null;
            }
        }

        private void BindToCurrentPlayer()
        {
            var player = _gameplayManager != null ? _gameplayManager.Player : null;
            HandlePlayerChanged(player);
        }

        private void HandlePlayerChanged(Player.Player player)
        {
            if (_boundPlayer == player && _playerHealth != null)
                return;

            Unbind();

            _boundPlayer = player;
            if (_boundPlayer == null)
                return;

            _playerHealth = _boundPlayer.GetComponent<Health>();
            if (_playerHealth == null)
                return;

            _playerHealth.OnHealthChanged += HandlePlayerHealthChanged;
            HandlePlayerHealthChanged(_playerHealth.CurrentHealth, _playerHealth.MaxHealth);
        }

        private void Unbind()
        {
            if (_playerHealth != null)
            {
                _playerHealth.OnHealthChanged -= HandlePlayerHealthChanged;
                _playerHealth = null;
            }

            _boundPlayer = null;
        }

        private void HandlePlayerHealthChanged(float current, float max)
        {
            SetHealth(current, max);
        }

        public void SetHealth(float current, float max)
        {
            _max = Mathf.Max(1f, max);
            _current = Mathf.Clamp(current, 0f, _max);
            Apply();
        }

        private void Apply()
        {
            if (healthBar != null)
            {
                healthBar.maxValue = _max;
                healthBar.value = _current;
            }

            if (healthText != null)
            {
                healthText.gameObject.SetActive(showNumbers);
                if (!showNumbers) return;

                if (showAsInt)
                {
                    int c = Mathf.CeilToInt(_current);
                    int m = Mathf.CeilToInt(_max);
                    healthText.text = $"{c}/{m}";
                }
                else
                {
                    string c = _current.ToString("0.#", CultureInfo.InvariantCulture);
                    string m = _max.ToString("0.#", CultureInfo.InvariantCulture);
                    healthText.text = $"{c}/{m}";
                }
            }
        }
    }
}