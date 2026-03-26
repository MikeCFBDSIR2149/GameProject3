using System;
using UnityEngine;

public enum EGameplayStatus
{
    Default,
    BulletTime
}

public class GameplayManager : MonoSingleton<GameplayManager>
{
    private TimeScaleController _timeScaleController;
    private EnergyController _energyController;

    public EGameplayStatus Status { get; private set; } = EGameplayStatus.Default;
    public event Action<EGameplayStatus> OnStatusChanged;
    
    public Player.Player Player { get; set; }

    [Header("Time Scale Settings")]
    private const float DefaultTimeScale = 1f;
    private const float BulletTimeScale = 0.2f;

    [Header("Energy Consumption Settings")]
    private const float DefaultTimeEnergyConsumption = 1f;
    private const float BulletTimeEnergyConsumption = 1.2f;

    protected override void Awake()
    {
        base.Awake();
        _timeScaleController = new TimeScaleController(DefaultTimeScale, BulletTimeScale);
        _energyController = new EnergyController(DefaultTimeEnergyConsumption, BulletTimeEnergyConsumption);
    }

    public void SetGameplayStatus(EGameplayStatus targetStatus)
    {
        if (Status == targetStatus) return;
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
            default:
                throw new ArgumentOutOfRangeException(nameof(targetStatus), targetStatus, null);
        }
    }
}
