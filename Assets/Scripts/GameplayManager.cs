using System;
using UnityEngine;
using UnityEngine.Serialization;

public enum EGameplayStatus
{
    Default,
    BulletTime,
    Paused,
    GameOver,
    GameWin,
    NotInitialized
}

public enum EGameplayStatusLevel
{
    Playable,
    Suspended,
    Terminal
}

public class GameplayManager : MonoSingleton<GameplayManager>
{
    private TimeScaleController _timeScaleController;
    private EnergyController _energyController;
    private Player.Player _player;

    // Pending pause flag: set this to true when you want to pause, and GameplayManager will handle it in Update
    private bool _pendingPause;


    public EGameplayStatus Status { get; private set; } = EGameplayStatus.NotInitialized;
    public EGameplayStatus PreviousStatus { get; private set; } = EGameplayStatus.NotInitialized;
    public EGameplayStatusLevel StatusLevel { get; private set; } = EGameplayStatusLevel.Terminal;
    
    public bool CanPerformGameplayActions => StatusLevel == EGameplayStatusLevel.Playable;
    public bool IsTerminalState => StatusLevel == EGameplayStatusLevel.Terminal;
    
    public event Action<EGameplayStatus> OnStatusChanged;
    public event Action<Player.Player> OnPlayerChanged;

    public bool doNotTriggerListener;

    public Player.Player Player
    {
        get => _player;
        set
        {
            if (_player == value) return;

            _player = value;
            
            if (_player != null) SetGameplayStatus(EGameplayStatus.Default, true);
            OnPlayerChanged?.Invoke(_player);
        }
    }

    [Header("Time Scale Settings")]
    private const float DefaultTimeScale = 5f;
    private const float BulletTimeScale = 1f;

    [Header("Energy Consumption Settings")]
    private const float DefaultTimeEnergyConsumption = 1f;
    private const float BulletTimeEnergyConsumption = 1.2f;

    protected override void Awake()
    {
        base.Awake();
        _timeScaleController = new TimeScaleController(DefaultTimeScale, BulletTimeScale);
        _energyController = new EnergyController(DefaultTimeEnergyConsumption, BulletTimeEnergyConsumption);
    }

    private void Start()
    {
        // Debug.Log("GameplayManager Start");
        SetGameplayStatus(EGameplayStatus.Default, true);
        doNotTriggerListener = false;
    }

    private void Update()
    {
        // Check if a pause was requested via RequestPause()
        if (_pendingPause)
        {
            SetGameplayStatus(EGameplayStatus.Paused, true);
            _pendingPause = false;
        }
    }

    public void SetGameplayStatus(EGameplayStatus targetStatus, bool forceSwitch = false)
    {
        if (Status == targetStatus && !forceSwitch) return;
        
        // Only update PreviousStatus if the status is actually changing
        if (Status != targetStatus)
        {
            PreviousStatus = Status;
        }
        
        Status = targetStatus;
        StatusLevel = ResolveStatusLevel(targetStatus);
        OnStatusChanged?.Invoke(Status);
        
        switch (targetStatus)
        {
            case EGameplayStatus.Default:
                _timeScaleController.UseDefaultTimeScale();
                _energyController.UseDefaultTimeEnergyConsumption();
                SetDoNotTriggerListener(false);
                break;
            case EGameplayStatus.BulletTime:
                _timeScaleController.UseBulletTimeScale();
                _energyController.UseBulletTimeEnergyConsumption();
                break;
            case EGameplayStatus.Paused:
                _timeScaleController.UsePausedTimeScale();
                // _energyController.UseDefaultTimeEnergyConsumption();
                break;
            case EGameplayStatus.GameOver:
                _timeScaleController.UseGameOverTimeScale();
                SetDoNotTriggerListener(true);
                // 游戏结束后不再消耗能量
                break;
            case EGameplayStatus.GameWin:
                _timeScaleController.UseGameOverTimeScale();
                SetDoNotTriggerListener(true);
                // 游戏胜利后不再消耗能量
                break;
            case EGameplayStatus.NotInitialized:
                _timeScaleController.UseGameOverTimeScale();
                SetDoNotTriggerListener(true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(targetStatus), targetStatus, null);
        }

        // Centralized input enabling/disabling: when gameplay is paused, disable game input; otherwise enable.
        if (Status == EGameplayStatus.Paused)
        {
            GlobalInputController.Instance?.DisableInputControllers();
        }
        else
        {
            GlobalInputController.Instance?.EnableInputControllers();
        }
    }

    private static EGameplayStatusLevel ResolveStatusLevel(EGameplayStatus status)
    {
        switch (status)
        {
            case EGameplayStatus.Default:
            case EGameplayStatus.BulletTime:
                return EGameplayStatusLevel.Playable;
            case EGameplayStatus.Paused:
                return EGameplayStatusLevel.Suspended;
            case EGameplayStatus.GameOver:
            case EGameplayStatus.GameWin:
            case EGameplayStatus.NotInitialized:
                return EGameplayStatusLevel.Terminal;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }
    }

    /// <summary>
    /// Request a gameplay pause. The pause will be applied in the next Update.
    /// This avoids state conflicts when multiple systems try to change status simultaneously.
    /// </summary>
    public void RequestPause()
    {
        _pendingPause = true;
    }

    public void SetToPreviousStatus()
    {
        if (IsTerminalState)
            return;
        if (ResolveStatusLevel(PreviousStatus) == EGameplayStatusLevel.Terminal) return;

        SetGameplayStatus(PreviousStatus);
        PreviousStatus = Status;
    }

    public void RequestGameOver()
    {
        if (IsTerminalState)
            return;
        Debug.Log("[GameplayManager] Requesting GameOver");

        SetGameplayStatus(EGameplayStatus.GameOver, true);
    }

    public void RequestGameWin()
    {
        if (IsTerminalState)
            return;
        Debug.Log("[GameplayManager] Requesting GameWin");
        // Debug.Log("[GameplayManager] IsTerminalState: " + IsTerminalState);

        SetGameplayStatus(EGameplayStatus.GameWin, true);
    }

    private void SetDoNotTriggerListener(bool value)
    {
        doNotTriggerListener = value;
    }

    private void OnEnable()
    {
        // 订阅场景切换前事件，确保在场景切换时重置到默认玩法状态
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.BeforeSceneLoad += HandleBeforeSceneLoad;
        }
    }

    private void OnDisable()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.BeforeSceneLoad -= HandleBeforeSceneLoad;
        }
    }

    private void HandleBeforeSceneLoad()
    {
        Player = null;
    }
}
