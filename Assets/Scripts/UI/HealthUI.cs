using System.Collections;
using System.Globalization;
using CharacterUniversal;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// 滑动条血量UI（上限可为100或任意值）：
    /// - 开局自动寻找 Player，并从 Player 物体上读取 Health
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
        private Health _playerHealth;
        private Coroutine _bindCoroutine;

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

            // 每次显示都确保绑定（如果之前没绑定上）
            EnsureBindToPlayerHealth();
        }

        private void OnEnable()
        {
            // 某些UI系统可能只 SetActive，这里也补一层绑定
            EnsureBindToPlayerHealth();
        }

        private void OnDisable()
        {
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
                return;
            }
        }

        private void EnsureBindToPlayerHealth()
        {
            // 已绑定就不重复
            if (_playerHealth != null) return;

            // 若已有协程在跑就不重复开
            if (_bindCoroutine == null)
                _bindCoroutine = StartCoroutine(BindWhenPlayerReady());
        }

        private IEnumerator BindWhenPlayerReady()
        {
            // 反复等待直到能拿到 Player，并从 Player 身上 GetComponent<Health>()
            while (_playerHealth == null)
            {
                // 优先走 GameplayManager 注册的 Player（它在 Player.Awake 注册）
                var player = GameplayManager.Instance != null ? GameplayManager.Instance.Player : null;

                // 如果 GameplayManager 还没拿到，也可以用 Find（作为兜底）
                if (player == null)
                    player = FindFirstObjectByType<Player.Player>();

                if (player != null)
                {
                    // 关键：直接从“玩家物体”上取 Health（你要求的点）
                    _playerHealth = player.GetComponent<Health>();
                    if (_playerHealth != null)
                    {
                        _playerHealth.OnHealthChanged += HandlePlayerHealthChanged;

                        // 立刻刷新一次，保证开局有值
                        HandlePlayerHealthChanged(_playerHealth.CurrentHealth, _playerHealth.MaxHealth);
                        break;
                    }
                }

                yield return null; // 下一帧再试
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

            if (_playerHealth != null)
            {
                _playerHealth.OnHealthChanged -= HandlePlayerHealthChanged;
                _playerHealth = null;
            }
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