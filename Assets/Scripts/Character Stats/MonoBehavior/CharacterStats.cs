using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class CharacterStats : MonoBehaviour
{
    private static int playerID;
    private static int enemyID;
    private String characterBaseStatsDataName;
    
    [Tooltip("Each player or enemy instantiated from this prefab will have a private copy of this BaseStatsData")]
    public CharacterBaseStatsData_SO TemplateData;
    
    [HideInInspector]
    public CharacterBaseStatsData_SO characterBaseStatsData;
    
    [HideInInspector]
    public bool isCritical;
    
    [HideInInspector]
    public bool closeAttack;
    
    [HideInInspector]
    public bool remoteAttack;
    
    public event Action UpdateHealthUI;
    public event Action UpdateExpLevelUI;
    

    #region Read From CharacterBaseStatsData_SO
    public int MaxHealth
    {
        get { if (characterBaseStatsData != null) return characterBaseStatsData.MaxHealth; else return 0; }
        set { characterBaseStatsData.MaxHealth = value; }
    }
    
    public int CurrentHealth
    {
        get { if (characterBaseStatsData != null) return characterBaseStatsData.CurrentHealth; else return 0; }
        set { characterBaseStatsData.CurrentHealth = value; }
    }
    
    public int MaxDefence
    {
        get { if (characterBaseStatsData != null) return characterBaseStatsData.MaxDefence; else return 0; }
        set { characterBaseStatsData.MaxDefence = value; }
    }
    
    public int CurrentDefense
    {
        get { if (characterBaseStatsData != null) return characterBaseStatsData.CurrentDefence; else return 0; }
        set { characterBaseStatsData.CurrentDefence = value; }
    }
    
    public int MaxExp
    {
        get { if (characterBaseStatsData != null) return characterBaseStatsData.MaxExp; else return 0; }
        set { characterBaseStatsData.MaxExp = value; }
    }
    
    public int CurrentExp
    {
        get { if (characterBaseStatsData != null) return characterBaseStatsData.CurrentExp; else return 0; }
        set { characterBaseStatsData.CurrentExp = value; }
    }
    
    public int MaxLevel
    {
        get { if (characterBaseStatsData != null) return characterBaseStatsData.MaxLevel; else return 0; }
        set { characterBaseStatsData.MaxLevel = value; }
    }
    
    public int CurrentLevel
    {
        get { if (characterBaseStatsData != null) return characterBaseStatsData.CurrentLevel; else return 0; }
        set { characterBaseStatsData.CurrentLevel = value; }
    }
    
    public int KillPoint
    {
        get { if (characterBaseStatsData != null) return characterBaseStatsData.KillPoint; else return 0; }
        set { characterBaseStatsData.KillPoint = value; }
    }
    
    public float CloseAttackRange
    {
        get { if (characterBaseStatsData != null) return characterBaseStatsData.CloseAttackRange; else return 0; }
        set { characterBaseStatsData.CloseAttackRange = value; }
    }
    
    public float RemoteAttackRange
    {
        get { if (characterBaseStatsData != null) return characterBaseStatsData.RemoteAttackRange; else return 0; }
        set { characterBaseStatsData.RemoteAttackRange = value; }
    }
    
    public float CloseAttackCoolDown
    {
        get { if (characterBaseStatsData != null) return characterBaseStatsData.CloseAttackCoolDown; else return 0; }
        set { characterBaseStatsData.CloseAttackCoolDown = value; }
    }
    
    
    public float RemoteAttackCoolDown
    {
        get { if (characterBaseStatsData != null) return characterBaseStatsData.RemoteAttackCoolDown; else return 0; }
        set { characterBaseStatsData.RemoteAttackCoolDown = value; }
    }
    
    
    public int Damage_Close
    {
        get { if (characterBaseStatsData != null) return characterBaseStatsData.Damage_Close; else return 0; }
        set { characterBaseStatsData.Damage_Close = value; }
    }
    
    public int DamageOffset_Close
    {
        get { if (characterBaseStatsData != null) return characterBaseStatsData.DamageOffset_Close; else return 0; }
        set { characterBaseStatsData.DamageOffset_Close = value; }
    }
    
    public int Damage_Remote
    {
        get { if (characterBaseStatsData != null) return characterBaseStatsData.Damage_Remote; else return 0; }
        set { characterBaseStatsData.Damage_Remote = value; }
    }
    
    public int DamageOffset_Remote
    {
        get { if (characterBaseStatsData != null) return characterBaseStatsData.DamageOffset_Remote; else return 0; }
        set { characterBaseStatsData.DamageOffset_Remote = value; }
    }
    
    public float CriticalMultiplier
    {
        get { if (characterBaseStatsData != null) return characterBaseStatsData.CriticalMultiplier; else return 0; }
        set { characterBaseStatsData.CriticalMultiplier = value; }
    }
    
    public float CriticalChance
    {
        get { if (characterBaseStatsData != null) return characterBaseStatsData.CriticalChance; else return 0; }
        set { characterBaseStatsData.CriticalChance = value; }
    }
    
    public float EnemyStiffness
    {
        get { if (characterBaseStatsData != null) return characterBaseStatsData.EnemyStiffness; else return 0; }
        set { characterBaseStatsData.EnemyStiffness = value; }
    }
    
    public float SprintInterval
    {
        get { if (characterBaseStatsData != null) return characterBaseStatsData.sprintInterval; else return 0; }
        set { characterBaseStatsData.sprintInterval = value; }
    }
    
    public float SprintForce
    {
        get { if (characterBaseStatsData != null) return characterBaseStatsData.sprintForce; else return 0; }
        set { characterBaseStatsData.sprintForce = value; }
    }
    
    public void UpdateExp(int killPoint)
    {
        while (killPoint > 0)
        {
            if ((killPoint + CurrentExp) < MaxExp)
            {
                CurrentExp += killPoint;
                UpdateExpLevelUI?.Invoke();
                return;
            }
            else
            {
                killPoint -= (MaxExp - CurrentExp);
                characterBaseStatsData.LevelUp();
            }
        }
    }
    #endregion

    #region Character Combat

    public static void TakeDamage_Close(CharacterStats attacker , CharacterStats defender)
    {
        int damage = Mathf.Max(attacker.CurrentDamage_Close() - defender.CurrentDefense, 0);
        defender.CurrentHealth = Mathf.Max(defender.CurrentHealth - damage, 0);
        if (attacker.isCritical)
        {
            defender.GetComponent<Animator>().SetTrigger("Hit");
            if (defender.CompareTag("Enemy"))
            {
                defender.GetComponent<EnemyController>().closeAttackCdRemain += attacker.EnemyStiffness;
                defender.GetComponent<EnemyController>().remoteAttackCdRemain += attacker.EnemyStiffness;
            }
        }
        
        //update defender HealthUI
        defender.UpdateHealthUI?.Invoke();
        
        //update Exp
        if (defender.CompareTag("Enemy"))
        {
            if (defender.CurrentHealth == 0)
                attacker.UpdateExp(defender.KillPoint);
        }
        else if(attacker.isCritical)
            attacker.UpdateExp((int)(defender.KillPoint * (attacker.CriticalMultiplier + 1)));
        else attacker.UpdateExp(defender.KillPoint);
    }
    
    public static void TakeDamage_Remote(CharacterStats attacker , CharacterStats defender)
    {
        int damage = Mathf.Max(attacker.CurrentDamage_Remote() - defender.CurrentDefense, 0);
        defender.CurrentHealth = Mathf.Max(defender.CurrentHealth - damage, 0);
        if (attacker.isCritical)
        {
            defender.GetComponent<Animator>().SetTrigger("Hit");
            if (defender.CompareTag("Enemy"))
            {
                defender.GetComponent<EnemyController>().closeAttackCdRemain += attacker.EnemyStiffness;
                defender.GetComponent<EnemyController>().remoteAttackCdRemain += attacker.EnemyStiffness;
            }
        }
        
        //update defender HealthUI
        defender.UpdateHealthUI?.Invoke();
        
        //update Exp 
        if (defender.CompareTag("Enemy"))
        {
            if(defender.CurrentHealth == 0)
                attacker.UpdateExp(defender.KillPoint);
        }
        else if (attacker.isCritical)
            attacker.UpdateExp((int)(defender.KillPoint * (attacker.CriticalMultiplier + 1)));
        else attacker.UpdateExp(defender.KillPoint);
    }

    public static void TakeDamage(bool isRaw , int rawDamage, CharacterStats defender , 
        bool damageIsCritical = false , float enemyStiffness = 0f , int damageOffset = 0)
    {
        int damage;
        if (isRaw)
        {
            rawDamage = Random.Range(rawDamage - damageOffset, rawDamage + damageOffset);
            damage = rawDamage;
        }
        else
        {
            rawDamage = Random.Range(rawDamage - damageOffset, rawDamage + damageOffset);
            damage = Mathf.Max(rawDamage - defender.CurrentDefense, 0);
        }
        defender.CurrentHealth = Mathf.Max(defender.CurrentHealth - damage, 0);
        if (damageIsCritical)
        {
            defender.GetComponent<Animator>().SetTrigger("Hit");
            if (defender.CompareTag("Enemy"))
            {
                defender.GetComponent<EnemyController>().closeAttackCdRemain += enemyStiffness;
                defender.GetComponent<EnemyController>().remoteAttackCdRemain += enemyStiffness;
            }
        }
        //update defender HealthUI
        defender.UpdateHealthUI?.Invoke();
        //this method doesn't need to update Exp， Rock_Golem will handle this
    }
    
    private int CurrentDamage_Close()
    {
        //Min and Max are both inclusive
        float coreDamage = Random.Range(Damage_Close - DamageOffset_Close, Damage_Close + DamageOffset_Close);
        if (isCritical)
            coreDamage *= (1f + CriticalMultiplier);
        return (int)coreDamage;
    }
    
    private int CurrentDamage_Remote()
    {
        //Min and Max are both inclusive
        float coreDamage = Random.Range(Damage_Remote - DamageOffset_Remote, Damage_Remote + DamageOffset_Remote);
        if (isCritical)
            coreDamage *= (1f + CriticalMultiplier);
        return (int)coreDamage;
    }
    
    #endregion

    private void Awake()
    {
        if (TemplateData != null)
            characterBaseStatsData = Instantiate(TemplateData);
    }

    private void Start()
    {
        isCritical = false;
        closeAttack = false;
        remoteAttack = false;
    }
}       
   