using UnityEngine;
using System.Collections.Generic;

public class ObjectPoolManager : MonoSingleton<ObjectPoolManager>
{
    private readonly Dictionary<string, ObjectPool> pools = new Dictionary<string, ObjectPool>();

    public void RegisterPool(string poolKey, ObjectPool pool)
    {
        if (!pools.TryAdd(poolKey, pool))
        {
            Debug.LogWarning($"[ObjectPoolManager] Pool with key {poolKey} already registered.");
        }
    }

    public GameObject Get(string poolKey, Vector3 position, Quaternion rotation)
    {
        if (pools.TryGetValue(poolKey, out ObjectPool pool))
        {
            return pool.Get(position, rotation);
        }
        Debug.LogWarning($"[ObjectPoolManager] No pool found for key {poolKey}.");
        return null;
    }

    public void Dispose(string poolKey, GameObject obj)
    {
        if (pools.TryGetValue(poolKey, out ObjectPool pool))
        {
            pool.Dispose(obj);
        }
        else
        {
            Debug.LogWarning($"[ObjectPoolManager] No pool found for key {poolKey}.");
            Destroy(obj);
        }
    }
}
