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
        
        private Canvas mainCanvas;
        public HealthUI healthUI;
        
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
            
            // Debug.Log($"UIManager初始化完成，已加载 {uiPrefabDictionary.Count} 个UI预制体");
        }
        
        private void RegisterUIPrefab(string uiName, GameObject prefab)
        {
            if (prefab != null)
            {
                uiPrefabDictionary[uiName] = prefab;
            }
        }
        
        // 显示UI（增强版）
        public UIBase ShowUI(string uiName, object data = null, bool asRootCanvas = false)
        {
            Debug.Log($"尝试显示UI: {uiName}");
            
            
            // 如果UI已经存在，直接显示
            if (uiDictionary.TryGetValue(uiName, out UIBase existingUI))
            {
                existingUI.OnShow(data);
                
                // existing UI: simply show it
                
                return existingUI;
            }
            
            // 动态创建UI
            return CreateUI(uiName, data, asRootCanvas);
        }
        
        private UIBase CreateUI(string uiName, object data = null, bool asRootCanvas = false)
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
            GameObject uiObj;
            if (asRootCanvas && prefab.GetComponent<Canvas>() != null)
            {
                // 作为根Canvas实例化
                uiObj = Instantiate(prefab);
            }
            else
            {
                // 默认作为mainCanvas子物体
                uiObj = Instantiate(prefab, mainCanvas.transform);
            }
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
            
            // created UI: initialized and shown
            
            Debug.Log($"成功创建UI: {uiName}");
            return ui;
        }
        public UIBase CreateUIInstance(string uiName, object data = null, Transform parent = null, bool asRootCanvas = false)
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
            GameObject uiObj;
            if (asRootCanvas && prefab.GetComponent<Canvas>() != null)
            {
                uiObj = Object.Instantiate(prefab);
            }
            else
            {
                uiObj = Object.Instantiate(prefab, targetParent);
            }
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
                
                // simply hide the UI
            }
        }
        
        // 隐藏所有菜单
        public void HideAllMenus()
        {
            // Hide all UI entries that are currently tracked
            List<string> toHide = new List<string>(uiDictionary.Keys);
            foreach (var name in toHide)
            {
                HideUI(name);
            }
            Debug.Log("隐藏所有UI");
        }
        
        // 隐藏顶层菜单
        public void HideTopMenu()
        {
            Debug.Log("HideTopMenu is no longer supported when menu stack is removed.");
        }
        
        // 返回上一级菜单
        public void GoBack()
        {
            Debug.Log("GoBack is no longer supported when menu stack is removed.");
        }
        
        // 获取当前顶层菜单
        public UIBase GetCurrentUI(string uiName)
        {
            if (uiDictionary.TryGetValue(uiName, out UIBase ui))
            {
                return ui;
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
                Debug.Log($"销毁UI: {uiName}");
            }
        }
        public void DestroyUIInstance(UIBase ui)
        {
            if (ui == null) return;

            // 如果是通过 ShowUI 创建的单例 UI，还需要从 uiDictionary 里移除
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
            }

            ui.OnHide();
            Object.Destroy(ui.gameObject);
        }
        
        /// <summary>
        /// 清空所有缓存的 UI 引用（场景切换时调用）
        /// </summary>
        public void ClearAllCachedUI()
        {
            // 隐藏所有 UI
            List<string> toHide = new List<string>(uiDictionary.Keys);
            foreach (var name in toHide)
            {
                if (uiDictionary.TryGetValue(name, out var ui))
                {
                    try
                    {
                        ui.OnHide();
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"[UIManager] Error hiding UI {name}: {e}");
                    }
                }
            }
            
            // 清空字典
            uiDictionary.Clear();
            uiPrefabDictionary.Clear();
            
            Debug.Log("[UIManager] Cleared all cached UI references");
        }

        private void OnEnable()
        {
            // 订阅场景切换前事件，使用事件驱动清理
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.BeforeSceneLoad += ClearAllCachedUI;
            }
        }

        private void OnDisable()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.BeforeSceneLoad -= ClearAllCachedUI;
            }
        }
    }
}