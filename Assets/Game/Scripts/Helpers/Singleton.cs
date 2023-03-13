using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static object _lock = new object();
    private static T _instance;

    public static T Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = (T) FindObjectOfType(typeof(T));

                    if (_instance == null)
                    {
                        Debug.Log($"{typeof(T)} (Singleton) cannot be found - should be attached to some game object first");
                    }
                }

                return _instance;
            }
        }
    }

    public virtual void Reset()
    {
        _instance = null;
    }
}