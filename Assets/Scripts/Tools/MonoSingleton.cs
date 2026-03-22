using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T Instance
    {
        get
        {
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

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
