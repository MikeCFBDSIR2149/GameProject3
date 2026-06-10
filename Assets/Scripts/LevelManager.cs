using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Config;

public class LevelManager : MonoSingleton<LevelManager>
{
    [Header("关卡配置")]
    [SerializeField] private string levelDisplayNameConfigPath = "LevelDisplayNameConfig";

    private LevelDisplayNameConfig _displayNameConfig;

    /// <summary>
    /// 在场景加载前触发。推荐其他单例或管理器订阅这个事件来执行自身清理逻辑。
    /// 如果没有订阅者，则会回退到当前的直接清理实现以保证兼容性。
    /// 订阅示例： LevelManager.Instance.BeforeSceneLoad += MyCleanupMethod;
    /// 在对象销毁或不再需要时记得取消订阅： LevelManager.Instance.BeforeSceneLoad -= MyCleanupMethod;
    /// </summary>
    public event Action BeforeSceneLoad;

    /// <summary>
    /// 获取当前激活场景名称。
    /// </summary>
    public string CurrentSceneName => SceneManager.GetActiveScene().name;

    /// <summary>
    /// 获取当前场景的构建索引（供进度系统使用）。
    /// </summary>
    public int GetCurrentLevelIndex()
    {
        Scene active = SceneManager.GetActiveScene();
        return active.IsValid() ? active.buildIndex : -1;
    }

    protected override void Awake()
    {
        base.Awake();
        LoadDisplayNameConfig();
    }

    private void LoadDisplayNameConfig()
    {
        _displayNameConfig = Resources.Load<LevelDisplayNameConfig>(levelDisplayNameConfigPath);
        if (_displayNameConfig == null)
        {
            Debug.LogWarning($"[LevelManager] LevelDisplayNameConfig not found at path: Resources/{levelDisplayNameConfigPath}");
        }
    }

    /// <summary>
    /// 获取可用于选关界面的关卡总数（默认排除第一个场景）。
    /// </summary>
    public int GetSelectableLevelCount()
    {
        return Mathf.Max(SceneManager.sceneCountInBuildSettings - 1, 0);
    }

    /// <summary>
    /// 根据关卡序号（1, 2, 3...）获取显示名称；找不到映射时直接返回序号字符串。
    /// </summary>
    public string GetLevelDisplayName(int levelIndex)
    {
        if (_displayNameConfig != null)
        {
            return _displayNameConfig.GetDisplayName(levelIndex);
        }

        return levelIndex.ToString();
    }


    /// <summary>
    /// 按场景名切换场景。
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[LevelManager] Scene name is null or empty.");
            return;
        }

        ClearBeforeSceneLoad();
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// 按构建索引切换场景。
    /// </summary>
    public void LoadScene(int sceneBuildIndex)
    {
        if (sceneBuildIndex < 0)
        {
            Debug.LogWarning($"[LevelManager] Invalid scene build index: {sceneBuildIndex}");
            return;
        }

        ClearBeforeSceneLoad();
        SceneManager.LoadScene(sceneBuildIndex);
    }

    /// <summary>
    /// 重新加载当前场景。
    /// </summary>
    public void ReloadCurrentScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid())
        {
            Debug.LogWarning("[LevelManager] Active scene is invalid, cannot reload.");
            return;
        }

        ClearBeforeSceneLoad();
        SceneManager.LoadScene(activeScene.buildIndex);
    }

    /// <summary>
    /// 以单例对象入口进行下一场景切换（当前场景索引 + 1）。
    /// </summary>
    public bool LoadNextScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        int nextIndex = activeScene.buildIndex + 1;

        if (nextIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning($"[LevelManager] Next scene index out of range: {nextIndex}");
            return false;
        }

        ClearBeforeSceneLoad();
        SceneManager.LoadScene(nextIndex);
        return true;
    }
    
    /// <summary>
    /// 场景加载前清空 MonoSingleton 中的旧缓存
    /// 这确保 UIManager 和 ObjectPoolManager 不会持有已销毁场景对象的引用
    /// </summary>
    private void ClearBeforeSceneLoad()
    {
        Debug.Log("[LevelManager] Clearing singleton caches before scene load...");

        // 如果有订阅者，优先通过事件让各自负责清理自己的状态。
        if (BeforeSceneLoad != null)
        {
            try
            {
                BeforeSceneLoad.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LevelManager] Exception while invoking BeforeSceneLoad subscribers: {ex}");
            }

            // 已通过订阅者处理清理，直接返回以避免重复清理。
            return;
        }

        // 回退到原来的直接清理实现以保证兼容性（没有订阅者时）
        if (GameplayManager.Instance != null)
        {
            GameplayManager.Instance.SetGameplayStatus(EGameplayStatus.Default, true);
        }

        UI.Menu.PauseButton.ResetPauseState();
        
        if (UI.UIManager.Instance != null)
        {
            UI.UIManager.Instance.ClearAllCachedUI();
        }
        
        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.ClearAllPools();
        }
    }
}
