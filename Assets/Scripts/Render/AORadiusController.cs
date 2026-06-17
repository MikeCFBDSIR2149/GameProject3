using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Render
{
    public class AORadiusController : MonoBehaviour
    {
        [Header("核心引用")]
        public ScriptableRendererData rendererData;

        [Header("半径数值")]
        public float radiusMin = 0f;
        public float radiusMax = 1f;

        [Header("过渡时间（秒）")]
        [Tooltip("进入子弹时间的过渡时长")]
        public float enterDuration = 0.2f;
        [Tooltip("退出子弹时间的过渡时长")]
        public float exitDuration = 0.5f;

        [Header("缓动曲线")]
        [Tooltip("进入的曲线（默认EaseOut，前快后慢）")]
        public AnimationCurve enterCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [Tooltip("退出的曲线（默认EaseIn，前快后慢消失）")]
        public AnimationCurve exitCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        // 内部状态
        private float currentRadius;
        private float transitionStart;
        private float transitionTarget;
        private float transitionTimer;
        private float transitionDuration;
        private AnimationCurve transitionCurve;
        private bool transitioning;

        // 反射缓存
        private ScriptableRendererFeature ssaoFeature;
        private FieldInfo settingsField;
        private FieldInfo radiusField;

        void Awake()
        {
            currentRadius = radiusMin;
            InitReflection();
        }

        private void OnEnable()
        {
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnStatusChanged += OnGameplayStatusChanged;
            }
        }

        private void OnDisable()
        {
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnStatusChanged -= OnGameplayStatusChanged;
            }
            ResetToDefault();
        }

        private void OnGameplayStatusChanged(EGameplayStatus newStatus)
        {
            if (newStatus == EGameplayStatus.BulletTime)
            {
                StartTransition(radiusMax, enterDuration, enterCurve);
            }
            else if (newStatus == EGameplayStatus.Default && GameplayManager.Instance.PreviousStatus == EGameplayStatus.BulletTime)
            {
                StartTransition(radiusMin, exitDuration, exitCurve);
            }
        }

        private void StartTransition(float target, float duration, AnimationCurve curve)
        {
            transitionStart = currentRadius;
            transitionTarget = target;
            transitionDuration = Mathf.Max(0.001f, duration);
            transitionCurve = curve;
            transitionTimer = 0f;
            transitioning = true;
        }

        private void Update()
        {
            if (ssaoFeature == null || radiusField == null) return;
            if (!transitioning) return;

            transitionTimer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(transitionTimer / transitionDuration);
            float eased = transitionCurve != null ? transitionCurve.Evaluate(t) : t;
            currentRadius = Mathf.Lerp(transitionStart, transitionTarget, eased);

            ApplyValue(currentRadius);

            if (t >= 1f)
            {
                currentRadius = transitionTarget;
                ApplyValue(currentRadius);
                transitioning = false;
            }
        }

        void OnApplicationQuit()
        {
            ResetToDefault();
        }

        private void ResetToDefault()
        {
            if (ssaoFeature != null && radiusField != null)
            {
                ApplyValue(radiusMin);
            }
        }

        private void ApplyValue(float val)
        {
            if (ssaoFeature == null) return;

            object settingsCopy = settingsField.GetValue(ssaoFeature);
            radiusField.SetValue(settingsCopy, val);
            settingsField.SetValue(ssaoFeature, settingsCopy);

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(rendererData);
            }
#endif
        }

        private bool InitReflection()
        {
            if (rendererData == null) return false;

            foreach (var feature in rendererData.rendererFeatures)
            {
                if (feature != null && feature.GetType().Name.Contains("ScreenSpaceAmbientOcclusion"))
                {
                    ssaoFeature = feature;
                    settingsField = feature.GetType().GetField("m_Settings", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (settingsField != null)
                    {
                        var settingsType = settingsField.FieldType;
                        radiusField = settingsType.GetField("Radius", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                      ?? settingsType.GetField("radius", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    }
                    break;
                }
            }

            return ssaoFeature != null && radiusField != null;
        }
    }
}