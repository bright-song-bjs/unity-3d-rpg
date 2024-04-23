using System;
using UnityEngine;

public class PlayerGameStatsData_SO : ScriptableObject
{
    [HideInInspector]
    public Vector3 previousPosition;

    [HideInInspector]
    public Quaternion previousRotation;
    
    [HideInInspector]
    public String previousSceneName;

    [HideInInspector]
    public PlayerUI.LevelColor levelColor;

    [HideInInspector]
    public PlayerUI.Profile profile;

    [HideInInspector]
    public String nickName;
    
}
