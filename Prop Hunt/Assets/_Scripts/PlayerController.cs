using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class PlayerController : MonoBehaviourPun
{
    private CharacterController _controller;

    [SerializeField]
    private float _playerSpeed = 3f;
    public float sprintSpeed = 5.5f;
    [SerializeField]
    private float _rotationSpeed = 10f;
    public float Stamina = 100.0f;
    public float MaxStamina = 100.0f;
    //---------------------------------------------------------
    private float StaminaRegenTimer = 0.0f;
    //---------------------------------------------------------
    private const float StaminaTimeToRegen = 0.5f;
    public Slider staminaslider;

    private Camera _followCamera;
    public Animator anim;
    private Vector3 _playerVelocity;

    [SerializeField]
    private float _jumpHeight = 1.0f;
    [SerializeField]
    private float _gravityValue = -9.81f;
    public bool isjumping;
    float verticalInput;
    float horizontalInput;
    public static bool cursorlock;
    bool isRunning;

    private void Start() 
    {
        Stamina = MaxStamina;
        _playerSpeed = 2.5f;
        _controller = GetComponent<CharacterController>(); 
        _followCamera = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {

        if (!photonView.IsMine)
            return;
            if (Input.GetKeyDown(KeyCode.F11))
            {
                ConnectToServer.fs = !ConnectToServer.fs;
                Screen.fullScreen = ConnectToServer.fs;
            }
            if (EnableUI.isHunter)
            {
                float yRotation = Camera.main.transform.rotation.eulerAngles.y;
                Quaternion newRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, yRotation, transform.rotation.eulerAngles.z);
                transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, _rotationSpeed * Time.deltaTime);
            }
            Movement();
            Animation();
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (cursorlock)
                {
                    cursorlock = false;
                    Cursor.lockState = CursorLockMode.Confined;
                    Cursor.visible = true;
                }
                else
                {
                    cursorlock = true;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            _playerSpeed = _playerSpeed / 3;
            sprintSpeed = sprintSpeed / 2;
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            _playerSpeed = _playerSpeed * 3;
            sprintSpeed = sprintSpeed * 2;
        }
        isRunning = Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.D);
        if (isRunning)
        {
            if (EnableUI.isHunter == true)
            {
                Stamina = Mathf.Clamp(Stamina - (7 * Time.deltaTime), 0.0f, MaxStamina);
                staminaslider.value = Stamina;
                StaminaRegenTimer = 0.0f;
            }
            else
            {
                Stamina = Mathf.Clamp(Stamina - (12 * Time.deltaTime), 0.0f, MaxStamina);
                staminaslider.value = Stamina;
                StaminaRegenTimer = 0.0f;
            }
        }
        else if (Stamina < MaxStamina)
        {
            if (StaminaRegenTimer >= StaminaTimeToRegen)
            {
                Stamina = Mathf.Clamp(Stamina + (25 * Time.deltaTime), 0.0f, MaxStamina);
                staminaslider.value = Stamina;
            }
            else
            {
                StaminaRegenTimer += Time.deltaTime;
            }
        }
    }


    void Movement() 
    {
        if (_controller.isGrounded && _playerVelocity.y <= 0) 
        {
            _playerVelocity.y = 0f;
        }
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");

        Vector3 movementInput = Quaternion.Euler(0, _followCamera.transform.eulerAngles.y, 0) * new Vector3(horizontalInput, 0, verticalInput);
        Vector3 movementDirection = movementInput.normalized;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A))
        {
            _controller.Move(movementDirection * _playerSpeed * Time.deltaTime);
            anim.SetBool("Walking", true);
        }
        if (isRunning && (Stamina > 0))
        {
            _controller.Move(movementDirection * sprintSpeed * Time.deltaTime);
            anim.SetBool("Walking", false);
            anim.SetBool("Running", true);
        }
        if (movementDirection != Vector3.zero) 
        {
            if (!EnableUI.isHunter)
            {
                Quaternion desiredRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, _rotationSpeed * Time.deltaTime);
            }
        }

        _playerVelocity.y += _gravityValue * Time.deltaTime;
        _controller.Move(_playerVelocity * Time.deltaTime);

        if (_controller.isGrounded)
        {
            isjumping = false;
            if (Input.GetButtonDown("Jump"))
            {
                _playerVelocity.y += Mathf.Sqrt(_jumpHeight * -3.0f * _gravityValue);
                anim.SetBool("Jumping", true);
                isjumping = true;
            }
            else
            {
                anim.SetBool("Jumping", false);
            }
        }

    }
    void Animation()
    {
        if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.S))
        {
            anim.SetBool("Running", false);
        }
        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.A))
        {
            anim.SetBool("Walking", false);
            anim.SetBool("Running", false);
        }
    }

}