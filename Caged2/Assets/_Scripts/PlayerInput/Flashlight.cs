using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class Flashlight : NetworkBehaviour
{
    [SerializeField] private HDAdditionalLightData lightData;
    public Light _flashLight;
    private NetworkObjectReference networkObjectReference;
    private NetworkVariable<bool> flashlightToggled = new(false);

    void Start()
    {
        if (IsOwner && IsLocalPlayer){
            networkObjectReference = new(GetComponent<NetworkObject>());
            lightData.affectsVolumetric = false;
        }
    }

    void Update()
    {
        if(UserInput.instance.FlashlightPressed){
            ToggleFlashlightServerRpc(networkObjectReference);
        }
    }
    [ServerRpc]
    public void ToggleFlashlightServerRpc(NetworkObjectReference reference){
        reference.TryGet(out NetworkObject networkObject);
        networkObject.GetComponent<Flashlight>().flashlightToggled.Value = !flashlightToggled.Value;
        TogglePlayerFlashlightClientRpc(reference);
    }
    [ClientRpc]
    public void TogglePlayerFlashlightClientRpc(NetworkObjectReference reference){
        reference.TryGet(out NetworkObject networkObject);
        Flashlight flashlight = networkObject.GetComponent<Flashlight>();
        flashlight._flashLight.enabled = flashlight.flashlightToggled.Value;
    }
}
