using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoSingleton<LevelManager>
{
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
    public void LoadNextScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        int nextIndex = activeScene.buildIndex + 1;

        if (nextIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning($"[LevelManager] Next scene index out of range: {nextIndex}");
            return;
        }

        ClearBeforeSceneLoad();
        SceneManager.LoadScene(nextIndex);
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
