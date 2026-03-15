public class EnergyController
{
    private readonly float _defaultTimeEnergyConsumption;
    private readonly float _bulletTimeEnergyConsumption;
    
    private static float _currentEnergyConsumption;
    
    public EnergyController(float defaultTimeEnergyConsumption, float bulletTimeEnergyConsumption)
    {
        _defaultTimeEnergyConsumption = defaultTimeEnergyConsumption;
        _bulletTimeEnergyConsumption = bulletTimeEnergyConsumption;
    }

    private static void SetEnergyConsumption(float newEnergyConsumption)
    {
        _currentEnergyConsumption = newEnergyConsumption;
    }

    public void UseDefaultTimeEnergyConsumption()
    {
        SetEnergyConsumption(_defaultTimeEnergyConsumption);
    }

    public void UseBulletTimeEnergyConsumption()
    {
        SetEnergyConsumption(_bulletTimeEnergyConsumption);
    }
}
