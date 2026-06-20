using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoSingleton<SceneTransitionManager>
{
    private GameObject _transitionObj;

    protected override void Awake()
    {
        base.Awake();

        GameObject prefab = Resources.Load<GameObject>("UI/SceneTransition");
        _transitionObj = Instantiate(prefab);
        _transitionObj.SetActive(false);
        DontDestroyOnLoad(_transitionObj);
    }

    public void TransitionToScene(string sceneName)
    {
        StartCoroutine(TransitionRoutine(sceneName));
    }
    
    public void TransitionToScene(int sceneBuildIndex)
    {
        StartCoroutine(TransitionRoutine(sceneBuildIndex));
    }

    private IEnumerator FadeIn()
    {
        _transitionObj.SetActive(true);
        yield return null;
    }

    private IEnumerator FadeOut()
    {
        _transitionObj.SetActive(false);
        yield return new WaitForSecondsRealtime(1f);
    }
    

    private IEnumerator TransitionRoutine(string sceneName)
    {
        // 1. 黑幕淡入
        yield return FadeIn();

        // 2. 开始加载场景
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        if (op == null) yield break;
        op.allowSceneActivation = false;

        // 3. 等待加载完成（到0.9）
        while (op.progress < 0.9f)
        {
            // UpdateProgress(op.progress);
            yield return null;
        }

        // 4. 允许场景激活
        op.allowSceneActivation = true;

        // 5. 等待场景真正启动（Awake/Start）
        yield return op;

        // 6. 通知 GameplayManager：场景已准备好但还未开始
        GameplayManager.Instance.SetGameplayStatus(
            EGameplayStatus.NotInitialized, true);

        // 7. 黑幕淡出
        yield return FadeOut();

        // 8. 游戏正式开始
        GameplayManager.Instance.SetGameplayStatus(
            EGameplayStatus.Default);
    }
    
    private IEnumerator TransitionRoutine(int sceneBuildIndex)
    {
        // 1. 黑幕淡入
        yield return FadeIn();

        // 2. 开始加载场景
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneBuildIndex);
        if (op == null) yield break;
        op.allowSceneActivation = false;

        // 3. 等待加载完成（到0.9）
        while (op.progress < 0.9f)
        {
            // UpdateProgress(op.progress);
            yield return null;
        }

        // 4. 允许场景激活
        op.allowSceneActivation = true;

        // 5. 等待场景真正启动（Awake/Start）
        yield return op;

        // 6. 通知 GameplayManager：场景已准备好但还未开始
        GameplayManager.Instance.SetGameplayStatus(
            EGameplayStatus.NotInitialized, true);

        // 7. 黑幕淡出
        yield return FadeOut();

        // 8. 游戏正式开始
        GameplayManager.Instance.SetGameplayStatus(
            EGameplayStatus.Default);
    }
}

