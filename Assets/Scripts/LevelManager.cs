using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoSingleton<LevelManager>
{
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

        SceneManager.LoadScene(nextIndex);
    }
}
