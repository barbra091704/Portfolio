using UnityEngine;
using Unity.Netcode;
using System;


public class PlayerMovement : NetworkBehaviour
{
    private CharacterController characterController;
    private InputManager inputManager;

    [SerializeField] private Transform cameraHolder;
    [SerializeField] private new Camera camera;
    [SerializeField] private Animator animator;
    [SerializeField] private Vector2 sensitivity;

    private bool isSprinting;
    private bool isCrouching;
    private bool toggleCrouch;
    private bool toggleSprint;
    private int currentStamina = 100;
    private bool isGrounded;
    private float xRotation;
    private float yRotation;
    private string animState;
    private Vector3 playerVelocity;

    [Header("Variables")]
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float jumpHeight;


    private string IDLE = "Idle";

    private string WALKING = "Walking";
    private string WALKING_BACKWARDS = "Walking Backwards";
    private string WALKING_LEFT = "Walking Left";
    private string WALKING_RIGHT = "Walking Right";
    
    private string CROUCHING = "Crouch Idle";
    private string CROUCH_WALKING_BACKWARDS = "Crouch Walk Backwards";
    private string CROUCH_WALKING = "Crouch Walk";
    private string CROUCH_WALKING_LEFT = "Crouch Walk Left";
    private string CROUCH_WALKING_RIGHT = "Crouch Walk Right";
    
    private string SPRINTING = "Running";
    private string RUNNING_BACKWARDS = "Running Backwards";
    private string SPRINTING_LEFT = "Running Left";
    private string SPRINTING_RIGHT = "Running Right";

    private string JUMPING = "Jumping";

    void Start()
    {
        inputManager = GetComponent<InputManager>();
        characterController = GetComponent<CharacterController>();


        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        GroundCheck();
        Movement();
        CameraMovement();
        Animate();
    }

    private void GroundCheck()
    {
        isGrounded = characterController.isGrounded;
    }

    private void Movement()
    {
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector2 input = inputManager.GetRawMovement();

        Vector3 movement = GetMovementState(input.y * transform.forward + input.x * transform.right);

        movement.y = 0;

        characterController.Move(movement * Time.deltaTime);

        if (inputManager.JumpedThisFrame() && isGrounded)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -2.0f * Physics.gravity.y);
        }

        playerVelocity.y += Physics.gravity.y * Time.deltaTime;

        characterController.Move(playerVelocity * Time.deltaTime);
    }

    private Vector3 GetMovementState(Vector3 vector)
    {
        // Handle sprinting state
        if (toggleSprint && currentStamina > 0 && characterController.isGrounded && !isCrouching && inputManager.SprintIsPressed())
        {
            isSprinting = !isSprinting;
        }
        else if (!toggleSprint && !isCrouching && characterController.isGrounded && currentStamina > 0 && inputManager.SprintIsHeld())
        {
            isSprinting = true;
        }
        else if (!inputManager.SprintIsHeld() || currentStamina <= 0 || isCrouching)
        {
            isSprinting = false;
        }

        // Handle crouching state
        if (toggleCrouch && inputManager.CrouchedThisFrame())
        {
            isCrouching = !isCrouching;
        }
        else if (!toggleCrouch && !isSprinting && inputManager.CrouchIsHeld())
        {
            isCrouching = true;
        }
        else if (!inputManager.CrouchIsHeld() || isSprinting)
        {
            isCrouching = false;
        }

        // Determine speed based on state
        float currentSpeed = isCrouching ? crouchSpeed : isSprinting ? sprintSpeed : walkSpeed;
        return vector * currentSpeed;
    }


    void CameraMovement()
    {
        Vector2 lookInput = inputManager.GetMouseDelta();

        xRotation += -lookInput.y * sensitivity.x;
        yRotation += lookInput.x * sensitivity.y;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);
        cameraHolder.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.localRotation = Quaternion.Euler(0, yRotation, 0);
    }

    void ChangeAnimState(string state)
    {
        if (state == animState) return;
        animator.CrossFadeInFixedTime(state, 0.2f);
        animState = state;
    }

    void Animate()
    {
        string state = null;
        Vector3 moveInput = inputManager.GetRawMovement();

        if (isGrounded)
        {
            if (moveInput.y > 0) // Moving forward
            {
                if (isSprinting)
                {
                    state = moveInput.x < 0 ? SPRINTING_LEFT : moveInput.x > 0 ? SPRINTING_RIGHT : SPRINTING;
                }
                else if (isCrouching)
                {
                    state = moveInput.x < 0 ? CROUCH_WALKING_LEFT : moveInput.x > 0 ? CROUCH_WALKING_RIGHT : CROUCH_WALKING;
                }
                else
                {
                    state = moveInput.x < 0 ? WALKING_LEFT : moveInput.x > 0 ? WALKING_RIGHT : WALKING;
                }
            }
            else if (moveInput.y < 0) // Moving backward
            {
                if (isSprinting)
                {
                    state = RUNNING_BACKWARDS;
                }
                else if (isCrouching)
                {
                    state = CROUCH_WALKING_BACKWARDS;
                }
                else
                {
                    state = WALKING_BACKWARDS;
                }
            }
            else if (moveInput.x != 0) // Moving sideways (left or right)
            {
                if (isSprinting)
                {
                    state = moveInput.x < 0 ? SPRINTING_LEFT : SPRINTING_RIGHT;
                }
                else if (isCrouching)
                {
                    state = moveInput.x < 0 ? CROUCH_WALKING_LEFT : CROUCH_WALKING_RIGHT;
                }
                else
                {
                    state = moveInput.x < 0 ? WALKING_LEFT : WALKING_RIGHT;
                }
            }
            else // No movement input
            {
                state = isCrouching ? CROUCHING : IDLE;
            }
        }
        else
        {
            state = JUMPING;
        }

        if (state != null)
        {
            ChangeAnimState(state);
        }
    }


}
