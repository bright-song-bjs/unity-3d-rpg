using UnityEngine;

public class EventSystemSingleton : Singleton<EventSystemSingleton>
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}
