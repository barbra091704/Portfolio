using UnityEngine;
using Unity.Netcode;
using Cinemachine;
using System.Collections;
using System;

public class PlayerMovement : NetworkBehaviour, IDamagable
{
    public NetworkVariable<int> Health = new(100);

    public float walkSpeed;
    public float runSpeed;
    public float sensitivity;
    public float currentSpeed;
    public Camera renderCamera;
    public Animator animator;  
    public Transform playerModel;
    public SpriteRenderer exclamation;
    public Stairs currentStairs; // Reference to the stairs the player is overlapping with
    public CinemachineConfiner2D playerConfiner;
    public GameObject holder;

    [SerializeField] private NetworkObject[] playerPrefabs;

    public float metersRan;

    private Vector3 originalScale;

    public void Start()
    {
        if (!IsOwner) 
        {
            holder.SetActive(false);
            GetComponent<Camera>().enabled = false;
            GetComponent<AudioListener>().enabled = false;
            renderCamera.enabled = false;
            return;
        }

        originalScale = transform.localScale;
        playerConfiner.m_BoundingShape2D = GameObject.FindGameObjectWithTag("BoundingBox").GetComponent<PolygonCollider2D>();
    }

    void Update()
    {
        if (!IsOwner) return;

        Movement(); 
        HandleStairsMovement();
        HandleScreaming();
        Leave();
    }

    private void Leave()
    {
        if (IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ClassicManager.Instance.BackToLobby();
            }
        }
    }

    private void HandleScreaming()
    {
        if (SoundManager.Instance.isScreaming.Value)
        {
            if (!exclamation.enabled)
            {
                ToggleExclamationRpc(true);
            }
        }
        else if (exclamation.enabled)
        {
            ToggleExclamationRpc(false);
        }
    }

    private void Movement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");

        if (horizontal != 0 && Health.Value > 0)
        {
            animator.SetBool("walking", true);

            currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

            transform.Translate(currentSpeed * Time.deltaTime * new Vector3(horizontal, 0, 0));

            if (currentSpeed == runSpeed)
            {
                metersRan += 3 *Time.deltaTime;
            }

            if (metersRan >= 200)
            {
                Tasks.Instance.CompleteTask("Run 200 Meters");
            }

            // Flip the direction based on the horizontal input
            if (horizontal > 0)
            {
                playerModel.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
            }
            else if (horizontal < 0)
            {
                playerModel.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
            }
        }
        else
        {
            if (animator != null)
                animator.SetBool("walking", false);
        }
    }

    private void HandleStairsMovement()
    {
        if (currentStairs == null) return;

        if (Input.GetKeyDown(KeyCode.W))
        {
            // Move upstairs
            transform.position = currentStairs.upstairsTarget.position;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            // Move downstairs
            transform.position = currentStairs.downstairsTarget.position;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Stairs"))
        {
            currentStairs = other.GetComponent<Stairs>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Stairs"))
        {
            currentStairs = null;
        }
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
        ClassicManager.Instance.SetIsAliveRpc(NetworkObject.OwnerClientId, isAlive);
    }

    [Rpc(SendTo.Everyone)]
    private void DieRpc()
    {
        if (IsOwner)animator.SetBool("dead", true);

        exclamation.enabled = false;
    }
    [Rpc(SendTo.Everyone)] 
    public void ToggleExclamationRpc(bool value)
    {
        exclamation.enabled = value;
    }
}
