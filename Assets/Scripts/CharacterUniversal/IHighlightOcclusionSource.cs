using UnityEngine;

namespace CharacterUniversal
{
    /// <summary>
    /// 可选的高亮遮挡配置。检测状态和调度时间由 HighlightManager 统一保存。
    /// </summary>
    public interface IHighlightOcclusionSource
    {
        bool UseHighlightOcclusion { get; }
        LayerMask HighlightOcclusionMask { get; }
        float HighlightOcclusionInterval { get; }
        Transform HighlightOcclusionRoot { get; }
    }
}
