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
            GameObject obj = Instantiate(prefab, transform);
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
        GameObject obj = pool.Count == 0 ? Instantiate(prefab, transform) : pool.Dequeue();
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
}
