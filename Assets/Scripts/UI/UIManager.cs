using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI
{
    public class UIManager : MonoSingleton<UIManager>
    {
        // 单例由MonoSingleton基类管理
        
        [Header("UI预制体")]
        [SerializeField] private GameObject countdownUIPrefab;
        [SerializeField] private GameObject healthUIPrefab;
        
        // 新增菜单预制体
        [Header("菜单预制体")]
        [SerializeField] private GameObject mainMenuPrefab;
        [SerializeField] private GameObject pauseMenuPrefab;
        [SerializeField] private GameObject settingsMenuPrefab;
        [SerializeField] private GameObject gameOverMenuPrefab;
        [SerializeField] private GameObject loadingScreenPrefab;
        [SerializeField] private GameObject confirmationDialogPrefab;
        [SerializeField] private GameObject creditsMenuPrefab;
        [SerializeField] private GameObject levelSelectMenuPrefab;
        [SerializeField] private GameObject energyUIPrefab;
        [SerializeField] private GameObject highlightRingPrefab;
        private Dictionary<string, UIBase> uiDictionary = new Dictionary<string, UIBase>();
        private Dictionary<string, GameObject> uiPrefabDictionary = new Dictionary<string, GameObject>();
        
        // 新增：菜单栈管理
        private Stack<string> menuStack = new Stack<string>();
        private Canvas mainCanvas;
        public HealthUI healthUI;
        // 新增：事件
        public event System.Action<string> OnMenuShown;
        public event System.Action<string> OnMenuHidden;
        
        protected override void Awake()
        {
            base.Awake();
            Initialize();
        }
        //调用血量UI变化
        public void SetPlayerHealth(float value)
        {
            healthUI.SetHealth(value);
        }
        // 示例角色血量变化时
        // UIManager.Instance.SetPlayerHealth(newHealthValue);
        private void Initialize()
        {
            // 初始化UI预制体字典
            RegisterUIPrefab("CountdownUI", countdownUIPrefab);
            RegisterUIPrefab("HealthUI", healthUIPrefab);
            // 注册菜单预制体
            RegisterUIPrefab("MainMenu", mainMenuPrefab);
            RegisterUIPrefab("PauseMenu", pauseMenuPrefab);
            RegisterUIPrefab("SettingsMenu", settingsMenuPrefab);
            RegisterUIPrefab("GameOverMenu", gameOverMenuPrefab);
            RegisterUIPrefab("LoadingScreen", loadingScreenPrefab);
            RegisterUIPrefab("ConfirmationDialog", confirmationDialogPrefab);
            RegisterUIPrefab("CreditsMenu", creditsMenuPrefab);
            RegisterUIPrefab("LevelSelectMenu", levelSelectMenuPrefab);
            RegisterUIPrefab("EnergyUIPrefab", energyUIPrefab);
            RegisterUIPrefab("HighlightRing", highlightRingPrefab);
            // 确保Canvas存在
            EnsureCanvas();
            
            Debug.Log($"UIManager初始化完成，已加载 {uiPrefabDictionary.Count} 个UI预制体");
        }
        
        private void RegisterUIPrefab(string uiName, GameObject prefab)
        {
            if (prefab != null)
            {
                uiPrefabDictionary[uiName] = prefab;
            }
        }
        
        // 显示UI（增强版）
        public UIBase ShowUI(string uiName, object data = null)
        {
            Debug.Log($"尝试显示UI: {uiName}");
            
            if (!uiPrefabDictionary.ContainsKey(uiName))
            {
                Debug.LogError($"未找到UI预制体: {uiName}");
                return null;
            }
            
            // 如果UI已经存在，直接显示
            if (uiDictionary.TryGetValue(uiName, out UIBase existingUI))
            {
                existingUI.OnShow(data);
                
                // 如果是菜单，推入栈
                if (existingUI is BaseMenu)
                {
                    menuStack.Push(uiName);
                    OnMenuShown?.Invoke(uiName);
                }
                
                return existingUI;
            }
            
            // 动态创建UI
            return CreateUI(uiName, data);
        }
        
        private UIBase CreateUI(string uiName, object data = null)
        {
            if (!uiPrefabDictionary.TryGetValue(uiName, out GameObject prefab))
            {
                Debug.LogError($"未找到UI预制体: {uiName}");
                return null;
            }
            
            // 确保有Canvas
            EnsureCanvas();
            
            // 实例化UI
            GameObject uiObj = Instantiate(prefab, mainCanvas.transform);
            UIBase ui = uiObj.GetComponent<UIBase>();
            
            if (ui == null)
            {
                Debug.LogError($"UI预制体没有UIBase组件: {uiName}");
                Destroy(uiObj);
                return null;
            }
            
            // 注册UI
            uiDictionary[uiName] = ui;
            
            // 初始化并显示
            ui.OnInit();
            ui.OnShow(data);
            
            // 如果是菜单，推入栈
            if (ui is BaseMenu)
            {
                menuStack.Push(uiName);
                OnMenuShown?.Invoke(uiName);
            }
            
            Debug.Log($"成功创建UI: {uiName}");
            return ui;
        }
        
        // 隐藏UI
        public void HideUI(string uiName)
        {
            if (uiDictionary.TryGetValue(uiName, out UIBase ui))
            {
                ui.OnHide();
                
                // 如果是菜单，从栈中移除
                if (ui is BaseMenu)
                {
                    var tempStack = new Stack<string>();
                    while (menuStack.Count > 0)
                    {
                        var item = menuStack.Pop();
                        if (item != uiName)
                        {
                            tempStack.Push(item);
                        }
                    }
                    
                    while (tempStack.Count > 0)
                    {
                        menuStack.Push(tempStack.Pop());
                    }
                    
                    OnMenuHidden?.Invoke(uiName);
                }
            }
        }
        
        // 隐藏所有菜单
        public void HideAllMenus()
        {
            List<string> menusToHide = new List<string>();
            
            foreach (var kvp in uiDictionary)
            {
                if (kvp.Value is BaseMenu)
                {
                    menusToHide.Add(kvp.Key);
                }
            }
            
            foreach (var menuName in menusToHide)
            {
                HideUI(menuName);
            }
            
            menuStack.Clear();
            Debug.Log("隐藏所有菜单");
        }
        
        // 隐藏顶层菜单
        public void HideTopMenu()
        {
            if (menuStack.Count > 0)
            {
                string topMenuName = menuStack.Pop();
                HideUI(topMenuName);
                Debug.Log($"隐藏顶层菜单: {topMenuName}");
            }
        }
        
        // 返回上一级菜单
        public void GoBack()
        {
            if (menuStack.Count > 0)
            {
                HideTopMenu();
            }
        }
        
        // 获取当前顶层菜单
        public BaseMenu GetCurrentMenu()
        {
            if (menuStack.Count > 0)
            {
                string topMenuName = menuStack.Peek();
                if (uiDictionary.TryGetValue(topMenuName, out UIBase ui))
                {
                    return ui as BaseMenu;
                }
            }
            return null;
        }
        
        private void EnsureCanvas()
        {
            if (mainCanvas == null)
            {
                mainCanvas = FindFirstObjectByType<Canvas>();
                if (mainCanvas == null)
                {
                    CreateCanvas();
                }
            }
        }
        
        private void CreateCanvas()
        {
            GameObject canvasObj = new GameObject("MainCanvas");
            mainCanvas = canvasObj.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            Debug.Log("创建了新的Canvas");
        }
        
        // 获取UI实例
        public T GetUI<T>(string uiName) where T : UIBase
        {
            if (uiDictionary.TryGetValue(uiName, out UIBase ui))
            {
                return ui as T;
            }
            return null;
        }
        
        // 销毁UI
        public void DestroyUI(string uiName)
        {
            if (uiDictionary.TryGetValue(uiName, out UIBase ui))
            {
                ui.OnHide();
                Destroy(ui.gameObject);
                uiDictionary.Remove(uiName);
                
                // 从栈中移除
                var tempStack = new Stack<string>();
                while (menuStack.Count > 0)
                {
                    var item = menuStack.Pop();
                    if (item != uiName)
                    {
                        tempStack.Push(item);
                    }
                }
                
                while (tempStack.Count > 0)
                {
                    menuStack.Push(tempStack.Pop());
                }
                
                Debug.Log($"销毁UI: {uiName}");
            }
        }
        
        // 更新：添加键盘输入检测
        // private void Update()
        // {
        //     // ESC键返回
        //     // 使用新的 Input System
        //     if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        //     {
        //         var currentMenu = GetCurrentMenu();
        //         if (currentMenu != null)
        //         {
        //             currentMenu.OnBackPressed();
        //         }
        //     }
        // }
    }
}