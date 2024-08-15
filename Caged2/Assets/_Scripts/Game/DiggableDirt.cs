using Unity.Netcode;
using UnityEngine;

public class DiggableDirt : NetworkBehaviour, IInteractable
{
    [SerializeField] private NetworkObject _dirtTop;

    public void Interact(RaycastHit hit, Inventory inventory)
    {
        RemoveDirtTopServerRpc(new NetworkObjectReference(_dirtTop));
    }
    [ServerRpc] 
    public void RemoveDirtTopServerRpc(NetworkObjectReference reference){
        reference.TryGet(out NetworkObject networkObject);
        Destroy(networkObject);
        networkObject.Despawn();
    }
}
