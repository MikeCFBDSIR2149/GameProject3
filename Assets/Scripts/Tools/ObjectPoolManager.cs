using UnityEngine;
using System.Collections.Generic;

public class ObjectPoolManager : MonoSingleton<ObjectPoolManager>
{
    private readonly Dictionary<string, ObjectPool> pools = new Dictionary<string, ObjectPool>();

    public void RegisterPool(string poolKey, ObjectPool pool)
    {
        if (string.IsNullOrEmpty(poolKey) || pool == null)
        {
            Debug.LogWarning("[ObjectPoolManager] Invalid pool registration request.");
            return;
        }

        if (pools.TryGetValue(poolKey, out ObjectPool existingPool) && existingPool != null && existingPool != pool)
        {
            Debug.LogWarning($"[ObjectPoolManager] Pool with key {poolKey} already registered. Replacing stale pool reference.");
        }

        pools[poolKey] = pool;
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
    
    /// <summary>
    /// 清空所有对象池中的对象引用（场景切换时调用）
    /// 这会重置所有对象池，确保不持有对已销毁场景中对象的引用
    /// </summary>
    public void ClearAllPools()
    {
        foreach (var pool in pools.Values)
        {
            if (pool != null)
            {
                pool.Clear();
            }
        }

        pools.Clear();
        
        Debug.Log("[ObjectPoolManager] Cleared all object pools");
    }
}
