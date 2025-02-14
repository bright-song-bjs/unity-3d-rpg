using System;
using UnityEngine;

public class Singleton<T> : MonoBehaviour  where T : Singleton<T>
{
    private static T instance;

    public static T Instance
    {
        get { return instance; }
        set { instance = value; }
    }
    public static bool IsInitialized
    {
        get { return instance != null; }
    }
    
    protected virtual void Awake()
    {
        if(instance != null)
            Destroy(this.gameObject);
        if(instance == null)
            instance = FindObjectOfType<T>();
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
