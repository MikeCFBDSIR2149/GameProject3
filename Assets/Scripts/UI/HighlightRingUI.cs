using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HighlightRingUI : UIBase, IHighlightUI
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Vector2 minSize = new Vector2(70f, 70f);
        [SerializeField] private Vector2 maxSize = new Vector2(150f, 150f);
        [SerializeField] private AnimationCurve distanceScaleCurve;

        private Canvas _canvas;
        private RectTransform _canvasRectTransform;
        private Camera _canvasCamera;
        private Vector2 _lastAnchoredPosition;
        private Vector2 _lastSize;
        private bool _hasVisualState;

        public override void OnInit()
        {
            if (rectTransform == null)
                rectTransform = transform as RectTransform;

            _canvas = GetComponentInParent<Canvas>();
            _canvasRectTransform = _canvas != null ? _canvas.transform as RectTransform : null;
            _canvasCamera = _canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? _canvas.worldCamera
                : null;

            Image image = GetComponent<Image>();
            if (image != null)
                image.raycastTarget = false;
        }

        public override void UpdateUI(object data)
        {
        }

        public void ApplyVisual(Vector3 screenPos, float distanceRatio)
        {
            if (rectTransform == null || _canvasRectTransform == null)
                return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _canvasRectTransform, screenPos, _canvasCamera, out Vector2 localPoint) &&
                (!_hasVisualState || localPoint != _lastAnchoredPosition))
            {
                rectTransform.anchoredPosition = localPoint;
                _lastAnchoredPosition = localPoint;
            }

            float clampedRatio = Mathf.Clamp01(distanceRatio);
            float scaleRatio = distanceScaleCurve != null
                ? distanceScaleCurve.Evaluate(clampedRatio)
                : clampedRatio;
            scaleRatio = Mathf.Clamp01(scaleRatio);

            Vector2 resolvedMinSize = Vector2.Min(minSize, maxSize);
            Vector2 resolvedMaxSize = Vector2.Max(minSize, maxSize);
            Vector2 targetSize = Vector2.Lerp(resolvedMinSize, resolvedMaxSize, scaleRatio);
            if (!_hasVisualState || targetSize != _lastSize)
            {
                rectTransform.sizeDelta = targetSize;
                _lastSize = targetSize;
            }

            _hasVisualState = true;
        }

        public bool TryGetAimScore(Vector2 screenPoint, out float normalizedSquaredDistance)
        {
            normalizedSquaredDistance = float.PositiveInfinity;
            if (rectTransform == null || _canvasRectTransform == null ||
                !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform, screenPoint, _canvasCamera, out Vector2 localPoint))
                return false;

            Rect rect = rectTransform.rect;
            if (!rect.Contains(localPoint))
                return false;

            float halfWidth = Mathf.Max(rect.width * 0.5f, Mathf.Epsilon);
            float halfHeight = Mathf.Max(rect.height * 0.5f, Mathf.Epsilon);
            float normalizedX = (localPoint.x - rect.center.x) / halfWidth;
            float normalizedY = (localPoint.y - rect.center.y) / halfHeight;
            normalizedSquaredDistance = normalizedX * normalizedX + normalizedY * normalizedY;
            return true;
        }
    }
}
