using UnityEngine;
using UI;

public class LevelTimer : MonoBehaviour
{
    [Header("Countdown Settings")]
    [SerializeField] private int levelTimeSeconds = 60;
    [SerializeField] private string countdownUIName = "CountDownUI";

    private CountdownUI _countdownUI;
    private float _remaining;
    private bool _isRunning;
    private bool _isPaused;
    private int _lastWholeSecond = -1;
    
    private bool _waitingForStart = false;

    private void OnEnable()
    {
        if (GameplayManager.Instance != null)
        {
            GameplayManager.Instance.OnStatusChanged += HandleGameplayStatusChanged;
            HandleGameplayStatusChanged(GameplayManager.Instance.Status);
        }
    }

    private void OnDisable()
    {
        if (GameplayManager.Instance != null)
        {
            GameplayManager.Instance.OnStatusChanged -= HandleGameplayStatusChanged;
        }
    }

    private void Start()
    {
        if (GameplayManager.Instance.StatusLevel == EGameplayStatusLevel.Playable)
        {
            StartLevelCountdown(levelTimeSeconds);
        }
        else
        {
            _waitingForStart = true;
        }
    }

    public void StartLevelCountdown(int seconds)
    {
        _remaining = Mathf.Max(0, seconds);
        _isRunning = true;
        _isPaused = false;
        _lastWholeSecond = Mathf.CeilToInt(_remaining);

        if (UIManager.Instance != null)
        {
            var data = new CountdownData
            {
                startTime = Mathf.CeilToInt(_remaining),
                message = "安全执行任务倒计时",
                completeMessage = "时间到！",
                autoHide = false,
                tickInterval = 1f
            };

            UIManager.Instance.ShowUI(countdownUIName, data);
            _countdownUI = UIManager.Instance.GetUI<CountdownUI>(countdownUIName);
            _countdownUI?.ResumeCountdown();
            _countdownUI?.SetTime(_lastWholeSecond, "安全执行任务倒计时");
        }
    }

    private void Update()
    {
        if (!_isRunning) return;

        if (_isPaused)
        {
            _countdownUI?.PauseCountdown();
            return;
        }

        _countdownUI?.ResumeCountdown();

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
            _countdownUI?.SetTime(whole, "安全执行任务倒计时");
        }
    }

    private void FinishCountdown()
    {
        _isRunning = false;
        _isPaused = false;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideUI(countdownUIName);
            _countdownUI = null;
        }

        if (GameplayManager.Instance != null)
        {
            GameplayManager.Instance.RequestGameOver();
        }
    }

    private void HandleGameplayStatusChanged(EGameplayStatus status)
    {
        if (_waitingForStart)
        {
            StartLevelCountdown(levelTimeSeconds);
            _waitingForStart = false;
            return;
        }
        if (GameplayManager.Instance != null && GameplayManager.Instance.IsTerminalState)
        {
            _isRunning = false;
            _isPaused = false;
            _countdownUI?.PauseCountdown();
            if (UIManager.Instance != null)
            {
                UIManager.Instance.HideUI(countdownUIName);
                _countdownUI = null;
            }
            return;
        }

        _isPaused = status == EGameplayStatus.Paused;

        if (_isPaused)
        {
            _countdownUI?.PauseCountdown();
        }
        else if (_isRunning)
        {
            _countdownUI?.ResumeCountdown();
        }
    }
}