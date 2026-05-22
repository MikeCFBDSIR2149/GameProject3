using System;
using UnityEngine;

public enum EGameplayStatus
{
    Default,
    BulletTime,
    Paused,
    GameOver
}

public class GameplayManager : MonoSingleton<GameplayManager>
{
    private TimeScaleController _timeScaleController;
    private EnergyController _energyController;

    public EGameplayStatus Status { get; private set; } = EGameplayStatus.Default;
    public EGameplayStatus PreviousStatus { get; private set; } = EGameplayStatus.Default;
    public event Action<EGameplayStatus> OnStatusChanged;
    
    public Player.Player Player { get; set; }

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
            default:
                throw new ArgumentOutOfRangeException(nameof(targetStatus), targetStatus, null);
        }
    }
    
    public void SetToPreviousStatus()
    {
        if (Status == EGameplayStatus.GameOver)
            return;

        SetGameplayStatus(PreviousStatus);
        PreviousStatus = Status;
    }

    public void RequestGameOver()
    {
        if (Status == EGameplayStatus.GameOver)
            return;
        Debug.Log("[GameplayManager] Requesting GameOver");

        SetGameplayStatus(EGameplayStatus.GameOver, true);
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
        SetGameplayStatus(EGameplayStatus.Default, true);
    }
}
