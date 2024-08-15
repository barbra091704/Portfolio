using UnityEngine;

public class ShadowmanSightBehaviour : ISight
{
    private Transform transform;
    private EntityData entityData;

    public void Initialize(Transform transform, EntityData entityData)
    {
        this.entityData = entityData;
        this.transform = transform;
    }

    public bool CanSeeTarget(Transform target)
    {
        return Vector3.Distance(transform.position, target.position) <= entityData.DetectionRange;
    }
    public bool CanTargetSeeMe(Renderer renderer)
    {
        return renderer.isVisible;
    }

    public bool IsTargetInAttackRange(Transform target)
    {
        return Vector3.Distance(transform.position, target.position) <= entityData.AttackStopDistance;
    }

    public bool IsTargetInFleeRange(Transform target)
    {
        return Vector3.Distance(transform.position, target.position) <= entityData.FleeDistance + 2;
    }
}