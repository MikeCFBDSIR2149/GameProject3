using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// 能量UI（滑动条）：
    /// - 自动绑定 Player 身上的 Player.BulletTimeEnergy
    /// - 监听 UIEventManager "OnEnergyChanged"（兼容事件推送）
    /// - UI 即使晚创建也会主动拉取一次当前能量，避免“开局没值/不更新”
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
        private Player.BulletTimeEnergy _energy;
        private Coroutine _bindCoroutine;

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
            EnsureBindToEnergy();
        }

        private void OnEnable()
        {
            // 监听事件（BulletTimeEnergy 每次变化会 TriggerEvent）
            UIEventManager.AddListener("OnEnergyChanged", UpdateUI);

            // 绑定并立刻拉一次当前值（解决“UI没变化/开局没值”）
            EnsureBindToEnergy();
        }

        private void OnDisable()
        {
            UIEventManager.RemoveListener("OnEnergyChanged", UpdateUI);
            Unbind();
        }

        public override void UpdateUI(object data)
        {
            // 兼容事件推送：OnEnergyChanged 传入 float
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

            // 也兼容 Vector2(current,max)（如果你以后想推 max）
            if (data is Vector2 v2)
            {
                SetEnergy(v2.x, Mathf.Max(1f, v2.y));
                return;
            }
        }

        private void EnsureBindToEnergy()
        {
            if (_energy != null) return;

            if (_bindCoroutine == null)
                _bindCoroutine = StartCoroutine(BindWhenReady());
        }

        private IEnumerator BindWhenReady()
        {
            while (_energy == null)
            {
                // 优先从 GameplayManager 的 Player 拿（如果它已注册）
                var player = GameplayManager.Instance != null ? GameplayManager.Instance.Player : null;

                // 兜底：场景里找 Player.Player
                if (player == null)
                    player = FindFirstObjectByType<Player.Player>();

                if (player != null)
                {
                    // 关键：从“玩家物体”上取 BulletTimeEnergy
                    _energy = player.GetComponent<Player.BulletTimeEnergy>();
                    if (_energy != null)
                    {
                        // 立刻拉取一次，保证 UI 立即刷新
                        SetEnergy(_energy.CurrentEnergy, _energy.MaxEnergy);
                        break;
                    }
                }

                yield return null;
            }

            _bindCoroutine = null;
        }

        private void Unbind()
        {
            if (_bindCoroutine != null)
            {
                StopCoroutine(_bindCoroutine);
                _bindCoroutine = null;
            }

            _energy = null;
        }

        // 对外（可选）初始化接口：如果你想在创建UI时手动喂值也可以用
        public void Init(float max, float current)
        {
            SetEnergy(current, max);
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