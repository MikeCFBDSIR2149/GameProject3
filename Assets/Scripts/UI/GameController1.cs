using UI;
using UnityEngine;
using UnityEngine.InputSystem; // 添加这个命名空间

public class GameController1 : MonoBehaviour
{
    // 新的Input System变量
    private PlayerInput playerInput;
    private InputAction healthDecreaseAction;
    private InputAction healthIncreaseAction;
    private InputAction setHealthAction;
    
    private void Start()
    {
        // 初始化Input System
        InitializeInputSystem();
        
        // 示例：显示倒计时
        StartCountdownExample();
        
        // 示例：显示血量
        ShowHealthExample();
    }
    
    private void InitializeInputSystem()
    {
        // 创建PlayerInput组件
        playerInput = gameObject.AddComponent<PlayerInput>();
        
        // 创建输入动作
        var actions = new InputActionAsset();
        
        // 创建动作Map
        var gameplayMap = new InputActionMap("Gameplay");
        
        // 创建动作
        healthDecreaseAction = gameplayMap.AddAction("DecreaseHealth", 
            InputActionType.Button, "<Keyboard>/h");
        healthIncreaseAction = gameplayMap.AddAction("IncreaseHealth", 
            InputActionType.Button, "<Keyboard>/j");
        setHealthAction = gameplayMap.AddAction("SetHealth", 
            InputActionType.Button, "<Keyboard>/k");
        
        // 绑定回调
        healthDecreaseAction.performed += ctx => OnDecreaseHealth();
        healthIncreaseAction.performed += ctx => OnIncreaseHealth();
        setHealthAction.performed += ctx => OnSetHealth();
        
        // 启用动作
        gameplayMap.Enable();
        
        Debug.Log("Input System初始化完成");
    }
    
    // 使用新Input System的输入处理
    private void OnDecreaseHealth()
    {
        var healthUI = UIManager.Instance.GetUI<HealthUI>("HealthUI");
        if (healthUI != null)
        {
            healthUI.DecreaseHealth(1);
            Debug.Log("按H键：血量-1");
        }
    }
    
    private void OnIncreaseHealth()
    {
        var healthUI = UIManager.Instance.GetUI<HealthUI>("HealthUI");
        if (healthUI != null)
        {
            healthUI.IncreaseHealth(1);
            Debug.Log("按J键：血量+1");
        }
    }
    
    private void OnSetHealth()
    {
        var healthUI = UIManager.Instance.GetUI<HealthUI>("HealthUI");
        if (healthUI != null)
        {
            //healthUI.SetHealth(5);
            Debug.Log("按K键：设置血量为5");
        }
    }
    
    private void StartCountdownExample()
    {
        CountdownData countdownData = new CountdownData
        {
            startTime = 5,
            message = "游戏即将开始",
            completeMessage = "战斗开始！",
            autoHide = true,
            hideDelay = 2f
        };
        
        // 显示倒计时UI
        var ui = UIManager.Instance.ShowUI("CountdownUI", countdownData);
        
        if (ui == null)
        {
            Debug.LogError("无法显示CountdownUI，请检查预制体设置");
        }
        else
        {
            Debug.Log("成功显示CountdownUI");
        }
        
        // 监听倒计时事件
        UIEventManager.AddListener("CountdownTick", OnCountdownTick);
        UIEventManager.AddListener("CountdownComplete", OnCountdownComplete);
    }
    
    private void ShowHealthExample()
    {
        HealthData healthData = new HealthData
        {
            currentHealth = 8,
            maxHealth = 10
        };
        
        // 显示血量UI
        var ui = UIManager.Instance.ShowUI("HealthUI", healthData);
        
        if (ui == null)
        {
            Debug.LogError("无法显示HealthUI，请检查预制体设置");
        }
        else
        {
            Debug.Log("成功显示HealthUI");
        }
        
        // 监听血量事件
        UIEventManager.AddListener("HealthChanged", OnHealthChanged);
        UIEventManager.AddListener("HealthDepleted", OnHealthDepleted);
    }
    
    // 事件处理函数
    private void OnCountdownTick(object data)
    {
        int remainingTime = (int)data;
        Debug.Log($"倒计时: {remainingTime}秒");
    }
    
    private void OnCountdownComplete(object data)
    {
        CountdownData countdownData = data as CountdownData;
        Debug.Log($"倒计时完成: {countdownData.completeMessage}");
    }
    
    private void OnHealthChanged(object data)
    {
        int currentHealth = (int)data;
        Debug.Log($"当前血量: {currentHealth}");
    }
    
    private void OnHealthDepleted(object data)
    {
        Debug.Log("血量耗尽！游戏结束");
    }
    
    private void OnDestroy()
    {
        // 清理Input System
        if (healthDecreaseAction != null) healthDecreaseAction.Dispose();
        if (healthIncreaseAction != null) healthIncreaseAction.Dispose();
        if (setHealthAction != null) setHealthAction.Dispose();
    }
}