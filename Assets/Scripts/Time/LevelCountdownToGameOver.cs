using UnityEngine;
using UI;
using UI.Menu;
using UnityEngine.Playables;

public class LevelCountdownToGameOver : MonoBehaviour
{
    [Header("Countdown Settings")]
    [SerializeField] private int levelTimeSeconds = 60;
    [SerializeField] private string countdownUIName = "CountDownUI";

    [Header("GameOver UI Settings")]
    [SerializeField] private string gameOverUIName = "GameOverMenu";

    [Header("Behavior")]
    [Tooltip("如果为 true：检测到 Time.timeScale==0 就认为在菜单/暂停，从而暂停倒计时。")]
    [SerializeField] private bool pauseWhenTimeScaleZero = true;

    private CountdownUI _countdownUI;
    private float _remaining;
    private bool _isRunning;
    private bool _manuallyPaused; // 来自菜单事件的暂停状态（可选）
    private int _lastWholeSecond = -1;

    private void OnEnable()
    {
        // 可选：如果场景里有 MenuManager，则用事件驱动更准确
        // 注意：你的 MenuManager 里 OnGamePaused 是 event Action<bool>
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.OnGamePaused += HandleGamePaused;
        }

        // 可选：也可以监听 CountdownUI 自己触发的完成事件（如果你想统一走事件）
        UIEventManager.AddListener("CountdownComplete", OnCountdownCompleteEvent);
    }

    private void OnDisable()
    {
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.OnGamePaused -= HandleGamePaused;
        }

        UIEventManager.RemoveListener("CountdownComplete", OnCountdownCompleteEvent);
    }

    private void Start()
    {
        StartLevelCountdown(levelTimeSeconds);
        Debug.Log("LevelCountdownToGameOver Start");
    }

    public void StartLevelCountdown(int seconds)
    {
        _remaining = Mathf.Max(0, seconds);
        _isRunning = true;

        // 显示倒计时 UI（如果你不想显示UI，可以删掉这一段）
        if (UIManager.Instance != null)
        {
            var data = new CountdownData
            {
                startTime = Mathf.CeilToInt(_remaining),
                message = "剩余时间",
                completeMessage = "时间到！",
                autoHide = false,
                tickInterval = 1f
            };

            UIManager.Instance.ShowUI(countdownUIName, data);
            _countdownUI = UIManager.Instance.GetUI<CountdownUI>(countdownUIName);
        }

        _lastWholeSecond = Mathf.CeilToInt(_remaining);
    }

    private void Update()
    {
        if (!_isRunning) return;

        // 统一判断：菜单暂停（两种来源）
        bool pausedByTimeScale = pauseWhenTimeScaleZero && Time.timeScale <= 0.0001f;
        bool isPausedNow = _manuallyPaused || pausedByTimeScale;
        if (isPausedNow)
        {
            // 让 UI 的协程也暂停（即使 timeScale=0 它本来就停，这里主要处理“菜单暂停但 timeScale 未必是0”的情况）
            _countdownUI?.PauseCountdown();
            return;
        }
        else
        {
            // 恢复 UI 协程（仅在需要时）
            _countdownUI?.ResumeCountdown();
        }

        // 用 unscaledDeltaTime 计时，避免受 timeScale 影响
        _remaining -= Time.unscaledDeltaTime;
        if (_remaining <= 0f)
        {
            _remaining = 0f;
            FinishCountdown();
            return;
        }

        int whole = Mathf.CeilToInt(_remaining);
        if (whole != _lastWholeSecond)
        {
            _lastWholeSecond = whole;
            _countdownUI?.SetTime(whole, "剩余时间"); // 只更新数字与文案，不要重启协程
        }
    }

    private void FinishCountdown()
    {
        _isRunning = false;

        // 显示 GameOver
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowUI(gameOverUIName);
        }

        // 如果你希望同时切换游戏状态（可选）
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.SetGameState(UI.Menu.GameState.GameOver);
        }
    }

    private void HandleGamePaused(bool paused)
    {
        _manuallyPaused = paused;

        if (paused) _countdownUI?.PauseCountdown();
        else _countdownUI?.ResumeCountdown();
    }

    private void OnCountdownCompleteEvent(object data)
    {
        // 这里是“如果倒计时完成事件被别的地方触发”，也可以兜底触发 GameOver
        // 为避免重复，这里仅当本脚本还在 running 时才响应
        if (_isRunning)
        {
            FinishCountdown();
        }
    }
}