using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenCanvasSingleton : Singleton<ScreenCanvasSingleton>
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}
