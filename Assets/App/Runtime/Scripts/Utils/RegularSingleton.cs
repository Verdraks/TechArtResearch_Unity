using UnityEngine;

public abstract class RegularSingleton<T> : MonoBehaviour where T : Component
{
    private static T s_Instance;
    public static T Instance => s_Instance;
    
    public static bool HasInstance => s_Instance != null;
    
    protected virtual void Awake()
    {
        if (s_Instance != null && s_Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        s_Instance = this as T;
    }
}
