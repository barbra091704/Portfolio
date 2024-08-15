using Unity.Netcode;
using UnityEngine;
using Cinemachine;
using KWS;
using Unity.Netcode.Components;
using System;
using System.Collections;

public class PlayerMovement : NetworkBehaviour
{
    public bool SprintHeld {get; private set;}
    public bool FastSwimHeld {get; private set;}
    public bool CrouchPressed {get; private set;}
    public bool CrouchIsHeld {get; private set;}
    public bool SinkHeld {get; private set;}
    public bool FloatHeld {get; private set;}
    public bool JumpPressed {get; private set;}


    [Header("Generic Variables")]
    public NetworkVariable<bool> InWater = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> CurrentStamina = new(100,NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public Transform playerXZRotationObject;
    public Transform playerYRotationObject;
    public Rigidbody rigidBody;
    private PlayerCamera cameraScript;
    [SerializeField] Animator animator;
    [SerializeField] CinemachineVirtualCamera virtualCamera;
    [SerializeField] Camera playerCam;
    [SerializeField] Canvas UI;
    [SerializeField] SkinnedMeshRenderer meshRenderer;
    [SerializeField] Transform footTransform;
    [SerializeField] LayerMask groundMask;
    private InputManager inputManager;
    public bool CanMove = true;
    public bool IsGrounded;
    public bool IsOnSlope;
    public bool IsClimbingLadder;
    [SerializeField] bool isCrouched;
    [SerializeField] float standingHeight;
    [SerializeField] float crouchingHeight;
    [SerializeField] float staminaRegenTimer;
    [SerializeField] float staminaRegenMultiplier;
    [SerializeField] float staminaDegenMultiplier; 
    [SerializeField] float staminaTimeToRegen;
    [SerializeField] float moveSpeed;
    string CurrentAnimationState;
    float movementMultiplier = 10f;
    
    [Header("Ground Parameters")]
    [SerializeField] float walkSpeed;
    [SerializeField] float sprintSpeed;
    [SerializeField] float crouchSpeed;
    [SerializeField] float jumpForce;
    [SerializeField] float jumpDelay;
    [SerializeField] float crouchOffset = -0.5f;
    [SerializeField] float acceleration = 10f;
    [SerializeField] float groundDistance = 0.4f;
    [SerializeField] float airMultiplier = 0.4f;

    [Header("Drag")]
    [SerializeField] float groundDrag = 6f;
    [SerializeField] float airDrag = 2f;
    [SerializeField] float waterDrag = 4f;
    [SerializeField] float climbingDrag = 6;

    [Header("Water Parameters")]
    [SerializeField] float swimSpeed;
    [SerializeField] float sprintSwimSpeed;
    [SerializeField] float sinkSpeed;
    [SerializeField] float floatSpeed;
    [SerializeField] float verticalYWaterOffset;

    [Header("Climbing Parameters")]
    [SerializeField] float climbSpeed;

    Vector2 moveInput;
    Vector3 moveDirection;
    Vector3 slopeMoveDirection;
    RaycastHit slopeHit;
    public float buoyancy;
    bool WasInWater;
    private int weight;
    private Ladder activeLadder;
    private bool exitLadder; 

    
    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, 2 / 2 + 0.5f))
        {
            if (slopeHit.normal != Vector3.up)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }
    public void Start()
    {
        if (!IsOwner) return;

        inputManager = InputManager.Instance;
        cameraScript = GetComponent<PlayerCamera>();
        GetComponent<Inventory>().CurrentWeight.OnValueChanged += SetWeight;
    }


    private void SetWeight(int previousValue, int newValue)
    {
        weight = newValue;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            UI.enabled = false;
            playerCam.enabled = false;
            playerCam.GetComponent<AudioListener>().enabled = false;
        }
        else
        {   
            virtualCamera.Priority = 1;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            InWater.OnValueChanged += HandlePlayerRotation;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    void Update()
    {
        if (!IsOwner || !CanMove) return;

        if (rigidBody.velocity.magnitude < 1f && moveInput.x == 0 && moveInput.y == 0 && IsGrounded && OnSlope())
        {
            rigidBody.velocity = Vector3.zero;
            rigidBody.useGravity = false;
        } 
        else
        {
            rigidBody.useGravity = true;
        }
        
        InWater.Value = WaterSystem.IsPositionUnderWater(new(transform.position.x, transform.position.y + verticalYWaterOffset, transform.position.z));
        IsGrounded = Physics.CheckSphere(footTransform.position, groundDistance, groundMask, QueryTriggerInteraction.Ignore) && !InWater.Value;
        moveInput = inputManager.GetPlayerMovement();

        rigidBody.useGravity = !InWater.Value;

        UpdateInput();
        MyInput();
        ControlSpeed();
        ControlDrag();
        HandleAnimation();
        
        slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
    }

    private void HandlePlayerRotation(bool old, bool value)
    {
        if (value)
        {
            playerXZRotationObject.rotation = Quaternion.LookRotation(playerCam.transform.forward, Vector3.up);
        }
        else if (!value)
        {
            StopCoroutine(nameof(LerpRotation));
            StartCoroutine(LerpRotation(playerXZRotationObject, Quaternion.identity, 0.5f));
        }
    }
    private IEnumerator LerpRotation(Transform transform, Quaternion targetRotation, float duration)
    {
        float timeElapsed = 0f;
        Quaternion initialRotation = transform.localRotation;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(timeElapsed / duration);
            transform.localRotation = Quaternion.Lerp(initialRotation, targetRotation, t);
            yield return null;
        }
        transform.localRotation = targetRotation;
    }

    private void MyInput()
    {
        if (IsGrounded)
        {
            WasInWater = false;
        }
        if (InWater.Value && !IsGrounded)
        {
            moveDirection = moveInput.y * cameraScript.camHolder.forward + moveInput.x * cameraScript.camHolder.right;

            WasInWater = true;

            float weightFactor = 1 - (weight / 40.0f);

            buoyancy = FloatHeld ? floatSpeed + weightFactor : SinkHeld ? sinkSpeed + weightFactor : weightFactor;
        }
        
        else if (!InWater.Value)
        { 
            moveDirection = moveInput.y * playerYRotationObject.forward + moveInput.x * playerYRotationObject.right;

            if (JumpPressed)
            {
                Jump();
            }
        }
    }
    private void Jump()
    {
        rigidBody.velocity = new(rigidBody.velocity.x, 0, rigidBody.velocity.z);
        rigidBody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    void ControlDrag()
    {
        if (IsClimbingLadder)
        {
            rigidBody.drag = climbingDrag;
        }
        else if (IsGrounded)
        {
            rigidBody.drag = groundDrag;
        }
        else if (InWater.Value || WasInWater)
        {
            rigidBody.drag = waterDrag;
        }
        else
        {
            rigidBody.drag = airDrag;
        }
    }

    void ControlSpeed()
    {
        // Calculate weight factor, will be 1 when Weight is 0, and goes down to 0.2 when Weight is 250 or more.
        float weightFactor = Math.Max(1 - (Math.Min(weight, 250) * 0.8f / 250.0f), 0.2f);

        if (SprintHeld)
        {
            if (!InWater.Value)
                moveSpeed = Mathf.Lerp(moveSpeed, sprintSpeed * weightFactor, acceleration * Time.deltaTime);
            else
                moveSpeed = Mathf.Lerp(moveSpeed, sprintSwimSpeed * weightFactor, acceleration * Time.deltaTime);
        }
        else
        {
            if (!InWater.Value)
                moveSpeed = Mathf.Lerp(moveSpeed, walkSpeed * weightFactor, acceleration * Time.deltaTime);
            else
                moveSpeed = Mathf.Lerp(moveSpeed, swimSpeed * weightFactor, acceleration * Time.deltaTime);
        }
    }

    public void OnLadder(Vector3 startPosition, Ladder ladder)
    {
        IsClimbingLadder = true;
        rigidBody.isKinematic = true;
        activeLadder = ladder;
        rigidBody.position = startPosition;
    }

    public void ExitLadder(bool teleport)
    {
        if (activeLadder != null)
        {
            IsClimbingLadder = false;
            rigidBody.isKinematic = false;

            if (teleport) 
            {
                rigidBody.position = activeLadder.GetEndPos();
            }
            activeLadder = null;
        }
    }
    private void FixedUpdate()
    {
        if (!IsOwner || !CanMove) return; 

        if (IsClimbingLadder)
        {
            MoveOnLadder();
        }
        else
        {
            MovePlayer();
        }

    }

    void MoveOnLadder()
    {
        if (moveInput.y != 0)
        {
            float climbingSpeed = moveInput.y * climbSpeed * Time.deltaTime;
            rigidBody.MovePosition(transform.position + Vector3.up * climbingSpeed);
        }
    }

    void MovePlayer()
    {
        if (IsGrounded && !OnSlope())
        {
            rigidBody.AddForce(movementMultiplier * moveSpeed * moveDirection.normalized, ForceMode.Acceleration);
        }
        else if (IsGrounded && OnSlope())
        {
            rigidBody.AddForce(movementMultiplier * moveSpeed * slopeMoveDirection.normalized, ForceMode.Acceleration);
        }
        else if (!IsGrounded && !InWater.Value)
        {
            rigidBody.AddForce(airMultiplier * movementMultiplier * moveSpeed * moveDirection.normalized, ForceMode.Acceleration);
        }
        else if (!IsGrounded && InWater.Value)
        {
            rigidBody.AddForce(movementMultiplier * moveSpeed * moveDirection.normalized, ForceMode.Acceleration);
            rigidBody.AddForce(Vector3.up * buoyancy, ForceMode.Acceleration);
        }
    }

    private void UpdateInput()
    {
        SprintHeld = inputManager.SprintIsHeld() && !isCrouched && CurrentStamina.Value > 0 && moveInput.y > 0;

        FastSwimHeld = inputManager.SprintIsHeld() && !isCrouched && CurrentStamina.Value > 0 && moveInput.y > 0;

        CrouchPressed = inputManager.CrouchedThisFrame() && !InWater.Value && IsGrounded;

        CrouchIsHeld = inputManager.CrouchIsHeld() && !InWater.Value && IsGrounded;

        SinkHeld = inputManager.CrouchIsHeld() && InWater.Value;

        FloatHeld = inputManager.JumpIsHeld() && InWater.Value;
        
        JumpPressed = inputManager.JumpedThisFrame() && IsGrounded && !isCrouched;
    }

    private void HandleAnimation()
    {
        string state = null;
        if (moveInput.y > 0)
        {
            if (SprintHeld)
            {
                if (!InWater.Value)
                {
                    state = "Sprinting";
                }
                else if (InWater.Value)
                {
                    state = "Fast Swimming";
                }
            }
            else
            {
                if (isCrouched)
                {
                    state = "Crouch Walk";
                }
                else if (!InWater.Value && IsGrounded)
                {
                    state = "Walking";
                }
                else if (InWater.Value)
                {
                    state = "Swimming";
                }
            }
        }
        else if (moveInput.y < 0)
        {
            if (isCrouched)
            {
                state = "Crouch Walk Backwards";
            }
            else if (!InWater.Value && IsGrounded)
            {
                state = "Walking Backwards";
            }
            else if (InWater.Value)
            {
                state = "Swimming Backwards";
            }
        }
        else
        {
            if (isCrouched)
            {
                state = "Crouch";
            }
            else if (!InWater.Value && IsGrounded)
            {
                state = "Idle";
            }
            else if (InWater.Value)
            {
                state = "Floating";
            }
        }
        if (state != null)
        {
            ChangeAnimationState(state);
        }
    }

    public void ChangeAnimationState(string state)
    {
        if (CurrentAnimationState != state)
        {
            animator.CrossFadeInFixedTime(state, 0.35f);
            CurrentAnimationState = state;
        }
    }

}
