using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{

    #region Editor Values

    [Header("Basic Settings")]

    [Tooltip("The maximum click interval for triggering sprint")]
    public float clickInterval;
    
    [Header("Attack Settings")]
    [Tooltip("The force with which player push the rock")]
    public float Force;

    #endregion
    
    #region Values
    public event Action OnPlayerDead;
    private NavMeshAgent agent;
    private Animator anim;
    private CharacterStats characterStats;
    private GameObject attackTarget;
    private float cdRemain;   
    private Coroutine attackCoroutine;
    private bool playerDead;
    private bool startIsCalled;
    
    
    [HideInInspector]
    public bool getDizzy;

    [HideInInspector]
    public PlayerGameStatsData_SO playerGameStatsData;

    #endregion

    #region Unity Functions
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        playerGameStatsData = ScriptableObject.CreateInstance<PlayerGameStatsData_SO>();
    }

    private void Start()
    {
        MouseManager.Instance.OnMouseClicked_Move += MoveToTarget;
        MouseManager.Instance.OnMouseClicked_Attack += AttackTarget;
        MouseManager.Instance.OnMouseClicked_Sprint += Sprint;
        cdRemain = characterStats.CloseAttackCoolDown;
        playerDead = false;
        startIsCalled = true;
    }

    private void Update()
    {
        if (!playerDead)
        {
            if (characterStats.CurrentHealth == 0)
                Dead();
            cdRemain -= Time.deltaTime;
            SwitchAnimation();
        }
    }

    private void OnEnable()
    {
        if (startIsCalled)
        {
            MouseManager.Instance.OnMouseClicked_Move += MoveToTarget;
            MouseManager.Instance.OnMouseClicked_Attack += AttackTarget;
            MouseManager.Instance.OnMouseClicked_Sprint += Sprint;
        }
    }
    
    private void OnDisable()
    {
        if (MouseManager.IsInitialized)
        {
            MouseManager.Instance.OnMouseClicked_Move -= MoveToTarget;
            MouseManager.Instance.OnMouseClicked_Attack -= AttackTarget;
            MouseManager.Instance.OnMouseClicked_Sprint -= Sprint;
        }
    }
    
    #endregion

    #region Custom Functions

    private void Sprint(Vector3 target)
    {
        if(playerDead) return;
        agent.isStopped = false;
        agent.destination = target;
        transform.LookAt(target);
        Vector3 direction = (target - transform.position).normalized;
        agent.velocity = characterStats.SprintForce * direction;
        agent.ResetPath();
    }
    
    private void MoveToTarget(Vector3 target)
    {
        if(playerDead) return;
        agent.isStopped = false;
        if((attackTarget != null) && (attackCoroutine != null))
            StopCoroutine(attackCoroutine);
        agent.destination = target;
    }

    private void SwitchAnimation()
    {
        anim.SetFloat("Speed" , agent.velocity.magnitude);
        if(characterStats.closeAttack)
            anim.SetTrigger("Attack");
        characterStats.closeAttack = false;
    }

    private void AttackTarget(GameObject target)
    {
        if(playerDead) return;
        agent.isStopped = false;
        if (target != null)
        {
            attackTarget = target;
            if (attackCoroutine != null)
                StopCoroutine(attackCoroutine);
            attackCoroutine = StartCoroutine(MoveToThenAttackTarget());
        }
        
    }

    //Now player doesn't have remoteAttack, so there is only a single function instead of having one for close and one for remote
    IEnumerator MoveToThenAttackTarget()
    {
        //get the radius of NavMeshAgent in case attackTarget doesn't have an agent like rock (Attackable)
        float radius;
        if (attackTarget.GetComponent<NavMeshAgent>())
            radius = attackTarget.GetComponent<NavMeshAgent>().radius;
        else radius = 0;
        
        while (attackTarget != null)
        {
            Vector3 pos1 = transform.position;
            Vector3 pos2 = attackTarget.transform.position;
            double distance = Vector3.SqrMagnitude(pos1 - pos2);
            bool condition1 = distance >= Math.Pow((characterStats.CloseAttackRange), 2);
            bool condition2 = distance >= Math.Pow((radius + agent.stoppingDistance + 0.1f), 2);
            if (condition1 && condition2)
            {
                agent.destination = attackTarget.transform.position;
                yield return null;
            }
            else break;
        }
        //Player's animation has no StopAgent script because the Attack could be interrupted
        agent.isStopped = true;
        if ((cdRemain < 0) && (!getDizzy))
        {
            cdRemain = characterStats.CloseAttackCoolDown; 
            Attack();
        }
    }

    private void Attack()
    {
        transform.LookAt(attackTarget.transform.position);
        characterStats.isCritical = (Random.value <= characterStats.CriticalChance);
        characterStats.closeAttack = true;
    }

    private void Dead()
    {
        playerDead = true;
        OnPlayerDead?.Invoke();
        anim.SetBool("Death" , true);
        
        if(PlayerPrefs.HasKey("Started"))
            PlayerPrefs.DeleteKey("Started");
        StartCoroutine(DeadWarning());
        StartCoroutine(SceneController.Instance.LoadScene_FadeInAndOut("StartScene", 
            UIManager.FadeInAndOutColor.Red, UIManager.FadeInAndOutColor.White, 
            4f, 1.5f , true , false));
    }

    public void KickedOff(Vector3 direction , float force)
    {
        anim.SetTrigger("Dizzy");
        agent.velocity = direction * force;
        agent.ResetPath();
    }
    
    //Animation event
    public void Hit()
    {
        if (attackTarget.CompareTag("Enemy"))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            CharacterStats.TakeDamage_Close(characterStats , targetStats);
            attackTarget.GetComponent<EnemyController>().enemyStates = EnemyStates.CHASE;
        }
        else
        {
            if (attackTarget.GetComponent<Rock_Golem>())
            {
                attackTarget.GetComponent<Rock_Golem>().rockStates = Rock_Golem.RockStates.HitEnemy;
                Vector3 direction = (attackTarget.transform.position - transform.position).normalized;
                attackTarget.GetComponent<Rock_Golem>().FlyInDirection(characterStats , direction , Force);
            }
        }
    }

    private IEnumerator DeadWarning()
    {
        GameObject playerDeadUI;
        UIManager.Instance.GetReference_PlayerDeadUI(out playerDeadUI);
        CanvasGroup canvasGroup = playerDeadUI.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.deltaTime / 0.7f;
            yield return null;
        }
        yield return new WaitForSeconds(1.1f);
        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.deltaTime / 0.7f;
            yield return null;
        }
        UIManager.Instance.BreakReference_PlayerDeadUI(ref playerDeadUI);
    }
    
    #endregion
}
    
