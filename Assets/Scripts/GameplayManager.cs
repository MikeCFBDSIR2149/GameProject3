using System;
using UnityEngine;

public enum EGameplayStatus
{
    Default,
    BulletTime,
    Paused
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
            default:
                throw new ArgumentOutOfRangeException(nameof(targetStatus), targetStatus, null);
        }
    }
    
    public void SetToPreviousStatus()
    {
        SetGameplayStatus(PreviousStatus);
        PreviousStatus = Status;
    }
}
