using UnityEngine;

public class TimeScaleController
{
    private readonly float _defaultTimeScale;
    private readonly float _bulletTimeScale;

    public TimeScaleController(float defaultTimeScale, float bulletTimeScale)
    {
        _defaultTimeScale = defaultTimeScale;
        _bulletTimeScale = bulletTimeScale;
    }

    private static void SetTimeScale(float timeScale)
    {
        Time.timeScale = timeScale;
    }
    
    public void UseDefaultTimeScale()
    {
        SetTimeScale(_defaultTimeScale);
    }
    
    public void UseBulletTimeScale()
    {
        SetTimeScale(_bulletTimeScale);
    }
}
