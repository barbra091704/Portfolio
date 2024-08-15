using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class ShadowmanCombatBehaviour : ICombat
{
    private Transform transform;
    private Transform target;
    private EntityData entityData;
    private Shadowman main;
    private float lastAttackTime;

    public void Initialize(Transform transform, NavMeshAgent agent, EntityData entityData)
    {
        this.entityData = entityData;
        this.transform = transform;
        main = transform.GetComponent<Shadowman>();
    }

    public void Attack(Transform target)
    {
        this.target = target;

        if (CanAttack())
        {
            // Deal damage to the target
            target.GetComponent<PlayerHealth>().TakeDamageRpc(entityData.AttackDamage, true);
            main.attackAmount++;
            lastAttackTime = Time.time; // Update the last attack time
        }
    }

    private bool CanAttack()
    {
        // Check if enough time has passed since the last attack
        return Time.time - lastAttackTime >= entityData.DamageInterval &&
               Vector3.Distance(transform.position, target.position) <= entityData.AttackStopDistance;
    }
}
