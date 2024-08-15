using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class CivilianAI : NetworkBehaviour, IDamagable
{
    public NetworkVariable<int> Health = new(100);
    public NetworkVariable<bool> IsAlive = new(true);
    private Animator anim;

    [SerializeField] private float walkSpeed;
    [SerializeField] private float panicSpeed;
    [SerializeField] private float changeDirectionTimeMin = 1f;
    [SerializeField] private float changeDirectionTimeMax = 3f;
    [SerializeField] private float stopTimeMin = 1f; 
    [SerializeField] private float stopTimeMax = 2f; 
    [SerializeField] private LayerMask wallLayer;
    
    private GameObject exclamation;
    private float moveSpeed;
    private float direction;
    public bool isMoving = true; 
    private bool canPause = true;

    private void Start()
    {
        anim = GetComponent<Animator>();
        exclamation = transform.GetChild(0).gameObject;

        if (!IsServer) return;

        direction = Random.value > 0.5f ? 1f : -1f; // Randomize initial direction

        // Set the initial character facing direction based on the initial movement direction
        Vector3 localScale = transform.localScale;
        if (direction < 0 && localScale.x > 0 || direction > 0 && localScale.x < 0)
        {
            localScale.x *= -1f;
            transform.localScale = localScale;
        }

        StartCoroutine(InitialDelayAndStart());
    }

    private IEnumerator InitialDelayAndStart()
    {
        float initialDelay = Random.Range(0f, 2f); // Random initial delay
        yield return new WaitForSeconds(initialDelay);
        StartCoroutine(MoveAndPause());
    }

    private void Update()
    {
        if (!IsServer) return;

        if (IsAlive.Value)
        {
            if (isMoving)
            {
                if (SoundManager.Instance.isScreaming.Value)
                {
                    moveSpeed = panicSpeed;

                    canPause = false;

                    if (!exclamation.activeSelf)
                    {
                        ToggleExclamationRpc(true);
                    }

                    StopCoroutine(MoveAndPause());

                    if (Random.value < 0.005f)
                    {
                        FlipDirection();
                    }
                }
                else
                {
                    if (exclamation.activeSelf)
                    {
                        ToggleExclamationRpc(false);
                    }
                    canPause = true;
                    moveSpeed = walkSpeed;
                }

                anim.SetBool("walking", true);

                transform.Translate(direction * moveSpeed * Time.deltaTime * Vector2.right);
                if (IsWallAhead())
                {
                    FlipDirection();
                }
            }
            else
            {
                anim.SetBool("walking", false);
            }
        }
    }

    private IEnumerator MoveAndPause()
    {
        while (IsAlive.Value && canPause)
        {
            isMoving = true;

            float waitTime = Random.Range(changeDirectionTimeMin, changeDirectionTimeMax);
            yield return new WaitForSeconds(waitTime);

            // Occasionally stop for a random amount of time
            if (Random.value > 0.5f) // 50% chance to stop
            {
                isMoving = false;
                float stopTime = Random.Range(stopTimeMin, stopTimeMax);
                yield return new WaitForSeconds(stopTime);
                isMoving = true;
            }

            if (isMoving)
            {
                yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
            }

            // Change direction randomly
            FlipDirection();
        }
    }

    private bool IsWallAhead()
    {
        // Check if there's a wall ahead using raycasting
        Vector2 rayDirection = Vector2.right * direction;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection, 0.1f, wallLayer);
        return hit.collider != null;
    }

    private void FlipDirection()
    {
        // Flip the direction
        direction *= -1f;

        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    [Rpc(SendTo.Server)]
    public void DamageRpc(int amount)
    {
        Health.Value -= amount;

        if (Health.Value <= 0)
        {
            DieRpc();
            SetIsAliveRpc(false);
        }
    }

    [Rpc(SendTo.Server)]
    public void SetIsAliveRpc(bool isAlive)
    {
        IsAlive.Value = isAlive;
    }

    [Rpc(SendTo.Everyone)]
    private void DieRpc()
    {
        if (IsOwner) anim.SetBool("dead", true);
        exclamation.SetActive(false);
    }

    [Rpc(SendTo.Everyone)]
    public void ToggleExclamationRpc(bool value)
    {
        exclamation.SetActive(value);
    }
}
