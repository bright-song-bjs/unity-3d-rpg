using System;
using UnityEngine;

public class Rock_Golem : MonoBehaviour
{

    #region Editor Values
    [Header("Basic Settings")]
    [Tooltip("The explosion effect (drop a particle system here)")]
    public GameObject ExplosionEffect;
    
    [Space]
    
    [Header("Damage Settings")]
    [Tooltip("The damage this rock will cause to player")]
    public int RockDamage_toPlayer;
    [Tooltip("The damage this rock will cause to Golem")]
    public int RockDamage_toGolem;
    [Tooltip("Whether this rock will directly cause death to other enemies")]
    public bool Fatal;
    [Tooltip("The damage this rock will cause to other enemies (only valid when Fatal is unchecked)")]
    public int RockDamage_toOthers;
    
    #endregion
    
    public enum RockStates {HitPlayer , HitEnemy , HitNothing}
    private Rigidbody rb;
    [HideInInspector]
    public RockStates rockStates;
    [HideInInspector]
    public bool initialized;
    private bool isFlying;

    [HideInInspector]
    public CharacterStats FromCharacterStats;
    
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        //put code above this line of code
        initialized = true;
    }

    private void FixedUpdate()
    {
        if (isFlying)
        {
            if (rb.velocity.sqrMagnitude < 1f)
            {
                rockStates = RockStates.HitNothing;
                isFlying = false;
                FromCharacterStats = null;
            }
        }
    }

    public void FlyToTarget(CharacterStats from , GameObject target , float force , float verticalOffset = 0f)
    {
        FromCharacterStats = from;
        Vector3 direction = target.transform.position - transform.position + new Vector3(0f , verticalOffset , 0f);
        direction.Normalize();
        rb.velocity = Vector3.one;
        rb.AddForce(direction * force , ForceMode.Impulse);
        isFlying = true;
    }

    public void FlyInDirection(CharacterStats from , Vector3 direction , float force , float verticalOffset = 0f)
    {
        FromCharacterStats = from;
        direction = direction + new Vector3(0f, verticalOffset, 0f);
        rb.velocity = Vector3.one;
        rb.AddForce(direction * force , ForceMode.Impulse);
        isFlying = true;
    }

    private void OnCollisionEnter(Collision other)
    {
        switch (rockStates)
        {
            case RockStates.HitPlayer:
                if (other.gameObject.CompareTag("Player"))
                {
                    isFlying = false;
                    Vector3 direction = other.gameObject.transform.position - transform.position;
                    direction = (rb.velocity + direction * 3f);
                    other.gameObject.GetComponent<PlayerController>().KickedOff(direction , rb.mass);
                    CharacterStats.TakeDamage(false , RockDamage_toPlayer , other.gameObject.GetComponent<CharacterStats>());
                    rockStates = RockStates.HitNothing;
                    
                    //update exp
                    if (FromCharacterStats != null)
                    {
                        if (FromCharacterStats.CompareTag("Enemy"))
                            FromCharacterStats.UpdateExp(other.gameObject.GetComponent<CharacterStats>().KillPoint);
                    }
                    FromCharacterStats = null;
                }
                break;
            case RockStates.HitEnemy:
                if (other.gameObject.CompareTag("Enemy"))
                {
                    isFlying = false;
                    if (other.gameObject.GetComponent<Golem>())
                        CharacterStats.TakeDamage(false , RockDamage_toGolem 
                            , other.gameObject.GetComponent<CharacterStats>() , true);
                    else
                    {
                        if (Fatal)
                            CharacterStats.TakeDamage(true , other.gameObject.GetComponent<CharacterStats>().CurrentHealth 
                                , other.gameObject.GetComponent<CharacterStats>());
                        else
                            CharacterStats.TakeDamage(false , RockDamage_toOthers 
                                , other.gameObject.GetComponent<CharacterStats>() , true);
                    }
                    //update exp
                    if (FromCharacterStats != null)
                    {
                        if(FromCharacterStats.CompareTag("Player") 
                           && other.gameObject.GetComponent<CharacterStats>().CurrentHealth ==0)
                            FromCharacterStats.UpdateExp(other.gameObject.GetComponent<CharacterStats>().KillPoint);
                    }
                    FromCharacterStats = null;
                        
                    //destroy rock
                    Instantiate(ExplosionEffect, transform.position, Quaternion.identity);
                    Destroy(gameObject);
                }
                break;
            case RockStates.HitNothing:
                break;
        }
    }
}