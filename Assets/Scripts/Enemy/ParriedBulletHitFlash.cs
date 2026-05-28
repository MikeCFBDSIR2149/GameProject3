using System.Collections;
using UnityEngine;

namespace Enemy
{
    /// <summary>
    /// 敌人被“反弹子弹”(tag识别)命中时，身体短暂闪浅红光泽并自动恢复。
    /// 使用 MaterialPropertyBlock，不污染共享材质。
    /// </summary>
    public class ParriedBulletHitFlash : MonoBehaviour
    {
        [Header("Detection")]
        [Tooltip("反弹子弹的Tag（命中敌人时触发闪光）")]
        public string parriedBulletTag = "ParriedBullet";

        [Header("Render Target")]
        [Tooltip("不填则默认从自己身上 GetComponentsInChildren<Renderer>()")]
        public Transform renderersRoot;

        [Header("Flash")]
        [Tooltip("浅红闪光颜色")]
        public Color flashColor = new Color(1f, 0.55f, 0.55f, 1f);

        [Tooltip("闪光总时长（秒）")]
        public float flashDuration = 0.12f;

        [Tooltip("颜色闪光强度（0=无，1=完全变成flashColor）")]
        [Range(0f, 1f)]
        public float colorIntensity = 0.85f;

        [Header("Optional Emission (Recommended for 'glossy' feel)")]
        [Tooltip("是否同时闪发光（更显著、更像光泽）")]
        public bool useEmissionFlash = true;

        [Tooltip("发光强度倍数（越大越亮）")]
        [Range(0f, 10f)]
        public float emissionIntensity = 2.0f;

        // 常见属性名：URP/HDRP Lit 用 _BaseColor；内置 Standard 常用 _Color
        private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorID = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

        private struct RendererCache
        {
            public Renderer renderer;
            public bool hasBaseColor;
            public bool hasColor;
            public bool hasEmission;
            public Color baseColor;    // 材质原色（BaseColor/Color）
            public Color emissionColor; // 材质原发光色
        }

        private RendererCache[] _cache;
        private MaterialPropertyBlock _mpb;
        private Coroutine _flashCo;

        private void Awake()
        {
            _mpb = new MaterialPropertyBlock();
            CacheRenderers();
        }

        private void OnEnable()
        {
            CacheRenderers();
        }

        private void OnDisable()
        {
            // 对象池复用/禁用时确保恢复
            ClearFlash();
            if (_flashCo != null)
            {
                StopCoroutine(_flashCo);
                _flashCo = null;
            }
        }

        private void CacheRenderers()
        {
            Transform root = renderersRoot != null ? renderersRoot : transform;
            Renderer[] rs = root.GetComponentsInChildren<Renderer>(true);

            _cache = new RendererCache[rs.Length];
            for (int i = 0; i < rs.Length; i++)
            {
                Renderer r = rs[i];
                var entry = new RendererCache
                {
                    renderer = r,
                    hasBaseColor = false,
                    hasColor = false,
                    hasEmission = false,
                    baseColor = Color.white,
                    emissionColor = Color.black
                };

                if (r != null && r.sharedMaterial != null)
                {
                    Material mat = r.sharedMaterial;

                    entry.hasBaseColor = mat.HasProperty(BaseColorID);
                    entry.hasColor = mat.HasProperty(ColorID);
                    entry.hasEmission = mat.HasProperty(EmissionColorID);

                    if (entry.hasBaseColor)
                        entry.baseColor = mat.GetColor(BaseColorID);
                    else if (entry.hasColor)
                        entry.baseColor = mat.GetColor(ColorID);

                    if (entry.hasEmission)
                        entry.emissionColor = mat.GetColor(EmissionColorID);
                }

                _cache[i] = entry;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null) return;
            if (string.IsNullOrEmpty(parriedBulletTag)) return;
            if (!other.CompareTag(parriedBulletTag)) return;

            TriggerFlash();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision == null || collision.collider == null) return;
            if (string.IsNullOrEmpty(parriedBulletTag)) return;
            if (!collision.collider.CompareTag(parriedBulletTag)) return;

            TriggerFlash();
        }

        private void TriggerFlash()
        {
            // 修复点：这里必须检查 _cache，而不是 _renderers
            if (_cache == null || _cache.Length == 0)
                return;

            if (_flashCo != null)
                StopCoroutine(_flashCo);

            _flashCo = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            // 用一个“尖峰”式曲线：快速亮起 -> 快速消退，更像“闪一下”
            float t = 0f;
            float dur = Mathf.Max(0.02f, flashDuration);

            while (t < dur)
            {
                t += Time.deltaTime;

                float x = Mathf.Clamp01(t / dur);
                // 0->1->0 (sin)
                float pulse = Mathf.Sin(x * Mathf.PI);

                ApplyFlash(pulse);
                yield return null;
            }

            ClearFlash();
            _flashCo = null;
        }

        private void ApplyFlash(float pulse01)
        {
            if (_cache == null) return;

            float wColor = colorIntensity * pulse01;

            // 发光用更“尖锐”的强度（可根据需要调）
            float wEmission = useEmissionFlash ? (emissionIntensity * pulse01) : 0f;

            for (int i = 0; i < _cache.Length; i++)
            {
                Renderer r = _cache[i].renderer;
                if (r == null) continue;

                r.GetPropertyBlock(_mpb);

                // 颜色闪
                if (_cache[i].hasBaseColor)
                {
                    Color final = Color.Lerp(_cache[i].baseColor, flashColor, wColor);
                    _mpb.SetColor(BaseColorID, final);
                }
                else if (_cache[i].hasColor)
                {
                    Color final = Color.Lerp(_cache[i].baseColor, flashColor, wColor);
                    _mpb.SetColor(ColorID, final);
                }

                // 发光闪（更显著的“浅红光泽”）
                if (useEmissionFlash && _cache[i].hasEmission)
                {
                    // 在原发光基础上叠加一段浅红发光
                    Color add = new Color(flashColor.r, flashColor.g, flashColor.b, 1f) * wEmission;
                    Color finalEmission = _cache[i].emissionColor + add;
                    _mpb.SetColor(EmissionColorID, finalEmission);
                }

                r.SetPropertyBlock(_mpb);
            }
        }

        private void ClearFlash()
        {
            if (_cache == null) return;

            for (int i = 0; i < _cache.Length; i++)
            {
                Renderer r = _cache[i].renderer;
                if (r == null) continue;

                // 清空覆盖，恢复材质原样
                r.SetPropertyBlock(null);
            }
        }
    }
}