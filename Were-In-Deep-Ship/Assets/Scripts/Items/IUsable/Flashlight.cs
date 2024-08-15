using UnityEngine;
using Unity.Netcode;

public class Flashlight : NetworkBehaviour, IUsable
{
    public bool CanUseCheck { get => CanUse.Value; set => CanUse.Value = value; } // only server calls this
    public NetworkVariable<bool> CanUse = new(true);
    public NetworkVariable<bool> IsOn = new(false, NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);
    public Light bulb;

    public void Use(NetworkObject player)
    {
        if (!CanUse.Value) return;

        ToggleServerRpc(!IsOn.Value);
    }

    public void Drop(NetworkObject player)
    {
        if (!IsOn.Value) return;

        ToggleServerRpc(false);
    }

    [Rpc(SendTo.Server)]
    public void ToggleServerRpc(bool value)
    {   
        var flashlight = GetComponent<Flashlight>();
        flashlight.IsOn.Value = value;
        GetComponent<Battery>().IsDraining.Value = value;
        ToggleClientRpc(flashlight.IsOn.Value);
    }
    
    [Rpc(SendTo.Everyone)]
    public void ToggleClientRpc(bool value)
    {
        var flashlight = GetComponent<Flashlight>();
        flashlight.bulb.enabled = value;
    }

    public void OnBatteryDead()
    {
        ToggleServerRpc(false);
    }
}
