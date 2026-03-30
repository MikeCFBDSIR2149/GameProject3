using System.Collections;
using UnityEngine;

public class BulletTimeEffectsManager : MonoBehaviour
{
    [Header("Overlay Settings")]
    [SerializeField] private UnityEngine.UI.Image overlayImage;   // 指向 BulletTimeOverlay
    [SerializeField] private float fadeInDuration = 0.3f;         // 变黑淡入时间
    [SerializeField] private float fadeOutDuration = 0.3f;        // 变黑淡出时间

    [Header("Wave Settings")]
    [SerializeField] private float waveMaxRadius = 1.0f;          // 波纹最大半径（0~1 左右）
    [SerializeField] private float waveWidth = 0.1f;              // 波纹带宽
    [SerializeField] private float waveStrength = 0.7f;           // 波纹亮度
    [SerializeField] private float waveExpandDuration = 0.4f;     // 扩散时间
    [SerializeField] private float waveContractDuration = 0.4f;   // 收回时间

    private Material _matInstance;
    private Coroutine _effectCoroutine;

    private static readonly int IntensityID   = Shader.PropertyToID("_Intensity");
    private static readonly int WaveCenterID  = Shader.PropertyToID("_WaveCenter");
    private static readonly int WaveRadiusID  = Shader.PropertyToID("_WaveRadius");
    private static readonly int WaveWidthID   = Shader.PropertyToID("_WaveWidth");
    private static readonly int WaveStrengthID= Shader.PropertyToID("_WaveStrength");
    private static readonly int GlitchStrengthID = Shader.PropertyToID("_GlitchStrength");
    private void Awake()
    {
        if (overlayImage != null)
        {
            // 用实例材质，避免改到其他地方
            _matInstance = Instantiate(overlayImage.material);
            _matInstance.SetFloat(GlitchStrengthID, 0f);
            overlayImage.material = _matInstance;

            // 初始化参数
            _matInstance.SetFloat(IntensityID, 0f);
            _matInstance.SetFloat(WaveRadiusID, 0f);
            _matInstance.SetFloat(WaveWidthID, waveWidth);
            _matInstance.SetFloat(WaveStrengthID, waveStrength);
            // 默认中心在屏幕中间
            _matInstance.SetVector(WaveCenterID, new Vector4(0.5f, 0.5f, 0, 0));
        }

        // 初始隐藏 UI
        if (overlayImage != null)
            overlayImage.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (GameplayManager.Instance != null)
            GameplayManager.Instance.OnStatusChanged += HandleStatusChanged;
    }

    private void OnDisable()
    {
        if (GameplayManager.Instance != null)
            GameplayManager.Instance.OnStatusChanged -= HandleStatusChanged;
    }

    private void HandleStatusChanged(EGameplayStatus status)
    {
        switch (status)
        {
            case EGameplayStatus.BulletTime:
                EnterBulletTimeEffects();
                break;

            case EGameplayStatus.Default:
                ExitBulletTimeEffects();
                break;
        }
    }

   
    private void EnterBulletTimeEffects()
    {
        if (_effectCoroutine != null)
            StopCoroutine(_effectCoroutine);
        _effectCoroutine = StartCoroutine(DoEnterEffect());
    }

    private void ExitBulletTimeEffects()
    {
        if (_effectCoroutine != null)
            StopCoroutine(_effectCoroutine);
        _effectCoroutine = StartCoroutine(DoExitEffect());
    }

    private IEnumerator DoEnterEffect()
    {
        if (overlayImage == null || _matInstance == null) yield break;

        overlayImage.gameObject.SetActive(true);

        // 1) 先把 Intensity 从 0 淡到 1（屏幕变暗）
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeInDuration);
            _matInstance.SetFloat(IntensityID, k);
          
            yield return null;
        }
        _matInstance.SetFloat(IntensityID, 1f);

        // 2) 在暗的背景上做一个波纹：从 0 扩散到 waveMaxRadius 再收回
        // 扩散
        t = 0f;
        while (t < waveExpandDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / waveExpandDuration);
            float radius = Mathf.Lerp(0f, waveMaxRadius, k);
            _matInstance.SetFloat(WaveRadiusID, radius);
            _matInstance.SetFloat(GlitchStrengthID, Mathf.SmoothStep(0f, 1f, k));
            yield return null;
        }

        // 收回
        t = 0f;
        while (t < waveContractDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / waveContractDuration);
            float radius = Mathf.Lerp(waveMaxRadius, 0f, k);
            _matInstance.SetFloat(WaveRadiusID, radius);
            _matInstance.SetFloat(GlitchStrengthID, Mathf.SmoothStep(1f, 0f, k));
            yield return null;
        }

        // 波纹结束，保留暗屏效果，直到子弹时间结束
        _matInstance.SetFloat(WaveRadiusID, 0f);
        _matInstance.SetFloat(GlitchStrengthID, 0f);
    }

    private IEnumerator DoExitEffect()
    {
        if (overlayImage == null || _matInstance == null) yield break;

        // 将 Intensity 从当前值淡回 0
        float startIntensity = _matInstance.GetFloat(IntensityID);
        float t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeOutDuration);
            float intensity = Mathf.Lerp(startIntensity, 0f, k);
            _matInstance.SetFloat(IntensityID, intensity);
            yield return null;
        }

        _matInstance.SetFloat(IntensityID, 0f);
        overlayImage.gameObject.SetActive(false);
    }
}