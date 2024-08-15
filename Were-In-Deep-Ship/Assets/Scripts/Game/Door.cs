using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class Door : NetworkBehaviour, IInteractable
{
    public NetworkVariable<bool> IsLocked = new(false);
    public NetworkVariable<bool> IsOpen = new(false);
    public KeyType KeyType;
    public int LockChance;
    public Transform DoorHinge;
    public Animator anim;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        int randomValue = Random.Range(0, 100);

        if (randomValue < LockChance)
        {
            IsLocked.Value = true;
            KeyType = KeyType.Door;
        }
        gameObject.SetActive(false);
        Invoke(nameof(ReEnableDoor),1f);
    }

    private void ReEnableDoor()
    {
        gameObject.SetActive(true);
    }

    public void Interact<T>(RaycastHit hit, NetworkObject Player, T type)
    {
        DoorHandlerRpc(new(Player));
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    public void DoorHandlerRpc(NetworkObjectReference playerReference){
        if (playerReference.TryGet(out NetworkObject Player)){
            if (!IsLocked.Value){
                DoorState();
            }
            else if (Player.TryGetComponent(out Inventory inventory)){
                if (inventory.InventorySlots[inventory.CurrentSlot.Value].itemInfo != null && inventory.InventorySlots[inventory.CurrentSlot.Value].itemInfo.ItemType == ItemType.Key){
                    if (inventory.InventorySlots[inventory.CurrentSlot.Value].itemInfo.KeyType == KeyType){
                        inventory.RemoveItemBySlotRpc(true, inventory.CurrentSlot.Value);
                        SetDoorLockState(false);
                    }
                } 
                else Debug.LogWarning($"Door Is Locked, Requires Key Type: {KeyType}");
            }  
        }
    }

    private void DoorState()
    {
        IsOpen.Value = !IsOpen.Value;
        if (IsOpen.Value)
        {
            anim.Play("DoorOpen");
        }
        else
        {
            anim.Play("DoorClose");
        }
    }

    private void SetDoorLockState(bool i)
    {
        IsLocked.Value = i;
    }
}
