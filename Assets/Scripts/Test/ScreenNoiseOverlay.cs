using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Effects
{
    [DisallowMultipleComponent]
    public class ScreenNoiseOverlay : MonoBehaviour
    {
        [Header("Auto Build")]
        [SerializeField] private bool dontDestroyOnLoad = false;
        [SerializeField] private int sortingOrder = -100;
        [SerializeField] private bool startHidden = true;

        [Header("Shader")]
        [Tooltip("不填就会自动用 Shader.Find(\"Custom/ScreenNoiseOverlay\")")]
        [SerializeField] private Shader noiseShader;

        [Header("Noise")]
        [Range(0f, 1f)]
        [SerializeField] private float defaultOpacity = 0.12f;

        [Range(10f, 1000f)]
        [SerializeField] private float noiseScale = 260f;

        [Range(0f, 50f)]
        [SerializeField] private float noiseSpeed = 12f;

        [Range(0f, 1f)]
        [SerializeField] private float noiseStrength = 1f;

        [Range(0.1f, 8f)]
        [SerializeField] private float grainPower = 4f;

        [SerializeField] private Color tint = Color.white;

        [Header("Fade")]
        [Range(0f, 2f)]
        [SerializeField] private float fadeInTime = 0.12f;

        [Range(0f, 2f)]
        [SerializeField] private float fadeOutTime = 0.12f;

        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private RawImage _noiseImage;
        private Material _mat;
        private Coroutine _fadeRoutine;

        private static readonly int OpacityID = Shader.PropertyToID("_Opacity");
        private static readonly int NoiseScaleID = Shader.PropertyToID("_NoiseScale");
        private static readonly int NoiseSpeedID = Shader.PropertyToID("_NoiseSpeed");
        private static readonly int NoiseStrengthID = Shader.PropertyToID("_NoiseStrength");
        private static readonly int TintID = Shader.PropertyToID("_Tint");
        private static readonly int GrainPowerID = Shader.PropertyToID("_GrainPower");

        private void Awake()
        {
            BuildIfNeeded();
            ApplyDefaults();
            ShowImmediate();
        }

        private void OnDestroy()
        {
            if (_mat != null)
            {
                Destroy(_mat);
                _mat = null;
            }
        }

        private void BuildIfNeeded()
        {
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            // Canvas
            _canvas = GetComponent<Canvas>();
            if (_canvas == null) _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.overrideSorting = true;
            _canvas.sortingOrder = sortingOrder;
            

            if (GetComponent<CanvasScaler>() == null)
                gameObject.AddComponent<CanvasScaler>();

            if (GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();

            // CanvasGroup
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            // RawImage
            Transform child = transform.Find("NoiseImage");
            if (child == null)
            {
                GameObject imageObj = new GameObject("NoiseImage", typeof(RectTransform), typeof(RawImage));
                imageObj.transform.SetParent(transform, false);
                child = imageObj.transform;
            }

            _noiseImage = child.GetComponent<RawImage>();
            if (_noiseImage == null)
                _noiseImage = child.gameObject.AddComponent<RawImage>();

            RectTransform rt = _noiseImage.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            _noiseImage.raycastTarget = false;
            _noiseImage.texture = Texture2D.whiteTexture;
            _noiseImage.color = Color.white;

            // Material
            Shader shader = noiseShader != null ? noiseShader : Shader.Find("Custom/ScreenNoiseOverlay");
            if (shader == null)
            {
                Debug.LogError("[ScreenNoiseOverlay] 找不到 Shader: Custom/ScreenNoiseOverlay");
                enabled = false;
                return;
            }

            _mat = new Material(shader);
            _noiseImage.material = _mat;
        }

        private void ApplyDefaults()
        {
            if (_mat == null) return;

            _mat.SetFloat(OpacityID, defaultOpacity);
            _mat.SetFloat(NoiseScaleID, noiseScale);
            _mat.SetFloat(NoiseSpeedID, noiseSpeed);
            _mat.SetFloat(NoiseStrengthID, noiseStrength);
            _mat.SetFloat(GrainPowerID, grainPower);
            _mat.SetColor(TintID, tint);
        }

        public void SetOpacity(float opacity)
        {
            if (_mat == null) return;
            _mat.SetFloat(OpacityID, Mathf.Clamp01(opacity));
        }

        public void SetNoiseScale(float value)
        {
            if (_mat == null) return;
            _mat.SetFloat(NoiseScaleID, value);
        }

        public void SetNoiseSpeed(float value)
        {
            if (_mat == null) return;
            _mat.SetFloat(NoiseSpeedID, value);
        }

        public void SetNoiseStrength(float value)
        {
            if (_mat == null) return;
            _mat.SetFloat(NoiseStrengthID, Mathf.Clamp01(value));
        }

        public void SetTint(Color color)
        {
            if (_mat == null) return;
            _mat.SetColor(TintID, color);
        }

        public void Show()
        {
            Show(defaultOpacity);
        }

        public void Show(float opacity)
        {
            if (_mat != null)
                _mat.SetFloat(OpacityID, Mathf.Clamp01(opacity));

            FadeTo(1f, fadeInTime);
        }

        public void Hide()
        {
            FadeTo(0f, fadeOutTime);
        }

        public void ShowImmediate()
        {
            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
                _fadeRoutine = null;
            }

            _canvasGroup.alpha = 1f;
            gameObject.SetActive(true);
        }

        public void HideImmediate()
        {
            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
                _fadeRoutine = null;
            }

            _canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        public void Pulse(float opacity, float holdTime)
        {
            StartCoroutine(PulseRoutine(opacity, holdTime));
        }

        private IEnumerator PulseRoutine(float opacity, float holdTime)
        {
            Show(opacity);
            yield return new WaitForSecondsRealtime(Mathf.Max(0f, holdTime));
            Hide();
        }

        private void FadeTo(float targetAlpha, float duration)
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
            }

            _fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha, duration));
        }

        private IEnumerator FadeRoutine(float targetAlpha, float duration)
        {
            float startAlpha = _canvasGroup.alpha;

            if (duration <= 0.0001f)
            {
                _canvasGroup.alpha = targetAlpha;
                if (Mathf.Approximately(targetAlpha, 0f))
                    gameObject.SetActive(false);

                _fadeRoutine = null;
                yield break;
            }

            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, k);
                yield return null;
            }

            _canvasGroup.alpha = targetAlpha;

            if (Mathf.Approximately(targetAlpha, 0f))
                gameObject.SetActive(false);

            _fadeRoutine = null;
        }
    }
}
