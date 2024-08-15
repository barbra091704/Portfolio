using Unity.Netcode;
using UnityEngine;

public class InputManager : NetworkBehaviour
{
    private static InputManager _instance;
    public static InputManager Instance {
        get{
            return _instance;
        }
    }
    private PlayerControls playerControls;
    [SerializeField] private bool Bypass;

    public void OnEnable()
    {
        if (!Bypass)
        {
            if (!IsOwner) 
            {
                _instance = null;
                enabled = false;
                return;
            }
        }
        _instance = this;
        playerControls = new();
        playerControls.Enable();
    }
    public void OnDisable(){
        if (!IsOwner) return;
        playerControls.Disable();
    }    
    public Vector2 GetRawMovement(){
        return playerControls.Actions.Move.ReadValue<Vector2>();
    }
    public Vector2 GetMouseDelta(){
        return playerControls.Actions.Look.ReadValue<Vector2>();
    }
    public Vector2 GetScrollWheel(){
        return playerControls.Actions.Scrollwheel.ReadValue<Vector2>();
    }
    public bool JumpedThisFrame(){
        return playerControls.Actions.Jump.triggered;
    }
    public bool JumpIsHeld(){
       return playerControls.Actions.Jump.IsPressed(); 
    }
    public bool SprintIsHeld(){
        return playerControls.Actions.Sprint.IsPressed();
    }
    public bool SprintIsPressed(){
        return playerControls.Actions.Sprint.triggered;
    }
    public bool CrouchedThisFrame(){
        return playerControls.Actions.Crouch.triggered;
    }
    public bool CrouchIsHeld(){
       return playerControls.Actions.Crouch.IsPressed(); 
    }
    public bool InteractedThisFrame(){
        return playerControls.Actions.Interact.triggered;
    }
    public bool InteractIsHeld(){
        return playerControls.Actions.Interact.IsPressed();
    }
    public bool InteractReleased(){
        return playerControls.Actions.Interact.WasReleasedThisFrame();
    }
    public bool DroppedThisFrame(){
        return playerControls.Actions.Drop.triggered;
    }
    public bool ExitedThisFrame(){
        return playerControls.Actions.Back.triggered;
    }
    public bool TabbedThisFrame(){
        return playerControls.Actions.Tab.triggered;
    }
    public bool Slot1ThisFrame(){
        return playerControls.Actions._1.triggered;
    }
    public bool Slot2ThisFrame(){
        return playerControls.Actions._2.triggered;
    }
    public bool Slot3ThisFrame(){
        return playerControls.Actions._3.triggered;
    }
    public bool Slot4ThisFrame(){
        return playerControls.Actions._4.triggered;
    }
    public bool Slot5ThisFrame(){
        return playerControls.Actions._5.triggered;
    }
    public bool EnterThisFrame(){
        return playerControls.Actions.Enter.triggered;
    }
    public bool RightBumperThisFrame(){
        return playerControls.Actions.RightBumper.triggered;
    }
    public bool LeftBumperThisFrame(){
        return playerControls.Actions.LeftBumper.triggered;
    }
    public bool RightClickPressed(){
        return playerControls.Actions.Aim.triggered;
    }
    public bool LeftClickPressed(){
        return playerControls.Actions.Shoot.triggered;
    }
    public bool PingPressed(){
        return playerControls.Actions.Ping.triggered;
    }
    public bool EscapePressed(){
        return playerControls.Actions.Back.triggered;
    }
    public bool RKeyPressed(){
        return playerControls.Actions.R.triggered;
    }



}
