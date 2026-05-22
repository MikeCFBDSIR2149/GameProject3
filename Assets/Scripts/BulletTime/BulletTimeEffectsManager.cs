using System.Collections;
using UnityEngine;

namespace BulletTime
{
    public class BulletTimeEffectsManager : MonoBehaviour
    {
    [Header("Overlay Settings")]
    [SerializeField] private UnityEngine.UI.Image overlayImage;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    [Header("Wave Settings")]
    [SerializeField] private float waveMaxRadius = 1.0f;
    [SerializeField] private float waveWidth = 0.1f;
    [SerializeField] private float waveStrength = 0.7f;
    [SerializeField] private float waveExpandDuration = 0.4f;
    [SerializeField] private float waveContractDuration = 0.4f;

    private Material _matInstance;
    private Coroutine _effectCoroutine;

    private bool _isPaused;
    private bool _isBulletTime;   // 当前是否处于子弹时间状态（用于恢复时不重播）
    private bool _enteredOnce;    // 防止重复播放 Enter 效果

    private static readonly int IntensityID = Shader.PropertyToID("_Intensity");
    private static readonly int WaveCenterID = Shader.PropertyToID("_WaveCenter");
    private static readonly int WaveRadiusID = Shader.PropertyToID("_WaveRadius");
    private static readonly int WaveWidthID = Shader.PropertyToID("_WaveWidth");
    private static readonly int WaveStrengthID = Shader.PropertyToID("_WaveStrength");
    private static readonly int GlitchStrengthID = Shader.PropertyToID("_GlitchStrength");

    private void Awake()
    {
        if (overlayImage != null)
        {
            _matInstance = Instantiate(overlayImage.material);
            overlayImage.material = _matInstance;

            _matInstance.SetFloat(IntensityID, 0f);
            _matInstance.SetFloat(WaveRadiusID, 0f);
            _matInstance.SetFloat(WaveWidthID, waveWidth);
            _matInstance.SetFloat(WaveStrengthID, waveStrength);
            _matInstance.SetFloat(GlitchStrengthID, 0f);
            _matInstance.SetVector(WaveCenterID, new Vector4(0.5f, 0.5f, 0, 0));
        }

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
        // 1) 处理暂停：只冻结，不触发 Enter/Exit
        if (status == EGameplayStatus.Paused)
        {
            _isPaused = true;
            return;
        }

        if (status == EGameplayStatus.GameOver)
        {
            _isPaused = false;
            _isBulletTime = false;
            _enteredOnce = false;
            ExitBulletTimeEffects();
            return;
        }

        // 2) 非暂停：恢复
        _isPaused = false;

        switch (status)
        {
            case EGameplayStatus.BulletTime:
                _isBulletTime = true;
                EnterBulletTimeEffects();
                break;

            case EGameplayStatus.Default:
                _isBulletTime = false;
                _enteredOnce = false; // 回到默认后允许下次再播 Enter
                ExitBulletTimeEffects();
                break;
        }
    }

    private void EnterBulletTimeEffects()
    {
        // 已经播过 enter 且仍在 bulletTime：不要重复播（防止“又播一遍”）
        if (_isBulletTime && _enteredOnce)
            return;

        _enteredOnce = true;

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

    private float GetEffectDeltaTime()
    {
        // 暂停时冻结效果动画
        if (_isPaused) return 0f;

        // 非暂停时：用 unscaled，保证子弹时间/慢动作不影响 UI 特效速度
        return Time.unscaledDeltaTime;
    }

    private IEnumerator DoEnterEffect()
    {
        if (overlayImage == null || _matInstance == null) yield break;

        overlayImage.gameObject.SetActive(true);

        // 1) Intensity 淡入
        float t = 0f;
        while (t < fadeInDuration)
        {
            float dt = GetEffectDeltaTime();
            if (dt <= 0f) { yield return null; continue; }

            t += dt;
            float k = Mathf.Clamp01(t / fadeInDuration);
            _matInstance.SetFloat(IntensityID, k);
            yield return null;
        }
        _matInstance.SetFloat(IntensityID, 1f);

        // 2) 波纹扩散
        t = 0f;
        while (t < waveExpandDuration)
        {
            float dt = GetEffectDeltaTime();
            if (dt <= 0f) { yield return null; continue; }

            t += dt;
            float k = Mathf.Clamp01(t / waveExpandDuration);
            float radius = Mathf.Lerp(0f, waveMaxRadius, k);
            _matInstance.SetFloat(WaveRadiusID, radius);
            _matInstance.SetFloat(GlitchStrengthID, Mathf.SmoothStep(0f, 1f, k));
            yield return null;
        }

        // 3) 波纹收回
        t = 0f;
        while (t < waveContractDuration)
        {
            float dt = GetEffectDeltaTime();
            if (dt <= 0f) { yield return null; continue; }

            t += dt;
            float k = Mathf.Clamp01(t / waveContractDuration);
            float radius = Mathf.Lerp(waveMaxRadius, 0f, k);
            _matInstance.SetFloat(WaveRadiusID, radius);
            _matInstance.SetFloat(GlitchStrengthID, Mathf.SmoothStep(1f, 0f, k));
            yield return null;
        }

        // 波纹结束：保留暗屏（直到退出子弹时间）
        _matInstance.SetFloat(WaveRadiusID, 0f);
        _matInstance.SetFloat(GlitchStrengthID, 0f);
    }

    private IEnumerator DoExitEffect()
    {
        if (overlayImage == null || _matInstance == null) yield break;

        float startIntensity = _matInstance.GetFloat(IntensityID);
        float t = 0f;

        while (t < fadeOutDuration)
        {
            float dt = GetEffectDeltaTime();
            if (dt <= 0f) { yield return null; continue; }

            t += dt;
            float k = Mathf.Clamp01(t / fadeOutDuration);
            float intensity = Mathf.Lerp(startIntensity, 0f, k);
            _matInstance.SetFloat(IntensityID, intensity);
            yield return null;
        }

        _matInstance.SetFloat(IntensityID, 0f);
        overlayImage.gameObject.SetActive(false);
    }
    }
}
