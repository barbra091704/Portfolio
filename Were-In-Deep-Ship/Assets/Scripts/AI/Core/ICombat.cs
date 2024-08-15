using UnityEngine;
using UnityEngine.AI;

public interface ICombat
{
    void Initialize(Transform transform, NavMeshAgent agent, EntityData entityData);
    void Attack(Transform target);
}