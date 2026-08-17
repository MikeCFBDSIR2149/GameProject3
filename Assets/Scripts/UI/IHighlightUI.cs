using UnityEngine;

namespace UI
{
    public interface IHighlightUI
    {
        // 在一次调用中更新位置和距离缩放
        void ApplyVisual(Vector3 screenPos, float distanceRatio);
        // 返回准心是否位于高亮矩形内，以及归一化后的中心距离
        bool TryGetAimScore(Vector2 screenPoint, out float normalizedSquaredDistance);
    }
}

