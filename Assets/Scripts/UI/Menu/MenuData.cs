// MenuData.cs - 菜单数据类
using System;

namespace UI
{
    [Serializable]
    public class MenuData
    {
        public MenuType menuType = MenuType.MainMenu;
        public bool canPauseGame = true; // 是否暂停游戏
        public object extraData = null; // 额外数据
        
        public MenuData() { }
        
        public MenuData(MenuType type)
        {
            menuType = type;
        }
        
        public MenuData(MenuType type, bool pauseGame)
        {
            menuType = type;
            canPauseGame = pauseGame;
        }
    }
    
    // 菜单类型枚举
    public enum MenuType
    {
        MainMenu,       // 主菜单
        PauseMenu,      // 暂停菜单
        SettingsMenu,   // 设置菜单
        GameOverMenu,   // 游戏结束菜单
        LevelSelectMenu, // 关卡选择菜单
        CreditsMenu,
    }
    
    // 游戏状态枚举
    public enum GameState
    {
        MainMenu,   // 主菜单
        Playing,    // 游戏中
        Paused,     // 暂停
        GameOver,   // 游戏结束
        Loading     // 加载中
    }
}