using System;
using UnityEngine;

public enum EGameplayStatus
{
    Default,
    BulletTime,
    Paused,
    GameOver,
    GameWin
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

    public EGameplayStatus Status { get; private set; } = EGameplayStatus.Default;
    public EGameplayStatus PreviousStatus { get; private set; } = EGameplayStatus.Default;
    public EGameplayStatusLevel StatusLevel { get; private set; } = EGameplayStatusLevel.Playable;
    
    public bool CanPerformGameplayActions => StatusLevel == EGameplayStatusLevel.Playable;
    public bool IsTerminalState => StatusLevel == EGameplayStatusLevel.Terminal;
    
    public event Action<EGameplayStatus> OnStatusChanged;
    public event Action<Player.Player> OnPlayerChanged;

    public Player.Player Player
    {
        get => _player;
        set
        {
            if (_player == value) return;

            _player = value;
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
        Debug.Log("GameplayManager Start");
        SetGameplayStatus(EGameplayStatus.Default, true);
    }

    public void SetGameplayStatus(EGameplayStatus targetStatus, bool forceSwitch = false)
    {
        if (Status == targetStatus && !forceSwitch) return;
        PreviousStatus = Status;
        Status = targetStatus;
        StatusLevel = ResolveStatusLevel(targetStatus);
        OnStatusChanged?.Invoke(Status);
        
        switch (targetStatus)
        {
            case EGameplayStatus.Default:
                _timeScaleController.UseDefaultTimeScale();
                _energyController.UseDefaultTimeEnergyConsumption();
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
                // 游戏结束后不再消耗能量
                break;
            case EGameplayStatus.GameWin:
                _timeScaleController.UseGameOverTimeScale();
                // 游戏胜利后不再消耗能量
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(targetStatus), targetStatus, null);
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
                return EGameplayStatusLevel.Terminal;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }
    }
    
    public void SetToPreviousStatus()
    {
        if (IsTerminalState)
            return;

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

        SetGameplayStatus(EGameplayStatus.GameWin, true);
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
        SetGameplayStatus(EGameplayStatus.Default, true);
    }
}
