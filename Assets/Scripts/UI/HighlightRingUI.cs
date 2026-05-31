using UnityEngine;

namespace UI
{
    public class HighlightRingUI : UIBase, IHighlightUI
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Vector2 minSize = new Vector2(70f, 70f);
        [SerializeField] private Vector2 maxSize = new Vector2(150f, 150f);
        [SerializeField] private AnimationCurve distanceScaleCurve;

        public override void UpdateUI(object data)
        {
            
        }

        public void SetPosition(Vector3 screenPos)
        {
            Canvas canvas = GetComponentInParent<Canvas>();

            if (rectTransform == null || canvas == null)
                return;

            Vector2 localPoint;
            // 如果Canvas为Screen Space - Overlay，camera参数传null
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform, screenPos, cam, out localPoint))
            {
                rectTransform.anchoredPosition = localPoint;
            }
        }

        public void SetDistanceRatio(float distanceRatio)
        {
            if (rectTransform == null)
                return;

            float clampedRatio = Mathf.Clamp01(distanceRatio);
            float scaleRatio = distanceScaleCurve != null ? distanceScaleCurve.Evaluate(clampedRatio) : clampedRatio;
            scaleRatio = Mathf.Clamp01(scaleRatio);

            Vector2 resolvedMinSize = Vector2.Min(minSize, maxSize);
            Vector2 resolvedMaxSize = Vector2.Max(minSize, maxSize);
            rectTransform.sizeDelta = Vector2.Lerp(resolvedMinSize, resolvedMaxSize, scaleRatio);
        }
    }
}
