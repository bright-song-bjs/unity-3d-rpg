using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Golem : EnemyController
{
    [Header("CloseAttack Settings")]
    [Tooltip("The Force with which the Golem kick player away when performing CloseAttack")]
    public float KickForce;
    
    [Space]
    
    [Header("RemoteAttack Settings")]
    [Tooltip("The rock prefab that the Golem throws")]
    public GameObject RockPrefab;
    [Tooltip("Drop the right hand gameObject here")]
    public Transform handPosition;
    [Tooltip("The rock will be thrown directly at player, and this value makes the path curves upwards")]
    public float UpwardOffset;
    [Tooltip("The force with which Golem throw the rock")]
    public float Force;

    //Animation event
    public void KickOff()
    {
        if (TargetInCloseAttackRange() && transform.IsFacingTarget(attackTarget.transform , viewingThreshold) && (!getHurt))
        {
            transform.LookAt(attackTarget.transform.position);
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            CharacterStats.TakeDamage_Close(characterStats , targetStats);
            
            Vector3 direction = attackTarget.transform.position - transform.position;
            direction.Normalize();
            attackTarget.GetComponent<PlayerController>().KickedOff(direction , KickForce);
        }
    }

    //Animation event
    public void ThrowRock()
    {
        if (TargetInRemoteAttackRange() && transform.IsFacingTarget(attackTarget.transform, viewingThreshold) && (!getHurt))
        {
            var rock = Instantiate(RockPrefab, handPosition.position, Quaternion.identity);
            Rock_Golem rockController = rock.GetComponent<Rock_Golem>();
            StartCoroutine(flyToTarget(rockController));
        }
    }

    private IEnumerator flyToTarget(Rock_Golem rockController)
    {
        while (true)
        {
            if (rockController.initialized)
            {
                rockController.rockStates = Rock_Golem.RockStates.HitPlayer;
                rockController.FlyToTarget(characterStats , attackTarget , Force , 1f);
                yield break;
            }
            yield return null;
        }
    }
}
