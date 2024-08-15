using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;
using Steamworks.Ugc;

[RequireComponent(typeof(NavMeshAgent))]
public class Shadowman : NetworkBehaviour
{
    public NetworkVariable<int> EntityHealth = new();
    public EntityData entityData;
    public Transform target;
    public LayerMask detectionMask;

    public int attackAmount;
    [HideInInspector] public NavMeshAgent agent;
    public new MeshRenderer renderer;
    private IMovement movementBehavior;
    private ICombat combatBehavior;
    private ISight sightBehavior;
    private IDecisionMaker decisionMakingBehavior;
    private Collider[] colliders;

    public void Start()
    {
        if (!IsServer) return;

        agent = GetComponent<NavMeshAgent>();

        EntityHealth.Value = entityData.MaxHealth;

        movementBehavior = new ShadowmanMovementBehaviour();
        combatBehavior = new ShadowmanCombatBehaviour();
        sightBehavior = new ShadowmanSightBehaviour();
        decisionMakingBehavior = new ShadowmanDecisionMakerBehaviour();

        movementBehavior.Initialize(transform, agent, entityData);
        combatBehavior.Initialize(transform, agent, entityData);
        sightBehavior.Initialize(transform, entityData);
        decisionMakingBehavior.Initialize(transform, movementBehavior, combatBehavior, sightBehavior);
        print("Initialized");
    }

    private void Update()
    {
        if (!IsServer) return;

        colliders = new Collider[30];
        Physics.OverlapSphereNonAlloc(transform.position, entityData.DetectionRange, colliders, detectionMask);

        foreach (var item in colliders)
        {
            if (item != null && item.CompareTag("Player"))
            {
                target = item.transform;
            }
        }

        decisionMakingBehavior.EvaluateActions(target);
        
    }

    [Rpc(SendTo.Server)]
    public void TakeDamageRpc(int value, bool applyResistance)
    {
        float resistanceMultiplier = 1;

        if (applyResistance) resistanceMultiplier = 1 - (entityData.DamageResistance / 100f); 

        int damageTaken = Mathf.RoundToInt(value * resistanceMultiplier);

        EntityHealth.Value -= damageTaken;
    }
}