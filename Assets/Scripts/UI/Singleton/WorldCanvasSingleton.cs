using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldCanvasSingleton : Singleton<WorldCanvasSingleton>
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}
