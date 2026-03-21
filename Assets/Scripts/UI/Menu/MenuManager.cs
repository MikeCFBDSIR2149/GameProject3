// GameManager.cs - 游戏状态管理器

using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI.Menu
{
    public class MenuManager : MonoSingleton<MenuManager>
    {
        [Header("游戏状态")]
        [SerializeField] private GameState currentState = GameState.MainMenu;
    
        [Header("游戏设置")]
        [SerializeField] private bool isGamePaused ;
        [SerializeField] private float gameTimeScale = 1f;
        [Header("输入设置")]
        [SerializeField] private InputActionReference pauseAction; // 使用 Input Action
        [SerializeField] private InputActionReference quitAction;
        // 事件
        public event Action<GameState> OnGameStateChanged;
        public event Action<bool> OnGamePaused;
    
        protected override void Awake()
        {
            MainMenu.StartGameRequested += OnStartGameRequested;
            base.Awake();
            InitializeGame();
        }
    
        private void InitializeGame()
        {
            // 初始化为主菜单状态
            SetGameState(GameState.MainMenu);
        
            // 确保UIManager存在
            if (UIManager.Instance == null)
            {
                Debug.LogWarning("UIManager未找到，请确保场景中有UIManager");
            }
        }
        private void InitializeInput()
        {
            // 如果没有设置Input Action，创建默认的
            if (pauseAction == null)
            {
                // 创建一个临时的Input Action
                var action = new InputAction("Pause", InputActionType.Button);
                action.AddBinding("<Keyboard>/escape");
                pauseAction = InputActionReference.Create(action);
            }
            
            if (quitAction == null)
            {
                var action = new InputAction("Quit", InputActionType.Button);
                action.AddBinding("<Keyboard>/escape", groups: "MainMenu");
                quitAction = InputActionReference.Create(action);
            }
            
            // 启用输入
            pauseAction.action.Enable();
            quitAction.action.Enable();
        }
        // 设置游戏状态
        public void SetGameState(GameState newState)
        {
            if (currentState == newState) return;
        
            GameState previousState = currentState;
            currentState = newState;
        
            // 根据状态执行相应操作
            switch (newState)
            {
                case GameState.MainMenu:
                    Time.timeScale = 1f;
                    isGamePaused = false;
                    ShowMainMenu();
                    break;
                
                case GameState.Playing:
                    Time.timeScale = gameTimeScale;
                    isGamePaused = false;
                    HideAllMenus();
                    break;
                
                case GameState.Paused:
                    Time.timeScale = 0f;
                    isGamePaused = true;
                    ShowPauseMenu();
                    break;
                
                case GameState.GameOver:
                    Time.timeScale = 1f;
                    isGamePaused = false;
                    break;
            }
        
            // 触发事件
            OnGameStateChanged?.Invoke(newState);
            OnGamePaused?.Invoke(isGamePaused);
        
            Debug.Log($"游戏状态: {previousState} -> {newState}");
        }
    
        // 暂停/继续游戏
        public void TogglePause()
        {
            if (currentState == GameState.Playing)
            {
                SetGameState(GameState.Paused);
            }
            else if (currentState == GameState.Paused)
            {
                SetGameState(GameState.Playing);
            }
        }
    
        // 开始新游戏
        public void StartNewGame()
        {
            SetGameState(GameState.Playing);
            Debug.Log("开始新游戏");
        }
    
        // 返回主菜单
        public void ReturnToMainMenu()
        {
            SetGameState(GameState.MainMenu);
            Debug.Log("返回主菜单");
        }
    
        // 退出游戏
        public void QuitGame()
        {
            Debug.Log("退出游戏");
        
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
        Application.Quit();
            #endif
        }
    
        // 显示主菜单
        private void ShowMainMenu()
        {
            if (UIManager.Instance != null)
            {
                MenuData mainMenuData = new MenuData(MenuType.MainMenu);
                UIManager.Instance.ShowUI("MainMenu", mainMenuData);
            }
        }
    
        // 显示暂停菜单
        private void ShowPauseMenu()
        {
            if (UIManager.Instance != null)
            {
                MenuData pauseData = new MenuData(MenuType.PauseMenu, false);
                UIManager.Instance.ShowUI("PauseMenu", pauseData);
            }
        }
    
        // 隐藏所有菜单
        private void HideAllMenus()
        {
            if (UIManager.Instance != null)
            {
                //UIManager.Instance.HideAllMenus();
            }
        }
    
        // 获取当前状态
        public GameState GetCurrentState()
        {
            return currentState;
        }
    
        // 检查是否在游戏中
        public bool IsPlaying()
        {
            return currentState == GameState.Playing;
        }
    
        // 检查是否暂停
        public bool IsPaused()
        {
            return currentState == GameState.Paused;
        }
    
        // 更新游戏时间缩放
        public void SetTimeScale(float scale)
        {
            gameTimeScale = Mathf.Clamp(scale, 0f, 2f);
            if (currentState == GameState.Playing)
            {
                Time.timeScale = gameTimeScale;
            }
        }
    
        // 键盘输入检测
        private void Update()
        {
            
            // 使用新的 Input System 检测按键
            if (pauseAction != null && pauseAction.action.WasPressedThisFrame())
            {
                if (currentState == GameState.Playing || currentState == GameState.Paused)
                {
                    TogglePause();
                }
                else if (currentState == GameState.MainMenu)
                {
                    // 主菜单按ESC可以退出游戏
                    ShowQuitConfirmation();
                }
            }
        }
        private new void OnDestroy()
        {
            MainMenu.StartGameRequested -= OnStartGameRequested;
            // ...existing code...
        }
        private void OnStartGameRequested()
        {
            StartNewGame(); 
        }
        private void ShowQuitConfirmation()
        {
            // 这里可以显示确认对话框
            // 暂时直接退出
            QuitGame();
        }
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