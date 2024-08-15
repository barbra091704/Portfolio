using UnityEngine;

public interface ISight
{
    void Initialize(Transform transform, EntityData entityData);
    bool CanSeeTarget(Transform target);
    bool CanTargetSeeMe(Renderer renderer);
    bool IsTargetInAttackRange(Transform target);
    bool IsTargetInFleeRange(Transform target);
}
