using UnityEngine;

public interface IHighlightUI
{
    // 设置UI在屏幕上的位置
    void SetPosition(Vector3 screenPos);
    // 显示UI
    void Show();
    // 隐藏UI
    void Hide();
    // 销毁UI（可选）
    void Dispose();
}

