using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character Base Stats Data", menuName = "Character Base Stats/Data")]
public class CharacterBaseStatsData_SO : ScriptableObject
{
    [Header("Level Settings")]
    
    [Tooltip("The initial level of this player or enemy when game starts")]
    public int InitialLevel;

    [Tooltip("The maximum level of this player or enemy")]
    public int MaxLevel;

    [Tooltip("The percentage any basic stat grows compared to previous level when level up " +
             "( MaxHealth = (int)(tempHealth * (1 + LevelUpBuff)) ) , " +
             "so player's LevelUpBuff has to be higher than that of the enemies")]
    public float LevelUpBuff;

    [Space]

    [Header("Lv.1 Basic Settings")]
    
    [Tooltip("The maximum health of Lv.1")]
    public int BaseHealth;

    [Tooltip("The maximum defence of Lv.1")]
    public int BaseDefence;

    [Tooltip("The maximum Exp of Lv.1")]
    public int BaseExp;

    [Space]

    [Header("Lv.1 Attack Settings")]
    
    [Tooltip("Set as 0 if doesn't have closeAttack")]
    public float CloseAttackRange;

    [Tooltip("Set as 0 if doesn't have remoteAttack")]
    public float RemoteAttackRange;

    [Tooltip("Set as 0 if doesn't have closeAttack")]
    public float CloseAttackCoolDown;

    [Tooltip("Set as 0 if doesn't have remoteAttack")]
    public float RemoteAttackCoolDown;

    [Tooltip("The basic remoteDamage has to be an integer")]
    public int BaseDamage_Close;

    [Tooltip("The raw damage will be between (Damage-DamageOffset) and (Damage + DamageOffset)")]
    public int BaseDamageOffset_Close;
    
    [Tooltip("The basic closeDamage has to be an integer")]
    public int BaseDamage_Remote;

    [Tooltip("The raw damage will be between (Damage-DamageOffset) and (Damage + DamageOffset)")]
    public int BaseDamageOffset_Remote;

    [Tooltip("Actual damage = (CriticalMultiplier + 1) * (Damage - Defence)")]
    public float CriticalMultiplier;

    [Tooltip("0 ~ 1")]
    public float CriticalChance;

    [Tooltip("The CD subtracted from enemies when player's attack is critical")]
    public float EnemyStiffness;
    
    [Tooltip("The impulse given to player when sprinting")]
    public float sprintForce;

    [Tooltip("The maximum interval between each sprint")]
    public float sprintInterval;

    [Space]

    [Header("Trophies Settings")]
    
    [Tooltip("If an enemy was killed by a player, then the player will get the same amount of Exp as the enemy's KillPoint;" +
             "If a player was hit(not critical) by an enemy, then the enemy will get the same amount of Exp as the player's KillPoint, " +
             "and this amount will multiplied by (1 + CriticalMultiplier) if the attack is critical;" +
             "so note that player's KillPoint has to be much lower than that of the enemies")]
    public int KillPoint;


    [HideInInspector]
    public int MaxHealth;

    [HideInInspector]
    public int CurrentHealth;

    [HideInInspector]
    public int MaxDefence;

    [HideInInspector]
    public int CurrentDefence;

    [HideInInspector]
    public int MaxExp;

    [HideInInspector]
    public int CurrentExp;

    [HideInInspector]
    public int CurrentLevel;

    [HideInInspector]
    public int Damage_Close;

    [HideInInspector]
    public int DamageOffset_Close;
    
    [HideInInspector]
    public int Damage_Remote;

    [HideInInspector]
    public int DamageOffset_Remote;

    
    private void Awake()
    {
        CurrentLevel = 1;
        MaxExp = BaseExp;
        CurrentExp = 0;
        MaxHealth = BaseHealth;
        MaxDefence = BaseDefence;
        CurrentHealth = MaxHealth;
        CurrentDefence = MaxDefence;
        Damage_Close = BaseDamage_Close;
        DamageOffset_Close = BaseDamageOffset_Close;
        Damage_Remote = BaseDamage_Remote;
        DamageOffset_Remote = BaseDamageOffset_Remote;

        for (int i = CurrentLevel; i < InitialLevel; i++)
            LevelUp();
    }

    public void LevelUp()
    {
        CurrentLevel += 1;
        MaxExp = (int)(MaxExp * (1 + LevelUpBuff) * (1 + (CurrentLevel / MaxLevel)));
        CurrentExp = 0;
        int tempHealth = MaxHealth;
        MaxHealth = (int)(tempHealth * (1 + LevelUpBuff));
        int tempDefence = MaxDefence;
        MaxDefence = (int)(tempDefence * (1 + LevelUpBuff));
        CurrentHealth = (int)(MaxHealth * (CurrentHealth / tempHealth));
        CurrentDefence = (int)(MaxDefence * (CurrentDefence / tempDefence));
        Damage_Close = (int)(Damage_Close * (1 + LevelUpBuff));
        DamageOffset_Close = (int)(DamageOffset_Close * (1 + LevelUpBuff) * 0.25f);
        Damage_Remote = (int)(Damage_Remote * (1 + LevelUpBuff));
        DamageOffset_Remote = (int)(DamageOffset_Remote * (1 + LevelUpBuff) * 0.25f);
    }
}
