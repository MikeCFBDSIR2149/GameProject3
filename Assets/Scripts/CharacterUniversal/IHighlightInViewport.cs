using UnityEngine;

namespace CharacterUniversal
{
    public interface IHighlightInViewport
    {
        // 高亮对象的世界空间锚点
        Transform HighlightTransform { get; }
        // UI预制体名称
        string HighlightUIPrefabName { get; }
        // 高亮距离最小值（归一化x=0）
        float HighlightMinDistance { get; }
        // 高亮距离最大值（归一化x=1）
        float HighlightMaxDistance { get; }
        // 与距离、视口和遮挡无关的廉价业务条件
        bool IsHighlightEligible { get; }
        // 准心评分相同时的交互优先级
        int InteractionPriority { get; }
        // 高亮状态变化时调用（true=开启，false=关闭）
        void OnHighlightStateChanged(bool isHighlighted);
    }
}
