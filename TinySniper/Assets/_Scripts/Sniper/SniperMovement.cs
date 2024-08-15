using Unity.Netcode;
using UnityEngine;

public class SniperMovement : NetworkBehaviour
{

    Sniper sniper;

    private Camera cam;
    private DefaultControl DefaultControl;
    public bool invertMouse = true;

    public bool canZoom = false;

    [SerializeField] float sensitivity = 1;

    
    public float minX, maxX, minY, maxY;

    void Awake(){
        sniper = GetComponent<Sniper>();
        DefaultControl = new DefaultControl();

        DefaultControl.Player.MouseScroll.performed += x => Zoom(x.ReadValue<float>());
        DefaultControl.Player.MousePan.performed += x => Pan(x.ReadValue<Vector2>());

        cam = transform.GetComponent<Camera>();
    }

    void Start()
    {
        if (!IsOwner) 
        {
            cam.enabled = false;
            GetComponent<AudioListener>().enabled = false;
            enabled = false;
            return;
        }

    }

    void Update(){

        float halfHeight = cam.orthographicSize;
        float halfWidth = cam.aspect * halfHeight;

        transform.position = new Vector3(Mathf.Clamp(transform.position.x, minX + halfWidth, maxX - halfWidth), Mathf.Clamp(transform.position.y, minY + halfHeight, maxY - halfHeight), -0.5f);
    }

    void Pan(Vector2 PanVal){

        transform.Translate(cam.orthographicSize * sensitivity * Time.deltaTime * PanVal);
    }

    void Zoom(float mouseScrollVal){

        if(!canZoom)
            return;

        cam.orthographicSize += -mouseScrollVal * Time.deltaTime;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, 2, 5);
    }

#region - Enable / Disable -

    void OnEnable(){

        DefaultControl.Enable();
    }
    void OnDisable(){

        DefaultControl.Disable();
    }

#endregion

}


