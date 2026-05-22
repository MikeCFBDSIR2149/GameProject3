using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    public GameObject prefab;
    public int initialSize = 10;
    private readonly Queue<GameObject> pool = new Queue<GameObject>();

    public string poolKey;

    private void Awake()
    {
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
        if (!string.IsNullOrEmpty(poolKey))
        {
            ObjectPoolManager.Instance.RegisterPool(poolKey, this);
        }
        else
        {
            Debug.LogWarning($"[ObjectPool] poolKey is empty, auto-register skipped.");
        }
    }

    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        GameObject obj = pool.Count == 0 ? Instantiate(prefab) : pool.Dequeue();
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.transform.parent = null;
        obj.SetActive(true);
        return obj;
    }

    public void Dispose(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
    
    /// <summary>
    /// 清空对象池中的所有对象（场景切换时调用）
    /// </summary>
    public void Clear()
    {
        while (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        
        Debug.Log($"[ObjectPool] Cleared pool for key: {poolKey}");
    }
}
