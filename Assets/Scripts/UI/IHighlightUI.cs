using UnityEngine;

namespace UI
{
    public interface IHighlightUI
    {
        // 设置UI在屏幕上的位置
        void SetPosition(Vector3 screenPos);
        // 设置距离归一化比例（0-1）
        void SetDistanceRatio(float distanceRatio);
    }
}

