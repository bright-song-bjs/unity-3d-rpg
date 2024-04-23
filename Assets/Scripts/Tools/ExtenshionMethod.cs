using UnityEngine;

public static class ExtenshionMethod
{
    public static bool IsFacingTarget(this Transform transform, Transform target, double dotThreshold)
    {
        Vector3 vectorToTarget = target.position - transform.position;
        vectorToTarget.Normalize();
        float dot = Vector3.Dot(transform.forward, vectorToTarget);
        return dot >= dotThreshold;
    }
}
