using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
   public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // 自动创建UIManager
                GameObject go = new GameObject("UIManager");
                _instance = go.AddComponent<UIManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
    
    [Header("UI预制体")]
    [SerializeField] private GameObject countdownUIPrefab;
    [SerializeField] private GameObject healthUIPrefab;
    
    private Dictionary<string, UIBase> uiDictionary = new Dictionary<string, UIBase>();
    private Dictionary<string, GameObject> uiPrefabDictionary = new Dictionary<string, GameObject>();
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Initialize()
    {
        // 初始化UI预制体字典
        if (countdownUIPrefab != null)
        {
            uiPrefabDictionary["CountdownUI"] = countdownUIPrefab;
        }
        if (healthUIPrefab != null)
        {
            uiPrefabDictionary["HealthUI"] = healthUIPrefab;
        }
        
        Debug.Log($"UIManager初始化完成，已加载 {uiPrefabDictionary.Count} 个UI预制体");
    }
    
    // 显示UI
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
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            CreateCanvas();
            canvas = FindObjectOfType<Canvas>();
        }
        
        // 实例化UI
        GameObject uiObj = Instantiate(prefab, canvas.transform);
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
        
        Debug.Log($"成功创建UI: {uiName}");
        return ui;
    }
    
    private void CreateCanvas()
    {
        GameObject canvasObj = new GameObject("MainCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
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
}
}