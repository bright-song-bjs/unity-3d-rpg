using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public enum EnemyStates {GUARD , PATROL , CHASE , DEAD}


[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterStats))]
public class EnemyController : MonoBehaviour
{
    #region Editor Values
    
    [Header("System Settings")]
    public bool RefreshTimeOverride;
    
    [Tooltip("Only valid when RefreshTimeOverride is checked")]
    public float RefreshTime;
    
    [Space]
    
    [Header("Basic Settings")]
    
    [Tooltip("True:Guard , False:Patrol")]
    public bool IsGuard;
    
    [Tooltip("The radius in which the player will be sighted (Red)")]
    public float SightRadius;
    
    [Tooltip("The Viewing angle of the enemy ( 0 ~ 360 )")]
    public float ViewingAngle;
    

    [Space]
    
    [Header("Patrol Settings")]
    
    [Tooltip("The radius of patrol area (Blue)")]
    public float PatrolRange;
    
    [Tooltip("The interval between patrols")]
    public float PatrolInterval;
    
    [Tooltip("The maximum offset of PatrolInterval")]
    public float PatrolIntervalOffset;
    
    #endregion

    #region Values
    private Coroutine SlowUpdateCoroutine;
    private Coroutine Rotating;
    private NavMeshAgent agent;
    protected CharacterStats characterStats;
    private Collider col;
    private float actualRefreshTime;
    [HideInInspector]
    public EnemyStates enemyStates;
    protected GameObject attackTarget;
    private float speed;
    private Animator anim;
    private bool isWalk;
    private bool isChase;
    private bool isFollow;
    private bool isRotating;
    private bool playerDead;
    private bool isDead;
    private Vector3 wayPoint;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private float remainInterval;
    private bool intervalSet;
    private float distanceToPlayer;
    [HideInInspector]
    public float closeAttackCdRemain;
    [HideInInspector]
    public float remoteAttackCdRemain;
    private bool startIsCalled;
    [HideInInspector]
    public bool getHurt;
    protected double viewingThreshold;
    private float HealthBarShowingDistance;
    private HealthBarUI healthBarUI;
        
    #endregion

