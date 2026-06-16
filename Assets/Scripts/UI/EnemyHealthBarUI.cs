using CharacterUniversal;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// 敌人头顶血条：
    /// - 挂在敌人头顶的 World Space Canvas 上
    /// - 自动寻找父物体上的 Health
    /// - 自动寻找子物体里的 Slider / TMP_Text
    /// - 支持跟随摄像机转向
    /// </summary>
    public class EnemyHealthBarUI : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Health targetHealth;

        [Header("References (auto if null)")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private TMP_Text healthText;

        [Header("Display")]
        [SerializeField] private bool showNumbers = true;
        [SerializeField] private bool showAsInt = true;

        [Header("Billboard")]
        [SerializeField] private bool faceCamera = true;
        [SerializeField] private Camera billboardCamera;
        [SerializeField] private Vector3 rotationOffset = Vector3.zero;

        [Header("Optional")]
        [SerializeField] private bool hideWhenFullHealth = false;

        private float _current;
        private float _max = 1f;
        private bool _subscribed;

        private void Awake()
        {
            AutoFindReferences();

            if (targetHealth == null)
                targetHealth = GetComponentInParent<Health>();

            if (billboardCamera == null)
                billboardCamera = Camera.main;
        }

        private void OnEnable()
        {
            AutoFindReferences();

            if (targetHealth == null)
                targetHealth = GetComponentInParent<Health>();

            Subscribe();
            RefreshOnce();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void LateUpdate()
        {
            if (!faceCamera)
                return;

            if (billboardCamera == null)
                billboardCamera = Camera.main;

            if (billboardCamera == null)
                return;

            Vector3 lookDir = transform.position - billboardCamera.transform.position;
            if (lookDir.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
                transform.Rotate(rotationOffset);
            }
        }

        private void Subscribe()
        {
            if (_subscribed)
                return;

            if (targetHealth == null)
                return;

            targetHealth.OnHealthChanged += HandleHealthChanged;
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed)
                return;

            if (targetHealth != null)
                targetHealth.OnHealthChanged -= HandleHealthChanged;

            _subscribed = false;
        }

        private void HandleHealthChanged(float current, float max)
        {
            SetHealth(current, max);
        }

        private void RefreshOnce()
        {
            if (targetHealth == null)
                return;

            SetHealth(targetHealth.CurrentHealth, targetHealth.MaxHealth);
        }

        public void SetHealth(float current, float max)
        {
            _max = Mathf.Max(1f, max);
            _current = Mathf.Clamp(current, 0f, _max);

            Apply();

            if (hideWhenFullHealth)
            {
                gameObject.SetActive(_current < _max);
            }
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
                if (!showNumbers)
                    return;

                if (showAsInt)
                {
                    int c = Mathf.CeilToInt(_current);
                    int m = Mathf.CeilToInt(_max);
                    healthText.text = $"{c}/{m}";
                }
                else
                {
                    healthText.text = $"{_current:0.#}/{_max:0.#}";
                }
            }
        }

        private void AutoFindReferences()
        {
            if (healthBar == null)
                healthBar = GetComponentInChildren<Slider>(true);

            if (healthText == null)
                healthText = GetComponentInChildren<TMP_Text>(true);
        }
    }
}