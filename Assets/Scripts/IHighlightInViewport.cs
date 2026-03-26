using UnityEngine;


public interface IHighlightInViewport
{
    // UI预制体名称
    string HighlightUIPrefabName { get; }
    // 检查是否满足高亮条件
    bool CheckHighlightCondition();
    // 屏幕坐标（用于UI定位）
    Vector3 GetScreenPosition(Camera cam);
    // 高亮状态变化时调用（true=开启，false=关闭）
    void OnHighlightStateChanged(bool isHighlighted);
}
