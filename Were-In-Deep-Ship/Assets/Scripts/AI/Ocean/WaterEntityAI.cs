using Unity.Netcode;
using UnityEngine;


[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class WaterEntityAI : NetworkBehaviour, IDamagable
{
    public NetworkVariable<int> EntityHealth = new(100);
    public WaterEntityWanderState WanderState = new();
    public WaterEntityAttackState AttackState = new();
    public WaterEntityFleeState  FleeState = new();
    public WaterEntityBaseState CurrentState;
    public EntityData entityData;
    public Animator animator;
    public Transform entityFront;
    public string SwimAnimationName;
    public string FleeAnimationName;
    public string AttackAnimationName;
    public string ChaseAnimationName;
    private new Rigidbody rigidbody;
    private new BoxCollider collider;
    private Transform target;
    public string CurrentAnimationState;
    [HideInInspector] public EntityManager entityManager;

    public BoxCollider Collider { get { return collider; } set { collider = value; } }
    public Rigidbody Rigidbody { get { return rigidbody; } set { rigidbody = value; } }
    public Transform Target { get { return target;} set { target = value; } }

    public void Awake()
    {
        entityManager = FindAnyObjectByType<EntityManager>(FindObjectsInactive.Exclude);
    }

    public void Start()
    {
        if (!IsServer) return;

        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<BoxCollider>(); 

        EntityHealth.Value = entityData.MaxHealth;

        float size = Random.Range(entityData.SizeMinMax.x, entityData.SizeMinMax.y);
        transform.localScale = new(size,size,size);

        CurrentState = WanderState;
        CurrentState.EnterState(this);
        ChangeAnimationState(SwimAnimationName);
    }

    public void FixedUpdate()
    {
        if (!IsServer) return;

        CurrentState.FixedUpdateState(this);

        Rigidbody.position = new(Rigidbody.position.x, Mathf.Clamp(Rigidbody.position.y, entityData.DepthRange.x, entityData.DepthRange.y), Rigidbody.position.z);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player"))
        {
            if (entityData.CanAttack)
            {
                Target = other.transform;
                ChangeState(AttackState);
                return;
            }
            else
            {
                Target = other.transform;
                ChangeState(FleeState);
                return;
            }
        }
        else if (entityData.EntityType == EntityType.Friendly && other.CompareTag("Entity"))
        {
            if (other.transform.root.TryGetComponent(out WaterEntityAI component) && component.entityData.EntityType == EntityType.Predator)
            {
                if (component != this)
                {
                    Target = other.transform;
                    ChangeState(FleeState);
                    return;
                }
            }
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;
        
        if (other.CompareTag("Player") && CurrentState == AttackState)
        {
            CurrentState = WanderState;
            CurrentState.EnterState(this);
        }
    }
    public void ChangeState(WaterEntityBaseState state)
    {
        if (state == CurrentState) return;

        CurrentState = state;
        CurrentState.EnterState(this);
    }

    public void ChangeAnimationState(string state)
    {
        if (state == CurrentAnimationState) return;

        CurrentAnimationState = state;
        animator.CrossFadeInFixedTime(state, 0.5f);
    }

    public void TakeDamage(int value, bool applyResistance)
    {
        TakeDamageRpc(value, applyResistance);
    }

    [Rpc(SendTo.Server)]
    public void TakeDamageRpc(int value, bool applyResistance)
    {
        float resistanceMultiplier = 1;

        if (applyResistance) resistanceMultiplier = 1 - (entityData.DamageResistance / 100f); // Calculate resistance multiplier (e.g., 10 resistance = 0.9 multiplier)

        int damageTaken = Mathf.RoundToInt(value * resistanceMultiplier);

        EntityHealth.Value -= damageTaken;
    }
}
