using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// 能量UI（滑动条）：
    /// - 通过 GameplayManager 提供的 Player 引用绑定 BulletTimeEnergy
    /// - 监听 BulletTimeEnergy 实时更新
    /// - 不依赖外部"手动传值"
    /// </summary>
    public class EnergyUI : UIBase
    {
        [Header("References")]
        [SerializeField] private Slider energySlider;
        [SerializeField] private TMP_Text energyText;

        [Header("Display")]
        [SerializeField] private bool showNumbers = true;
        [SerializeField] private bool showAsInt = true;

        // runtime
        private GameplayManager _gameplayManager;
        private Player.Player _boundPlayer;
        private Player.BulletTimeEnergy _energy;

        private float _current;
        private float _max = 100f;

        public override void OnInit()
        {
            base.OnInit();

            if (energySlider == null)
                energySlider = GetComponentInChildren<Slider>(true);

            if (energyText == null)
                energyText = GetComponentInChildren<TMP_Text>(true);

            // 先用默认值刷新一次，避免UI空白
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
            // 兼容事件推送：传入 float 或 Vector2(current,max)
            if (data is float f)
            {
                SetEnergy(f, _max);
                return;
            }

            if (data is int i)
            {
                SetEnergy(i, _max);
                return;
            }

            if (data is Vector2 v2)
            {
                SetEnergy(v2.x, Mathf.Max(1f, v2.y));
                return;
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
            if (_boundPlayer == player && _energy != null)
                return;

            Unbind();

            _boundPlayer = player;
            if (_boundPlayer == null)
                return;

            _energy = _boundPlayer.GetComponent<Player.BulletTimeEnergy>();
            if (_energy == null)
                return;

            // 立刻拉取一次，保证 UI 立即刷新
            SetEnergy(_energy.CurrentEnergy, _energy.MaxEnergy);
        }

        private void Unbind()
        {
            _energy = null;
            _boundPlayer = null;
        }

        public void SetEnergy(float current, float max)
        {
            _max = Mathf.Max(1f, max);
            _current = Mathf.Clamp(current, 0f, _max);
            Apply();
        }

        private void Apply()
        {
            if (energySlider != null)
            {
                energySlider.maxValue = _max;
                energySlider.value = _current;
            }

            if (energyText != null)
            {
                energyText.gameObject.SetActive(showNumbers);
                if (!showNumbers) return;

                if (showAsInt)
                {
                    int c = Mathf.CeilToInt(_current);
                    int m = Mathf.CeilToInt(_max);
                    energyText.text = $"{c}/{m}";
                }
                else
                {
                    string c = _current.ToString("0.#", CultureInfo.InvariantCulture);
                    string m = _max.ToString("0.#", CultureInfo.InvariantCulture);
                    energyText.text = $"{c}/{m}";
                }
            }
        }
    }
}