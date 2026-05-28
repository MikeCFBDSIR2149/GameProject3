using UnityEngine;

namespace UI
{
    public class HighlightRingUI : UIBase, IHighlightUI
    {
        public override void UpdateUI(object data)
        {
            
        }

        public void SetPosition(Vector3 screenPos)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            RectTransform rectTransform = GetComponent<RectTransform>();

            Vector2 localPoint;
            // 如果Canvas为Screen Space - Overlay，camera参数传null
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform, screenPos, cam, out localPoint))
            {
                rectTransform.anchoredPosition = localPoint;
            }
        }
    }
}