    #region Unity Functions
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        col = GetComponent<Collider>();
    }

    private void Start()
    {
        //if the enemy is killed before, than it won't appear again 
        String key = SceneManager.GetActiveScene().name + name + characterStats.characterBaseStatsData.name;
        if (GameManager.Instance.LoadData(key, characterStats.characterBaseStatsData))
            if(characterStats.CurrentHealth == 0)
                Destroy(gameObject);
        
        if (RefreshTimeOverride)
            actualRefreshTime = RefreshTime;
        else actualRefreshTime = GameManager.Instance.EnemyRefreshTime;

        GameManager.Instance.playerController.OnPlayerDead += Victory;
        speed = agent.speed;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        closeAttackCdRemain = 0f;
        remoteAttackCdRemain = 0f;
        intervalSet = false;
        isRotating = false;
        playerDead = false;
        viewingThreshold = Math.Cos(ViewingAngle * 0.5);
        HealthBarShowingDistance = GetComponent<HealthBarUI>().ShowingRadius;
        healthBarUI = GetComponent<HealthBarUI>();
        if (IsGuard)
        {
            enemyStates = EnemyStates.GUARD;
        }
        else
        {
            enemyStates = EnemyStates.PATROL;
            GetNewWayPoint();
        }
        startIsCalled = true;

        //this StartCoroutine must be called at the end of Start function
        SlowUpdateCoroutine = StartCoroutine(SlowUpdate());
    }

    private void OnEnable()
    {
        if(startIsCalled)
            GameManager.Instance.playerController.OnPlayerDead += Victory;
        if (SlowUpdateCoroutine != null)
            SlowUpdateCoroutine = StartCoroutine(SlowUpdate());
    }

    private void OnDisable()
    {
        if(GameManager.IsInitialized)
            GameManager.Instance.playerController.OnPlayerDead -= Victory;
        if(SlowUpdateCoroutine != null)
            StopCoroutine(SlowUpdateCoroutine);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position , PatrolRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position , SightRadius);
    }

    #endregion

    #region Custom Functions
    
    private IEnumerator SlowUpdate()
    {
        while (true)
        {
            if (!playerDead)
            {
                SwitchStates();
                SwitchAnimation();
                closeAttackCdRemain -= actualRefreshTime;
                remoteAttackCdRemain -= actualRefreshTime;
            }
            //put code above this comment
            yield return new WaitForSeconds(actualRefreshTime);
        }
    }
    
    private void SwitchStates()
    {
        if(isDead)
            return;
        
        if (characterStats.CurrentHealth == 0)
        {
            enemyStates = EnemyStates.DEAD;
            isDead = true;
        }
        else 
        {
            if(DetectPlayer())
                enemyStates = EnemyStates.CHASE;
        }

        switch (enemyStates)
        {
            case EnemyStates.GUARD:
                Guard();
                break;
            case EnemyStates.PATROL:
                Patrol();
                break;
            case EnemyStates.CHASE:
                Chase();
                break;
            case EnemyStates.DEAD:
                Dead();
                break;
        }
    }

    private void SwitchAnimation()
    {
        anim.SetBool("Walk" , isWalk);
        anim.SetBool("Chase" , isChase);
        anim.SetBool("Follow" , isFollow);
        anim.SetBool( "Critical" , characterStats.isCritical);
        if(characterStats.closeAttack)
            anim.SetTrigger("CloseAttack");
        if(characterStats.remoteAttack)
            anim.SetTrigger("RemoteAttack");
        //those CharacterStats bool values could be set at animation script
        characterStats.closeAttack = false;
        characterStats.remoteAttack = false;
    }

    
    private bool DetectPlayer()
    {
        bool temp1 = false;
        bool temp2 = false;
        var colliders = Physics.OverlapSphere(transform.position, Math.Max(SightRadius, HealthBarShowingDistance));
        foreach (var target in colliders)
        {
            if (target.CompareTag("Player"))
            {
                //detect whether player is within HealthBarShowingDistance
                float distance = Vector3.SqrMagnitude(target.transform.position - transform.position);
                if (distance <= Mathf.Pow(HealthBarShowingDistance, 2))
                {
                    temp1 = true;
                    if(healthBarUI.healthBar != null)
                        healthBarUI.healthBar.SetActive(true);
                }

                //detect whether player is within SightRadius
                if (distance <= Mathf.Pow(SightRadius, 2))
                {
                    temp2 = true;
                    attackTarget = target.gameObject;
                    distanceToPlayer = (float)Math.Sqrt(distance);
                }
                break;
            }
        }
        if((healthBarUI.healthBar != null) && !temp1)
            healthBarUI.healthBar.SetActive(false);
        
        if(temp2)
            return transform.IsFacingTarget(attackTarget.transform, viewingThreshold);
        return false;
    }

    private void GetNewWayPoint()
    {
        NavMeshHit hit;
        do
        {
            Vector3 temp = Random.insideUnitCircle * PatrolRange;
            //TODO figure out how to solve this problem 
            Vector3 randomPosition = (new Vector3(originalPosition.x + temp.x , transform.position.y , originalPosition.z + temp.y));
            wayPoint = randomPosition;
        } while ( !(NavMesh.SamplePosition(wayPoint , out hit ,  0.1f , 1)) );
    }

    protected bool TargetInCloseAttackRange()
    {
        if(distanceToPlayer <= characterStats.CloseAttackRange)
            return true;
        return false;
    }

    protected bool TargetInRemoteAttackRange()
    {
        if (distanceToPlayer <= characterStats.RemoteAttackRange)
            return true;
        return false;
    }

    private IEnumerator RotateToOrigin()
    {
        isRotating = true;
        while (Math.Abs(transform.rotation.eulerAngles.y - originalRotation.eulerAngles.y) > 3f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation , originalRotation , (0.6f * Time.deltaTime));
            yield return null;
        }
        isRotating = false;
    }

    private IEnumerator RemoveHealthBar()
    {
        CanvasGroup canvasGroup = healthBarUI.healthBar.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        yield return new WaitForSeconds(0.5f);
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime / 1f;
            yield return null;
        }
        healthBarUI.healthBar.SetActive(false);
    }

    private void Guard()
    {
        isChase = false;
        if (transform.position != originalPosition)
        {
            if (Vector3.SqrMagnitude(transform.position - originalPosition) > (agent.stoppingDistance) * (agent.stoppingDistance))
            {
                isWalk = true;
                agent.isStopped = false;
                agent.destination = originalPosition;
            }
            else
            {
                isWalk = false;
                if (!isRotating)
                    Rotating = StartCoroutine(RotateToOrigin());
                agent.isStopped = true;
            }
        }
        else agent.isStopped = true;
    }

    private void Patrol()
    {
        isChase = false;
        agent.isStopped = false;
        agent.speed = speed * 0.5f;
        if (Vector3.SqrMagnitude(wayPoint - transform.position) <= (agent.stoppingDistance) * (agent.stoppingDistance))
        {
            isWalk = false;
            if (!intervalSet)
            {
                remainInterval = Random.Range((PatrolInterval - PatrolIntervalOffset), (PatrolInterval + PatrolIntervalOffset));
                intervalSet = true;
            }
            
            if (remainInterval >= 0)
                remainInterval -= actualRefreshTime;
            else GetNewWayPoint();
        }
        else
        {
            isWalk = true;
            intervalSet = false;
            agent.destination = wayPoint;
        }
        
    }
    
    private void Chase()
    {
        isWalk = false;
        isChase = true;
        agent.isStopped = false;
        agent.speed = speed;
        if (isRotating)
        {
            isRotating = false;
            StopCoroutine(Rotating);
        }
        if (Vector3.SqrMagnitude(transform.position - attackTarget.transform.position) > SightRadius * SightRadius)
        {
            
            isFollow = false;
            //back to the previous state
            if (!intervalSet)
            {
                remainInterval = Random.Range((PatrolInterval - PatrolIntervalOffset), (PatrolInterval + PatrolIntervalOffset));
                intervalSet = true;
            }
            
            if (remainInterval >= 0)
            {
                agent.destination = transform.position;
                remainInterval -= actualRefreshTime;
            }
            else if (IsGuard)
            {
                intervalSet = false;
                enemyStates = EnemyStates.GUARD;
            }
            else
            {
                intervalSet = false;
                enemyStates = EnemyStates.PATROL;
            }
        }
        else
        {
            if (TargetInCloseAttackRange())
            {
                isFollow = false;
                if ((closeAttackCdRemain <= 0) && (!getHurt))
                {
                    closeAttackCdRemain = characterStats.CloseAttackCoolDown;
                    CloseAttack();
                }
            }
            else if(TargetInRemoteAttackRange())
            {
                isFollow = false;
                if ((remoteAttackCdRemain <= 0) && (!getHurt))
                {
                    remoteAttackCdRemain = characterStats.RemoteAttackCoolDown;
                    RemoteAttack();
                }
            }
            else
            {
                isFollow = true;
                agent.isStopped = false;
                agent.destination = attackTarget.transform.position;
            }
        }
    }

    private void Dead()
    {
        col.enabled = false;
        anim.SetBool("Death" , true);
        StartCoroutine(RemoveHealthBar());
        String key = SceneManager.GetActiveScene().name + name + characterStats.characterBaseStatsData.name;
        GameManager.Instance.SaveData(key , characterStats.characterBaseStatsData);
        Destroy(gameObject , 2f);
    }

    private void CloseAttack()
    {
        transform.LookAt(attackTarget.transform.position);
        characterStats.isCritical = (Random.value <= characterStats.CriticalChance);
        characterStats.closeAttack = true;
    }

    private void RemoteAttack()
    {
        transform.LookAt(attackTarget.transform.position);
        characterStats.isCritical = (Random.value <= characterStats.CriticalChance);
        characterStats.remoteAttack = true;
    }

    private void Victory()
    {
        isChase = false;
        isWalk = false;
        playerDead = true;
        anim.SetBool("Victory" , true);
    }
    
    //Animation event
    public void CloseHit()
    {
        if (TargetInCloseAttackRange() && transform.IsFacingTarget(attackTarget.transform , viewingThreshold) && (!getHurt))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            CharacterStats.TakeDamage_Close(characterStats , targetStats);
        }
    }

    //Animation event
    public void RemoteHit()
    {
        if (TargetInRemoteAttackRange() && transform.IsFacingTarget(attackTarget.transform , viewingThreshold) && (!getHurt))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            CharacterStats.TakeDamage_Remote(characterStats , targetStats);
        }
    }
    
    #endregion
}
