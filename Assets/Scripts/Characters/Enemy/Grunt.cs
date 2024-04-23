using UnityEngine;
using UnityEngine.AI;

public class Grunt : EnemyController
{
    [Header("CloseAttack Settings")]
    [Tooltip("The Force with which the Grunt kick player away when performing CloseAttack")]
    public float KickForce;

    //Animation event
    public void KickOff()
    {
        if (TargetInCloseAttackRange() && transform.IsFacingTarget(attackTarget.transform , viewingThreshold) && (!getHurt))
        {
            transform.LookAt(attackTarget.transform.position);
            Vector3 direction = attackTarget.transform.position - transform.position;
            direction.Normalize();
            attackTarget.GetComponent<PlayerController>().KickedOff(direction , KickForce);
        }
    }
}
