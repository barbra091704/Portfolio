using UnityEngine;

public interface IDecisionMaker
{
    void Initialize(Transform transform, IMovement movementBehavior, ICombat combatBehavior, ISight perceptionBehavior);
    void EvaluateActions(Transform target);
}
