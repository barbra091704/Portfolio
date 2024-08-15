using UnityEngine;
using UnityEngine.AI;

public interface IMovement
{
    void Initialize(Transform transform, NavMeshAgent agent, EntityData entityData);
    void Patrol();
    void Chase(Transform target);
    void Flee(Transform target);
}