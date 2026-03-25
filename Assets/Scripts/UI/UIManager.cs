using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI
{
    public class UIManager : MonoSingleton<UIManager>
    {
        
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
               
                // 尝试从 Resources 里加载：约定路径为 "UI/<uiName>"
                prefab = Resources.Load<GameObject>($"UI/{uiName}");
                if (prefab == null)
                {
                    Debug.LogError($"[UIManager] 未在Resources/UI下找到 {uiName} 预制体");
                    return null;
                }
                // 缓存起来，避免下次重复加载
                uiPrefabDictionary[uiName] = prefab;
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
        public UIBase CreateUIInstance(string uiName, object data = null, Transform parent = null)
        {
            // 1. 从缓存字典或 Resources 里拿到 prefab
            if (!uiPrefabDictionary.TryGetValue(uiName, out GameObject prefab) || prefab == null)
            {
                prefab = Resources.Load<GameObject>($"UI/{uiName}");
                if (prefab == null)
                {
                    Debug.LogError($"[UIManager] 未在Resources/UI下找到 {uiName} 预制体");
                    return null;
                }
                uiPrefabDictionary[uiName] = prefab;
            }

            // 2. 确保Canvas，或者允许传入自定义 parent
            EnsureCanvas();
            Transform targetParent = parent != null ? parent : mainCanvas.transform;

            // 3. 实例化
            GameObject uiObj = Object.Instantiate(prefab, targetParent);
            UIBase ui = uiObj.GetComponent<UIBase>();
            if (ui == null)
            {
                Debug.LogError($"UI预制体没有UIBase组件: {uiName}");
                Object.Destroy(uiObj);
                return null;
            }

            // 4. 这里刻意**不**放进 uiDictionary，不入菜单栈
            ui.OnInit();
            ui.OnShow(data);

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
        public void DestroyUIInstance(UIBase ui)
        {
            if (ui == null) return;

            // 如果是通过 ShowUI 创建的单例 UI，还需要从 uiDictionary/menuStack 里移除
            string keyToRemove = null;
            foreach (var kv in uiDictionary)
            {
                if (kv.Value == ui)
                {
                    keyToRemove = kv.Key;
                    break;
                }
            }
            if (keyToRemove != null)
            {
                uiDictionary.Remove(keyToRemove);

                // 从菜单栈中移除
                var tempStack = new Stack<string>();
                while (menuStack.Count > 0)
                {
                    var item = menuStack.Pop();
                    if (item != keyToRemove)
                    {
                        tempStack.Push(item);
                    }
                }
                while (tempStack.Count > 0)
                {
                    menuStack.Push(tempStack.Pop());
                }
            }

            ui.OnHide();
            Object.Destroy(ui.gameObject);
        }
        
    }
}