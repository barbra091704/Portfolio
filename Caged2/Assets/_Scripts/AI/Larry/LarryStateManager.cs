using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;

public enum State{
    Wander,
    Chase,
    Attack,
}
public class LarryStateManager : NetworkBehaviour
{
    public State CurrentAIState;
    private LarryBaseState currentState;
    public LarryWanderState WanderState = new();
    public LarryChaseState ChaseState = new();
    public LarryAttackState AttackState = new();
    [Header("Bools")]
    public bool _isActive;
    public bool _walkPointSet;

    [Header("Variables")]
    public NavMeshAgent agent;
    public Transform _Target;
    public Transform _currentPOI;
    public Animator anim;
    public Vector3 _walkPointPosition;
    public Transform[] _POIList;

    [Header("Parameters")]
    public Vector2 _chaseSpeedRange;
    public Vector2 _wanderSpeedRange;
    public Vector2 _wanderWaitRange;
    public float _wanderToPOIDelay;
    public float _hearingDistance;
    public float _hearingChaseVolume;
    public float _checkRadius;
    public float _wanderDistance;
    public float _chaseRadius;
    public float _attackRadius;
    public int _maxWalkPointAttempts;
    public int _wanderVariationSteps;
    public LayerMask layerMask;
    [SerializeField] private int _colliderCount;
    [SerializeField] private Collider[] results;
    private string CurrentAnimState;
    public const string IDLE = "IDLE";
    public const string WANDER = "WANDER";
    public const string CHASE = "CHASE";

    //public override void OnNetworkSpawn()
    void Start()
    {
        //if (!IsServer) return;
        _isActive = true;
        currentState = WanderState;
        CurrentAIState = State.Wander;
        currentState.EnterState(this);
    }
    void Update(){ 
        //if (!_isActive || !IsServer) return;
        results = new Collider[_colliderCount];
        Physics.OverlapSphereNonAlloc(transform.position, _checkRadius, results, layerMask);
        currentState.UpdateState(this, results);
        HandleAnimations(CurrentAIState);
    }
    public void SwitchState(LarryBaseState state){
        currentState = state;
        state.EnterState(this);
    }
    void OnDrawGizmosSelected(){
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, _checkRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _chaseRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _wanderDistance);
    }

    void HandleAnimations(State state){
        if (state == State.Wander){
            if (agent.velocity.magnitude > 0.5f){
                ChangeAnimationState(WANDER);
            }
            else{
                ChangeAnimationState(IDLE);
            }
        }
        else if (state == State.Chase){
            ChangeAnimationState(CHASE);
        }
        else if (state == State.Attack){
            ChangeAnimationState(IDLE);
        }
    }
    private void ChangeAnimationState(string state){
        if (CurrentAnimState == state) {return;}
        // make sure the state isnt the same
        anim.CrossFadeInFixedTime(state, 10 * Time.fixedDeltaTime);
        // play animation with a blend time
        CurrentAnimState = state;
        // set the incoming state to currentstate
    }
}

