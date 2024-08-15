using Unity.Netcode;
using UnityEngine;

public class PlayerCamera : NetworkBehaviour
{
    public float mouseSensX;
    public float mouseSensY;
    public float gamepadSensX;
    public float gamepadSensY;
    public Transform camHolder;
    public Transform cameraRotater;
    public Vector2 YRotationLimit;
    private InputManager inputManager;
    private float xRotation;
    private float yRotation;
    public bool canLook = true;

    // Start is called before the first frame update
    void Start()
    {
        if (!IsOwner) enabled = false;
        canLook = true;
        inputManager = InputManager.Instance;  
    }
    void Update(){
        if (!canLook) return;
        CameraMovement();
    }
    void CameraMovement(){
        Vector2 lookInput = inputManager.GetMouseDelta();

        xRotation += -lookInput.y * mouseSensY;
        yRotation += lookInput.x * mouseSensX;
        xRotation = Mathf.Clamp(xRotation, YRotationLimit.x, YRotationLimit.y);
        camHolder.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        cameraRotater.localRotation = Quaternion.Euler(0, yRotation, 0);
    }

    void tCameraMovement()
    {
        Vector2 lookInput = inputManager.GetMouseDelta(); 

        float smoothX = -lookInput.y * mouseSensY * Time.deltaTime;
        float smoothY = lookInput.x * mouseSensX * Time.deltaTime;

        xRotation += smoothX;
        yRotation += smoothY;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        camHolder.localRotation = Quaternion.Euler(0, yRotation, 0);
        cameraRotater.localRotation = Quaternion.Euler(xRotation, 0, 0);
    }
}
