using UnityEngine;
using UnityEngine.UI;
// using DG.Tweening; // temporarily using coroutine instead of DOTween

namespace UI
{
    public class GameEndUI : UIBase
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private GameObject content;
        [SerializeField] private float fadeDuration = 0.5f;

        private Coroutine _fadeCoroutine;

        public override void OnShow(object data = null)
        {
            // Ensure content is hidden until the background fade completes
            if (content != null)
                content.SetActive(false);

            if (backgroundImage != null)
            {
                // set alpha to 0
                var col = backgroundImage.color;
                col.a = 0f;
                backgroundImage.color = col;

                // start coroutine fade (use unscaled time to match typical UI behaviour)
                if (_fadeCoroutine != null)
                    StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = StartCoroutine(FadeInBackground());
            }
            else
            {
                if (content != null)
                    content.SetActive(true);
            }
        }

        private System.Collections.IEnumerator FadeInBackground()
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(0.0001f, fadeDuration));
                var c = backgroundImage.color;
                c.a = t;
                backgroundImage.color = c;
                yield return null;
            }

            var final = backgroundImage.color;
            final.a = 1f;
            backgroundImage.color = final;

            if (content != null)
                content.SetActive(true);

            _fadeCoroutine = null;
        }
    }
}
