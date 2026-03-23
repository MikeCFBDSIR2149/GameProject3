using UnityEngine;

/// <summary>
/// 管理第三人称游戏中鼠标指针的锁定与解锁。
/// </summary>
public class MousePointerManager : MonoSingleton<MousePointerManager>
{
    /// <summary>
    /// 锁定并隐藏鼠标指针（常用于第三人称控制）
    /// </summary>
    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// 解锁并显示鼠标指针（如打开菜单、暂停等）
    /// </summary>
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
