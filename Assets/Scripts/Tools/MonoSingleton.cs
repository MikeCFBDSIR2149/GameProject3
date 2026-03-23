using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    // ReSharper disable once StaticMemberInGenericType
    private static bool isShuttingDown;

    public static T Instance
    {
        get
        {
            if (isShuttingDown)
            {
                // Debug.LogWarning($"[MonoSingleton] Instance '{typeof(T)}' already destroyed. Returning null.");
                return null;
            }
            if (instance == null)
            {
                GameObject go = new GameObject(typeof(T).Name);
                instance = go.AddComponent<T>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        isShuttingDown = true;
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
        isShuttingDown = true;
    }
}
