using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// 子弹时间能量UI（滑动条版，TMP文本可选）
    /// - 自动在自身子物体中寻找 Slider / TMP_Text（无需手动拖引用）
    /// - 订阅 UIEventManager: "OnEnergyChanged" 实时更新（由 BulletTimeEnergy 推送）
    /// - 开局会从 GameplayManager.Player 的 BulletTimeEnergy 拉一次当前值，避免不显示/显示旧值
    /// - 能量归零或锁定期间，文字会变红提醒
    /// </summary>
    public class EnergyUI : UIBase
    {
        [Header("Auto Find (optional, recommended)")]
        [Tooltip("如果你的UI子物体里有多个Slider，建议填写能量条Slider的物体名以精确定位。留空则取第一个找到的Slider。")]
        [SerializeField] private string sliderObjectName = "";

        [Tooltip("如果你的UI子物体里有多个TMP_Text，建议填写能量文本的物体名以精确定位。留空则取第一个找到的TMP_Text。")]
        [SerializeField] private string textObjectName = "";

        [Header("References (auto if null)")]
        [SerializeField] private Slider energySlider;
        [SerializeField] private TMP_Text energyText;

        [Header("Display")]
        [SerializeField] private bool showNumbers = true;
        [SerializeField] private bool showAsInt = true;

        [Header("Colors")]
        [SerializeField] private Color normalTextColor = Color.white;
        [SerializeField] private Color warningTextColor = Color.red;

        [Header("Defaults (used before binding)")]
        [SerializeField] private float defaultMaxEnergy = 100f;

        private GameplayManager _gameplayManager;
        private Player.Player _boundPlayer;
        private Player.BulletTimeEnergy _energy;

        private float _current;
        private float _max;

        private const string EnergyChangedEventName = "OnEnergyChanged";

        public override void OnInit()
        {
            base.OnInit();

            _max = Mathf.Max(1f, defaultMaxEnergy);
            _current = _max;

            AutoFindReferences();
            Apply();
        }

        public override void OnShow(object data = null)
        {
            base.OnShow(data);

            AutoFindReferences();
            Subscribe();
            BindToCurrentPlayerAndPullOnce();
        }

        private void OnEnable()
        {
            AutoFindReferences();
            Subscribe();
            BindToCurrentPlayerAndPullOnce();
        }

        private void OnDisable()
        {
            Unsubscribe();
            Unbind();
        }

        public override void UpdateUI(object data)
        {
            // 事件推送过来的是 currentEnergy(float)
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

            // 兼容 (current,max)
            if (data is Vector2 v2)
            {
                SetEnergy(v2.x, Mathf.Max(1f, v2.y));
                return;
            }
        }

        private void Subscribe()
        {
            UIEventManager.AddListener(EnergyChangedEventName, HandleEnergyChangedEvent);

            _gameplayManager = GameplayManager.Instance;
            if (_gameplayManager != null)
            {
                _gameplayManager.OnPlayerChanged -= HandlePlayerChanged;
                _gameplayManager.OnPlayerChanged += HandlePlayerChanged;
            }
        }

        private void Unsubscribe()
        {
            UIEventManager.RemoveListener(EnergyChangedEventName, HandleEnergyChangedEvent);

            if (_gameplayManager != null)
            {
                _gameplayManager.OnPlayerChanged -= HandlePlayerChanged;
                _gameplayManager = null;
            }
        }

        private void HandleEnergyChangedEvent(object data)
        {
            UpdateUI(data);
        }

        private void BindToCurrentPlayerAndPullOnce()
        {
            if (_gameplayManager == null)
                _gameplayManager = GameplayManager.Instance;

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

            // 开局/切换玩家时立刻刷新一次
            SetEnergy(_energy.CurrentEnergy, _energy.MaxEnergy);
        }

        private void Unbind()
        {
            _energy = null;
            _boundPlayer = null;
        }

        private void AutoFindReferences()
        {
            // Slider
            if (energySlider == null)
            {
                energySlider = FindChildByName<Slider>(sliderObjectName);
                if (energySlider == null)
                    energySlider = GetComponentInChildren<Slider>(true);
            }

            // TMP Text（可选）
            if (energyText == null)
            {
                energyText = FindChildByName<TMP_Text>(textObjectName);
                if (energyText == null)
                    energyText = GetComponentInChildren<TMP_Text>(true);
            }
        }

        private T FindChildByName<T>(string childName) where T : Component
        {
            if (string.IsNullOrWhiteSpace(childName))
                return null;

            var comps = GetComponentsInChildren<T>(true);
            for (int i = 0; i < comps.Length; i++)
            {
                if (comps[i] != null && comps[i].name == childName)
                    return comps[i];
            }

            return null;
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

            bool shouldWarn = _energy != null && (_energy.CurrentEnergy <= 0f || _energy.IsDepletedLocked);

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

                energyText.color = shouldWarn ? warningTextColor : normalTextColor;
            }
        }
    }
}