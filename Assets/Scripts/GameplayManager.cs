using UnityEngine;

public class GameplayManager : MonoSingleton<GameplayManager>
{
    private TimeScaleController _timeScaleController;
    private EnergyController _energyController;

    [Header("Time Scale Settings")]
    private const float DefaultTimeScale = 1f;
    private const float BulletTimeScale = 0.5f;

    [Header("Energy Consumption Settings")]
    private const float DefaultTimeEnergyConsumption = 1f;
    private const float BulletTimeEnergyConsumption = 1.2f;

    protected override void Awake()
    {
        base.Awake();
        _timeScaleController = new TimeScaleController(DefaultTimeScale, BulletTimeScale);
        _energyController = new EnergyController(DefaultTimeEnergyConsumption, BulletTimeEnergyConsumption);
    }
}
