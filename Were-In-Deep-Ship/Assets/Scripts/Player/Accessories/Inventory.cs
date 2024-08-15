using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Animations;

[Serializable]
public class InventorySlot
{
    public ItemInfo itemInfo;
    public NetworkObject itemNetworkObject;
}

public class Inventory : NetworkBehaviour
{
    private InputManager inputManager;
    private NetworkObjectReference networkObjectReference;

    public NetworkVariable<sbyte> CurrentSlot = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> CurrentWeight = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    public SkinnedMeshRenderer skinnedMeshRenderer;
    public DivingSuit currentSuit;
    public Transform handPos;
    public Transform dropPos;

    private Mesh originalMesh;
    private Material originalMaterial;

    public event Action<int> UISelectSlotEvent;
    public event Action<int,bool> UISetSlotImageEvent;
    public event Action<Sprite> UISetArmorEvent;
    public event Action<InventorySlot> BatteryUpdateCheck;

    [Header("Inventory Slots")]

    public InventorySlot[] InventorySlots;

    private void Start()
    {
        networkObjectReference = new(NetworkObject);

        if (!IsOwner) return;

        inputManager = InputManager.Instance;

        GetComponent<Interaction>().ItemPickupEvent += PickupCheck;

        originalMaterial = GetComponentInChildren<SkinnedMeshRenderer>().material;
        originalMesh = GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh;
    }
    private void Update()
    {
        if (!IsOwner) return;

        Handleinput();
    }

    private void Handleinput()
    {
        sbyte slotIndex = (sbyte)(
            inputManager.Slot1ThisFrame() ? 0 : 
            inputManager.Slot2ThisFrame() ? 1 : 
            inputManager.Slot3ThisFrame() ? 2 : 
            inputManager.Slot4ThisFrame() ? 3 : 
            inputManager.Slot5ThisFrame() ? 4 : 
        -1);

        if (slotIndex != -1) SelectSlotRpc(slotIndex);

        if (inputManager.DroppedThisFrame() && InventorySlots[CurrentSlot.Value].itemNetworkObject != null) 
        {
            if (InventorySlots[CurrentSlot.Value].itemInfo == null) return;

            switch(InventorySlots[CurrentSlot.Value].itemInfo.ItemType)
            {
                case ItemType.Usable:  
                    IUsable usable = InventorySlots[CurrentSlot.Value].itemInfo.GetComponent<IUsable>();
                    usable?.Drop(networkObjectReference);
                    break;
            }
            DropItemServerRpc();
            BatteryUpdateCheck?.Invoke(InventorySlots[CurrentSlot.Value]);
        }
        if (inputManager.RightBumperThisFrame())
        {
            BumperNavigation(true);
        }
        if (inputManager.LeftBumperThisFrame())
        {
            BumperNavigation(false);
        }
        if (inputManager.ShootPressed())
        {
            if (InventorySlots[CurrentSlot.Value].itemInfo == null) return;

            switch(InventorySlots[CurrentSlot.Value].itemInfo.ItemType)
            {
                case ItemType.Equippable:
                    DivingSuit suit = InventorySlots[CurrentSlot.Value].itemInfo.equippable as DivingSuit;
                    EquipSuitRpc(suit.Level);
                    break;
                case ItemType.Usable:  
                    IUsable usable = InventorySlots[CurrentSlot.Value].itemInfo.GetComponent<IUsable>();
                    usable?.Use(networkObjectReference);
                    break;
            }
        }
        ScrollWheelNavigation(inputManager.ScrollWheelMoved().y);
    }

    private void ScrollWheelNavigation(float value)
    {
        if (value == 0) return;
        
        int slotOffset = value > 0 ? -1 : 1;
        sbyte newSlot = (sbyte)((CurrentSlot.Value + slotOffset + 5) % 5); // Ensures cyclic behavior

        SelectSlotRpc(newSlot);
    }

    private void BumperNavigation(bool value)
    {
        int newSlot = value ? (CurrentSlot.Value == 0 ? 4 : CurrentSlot.Value - 1): 
                              (CurrentSlot.Value == 4 ? 0 : CurrentSlot.Value + 1);
        SelectSlotRpc((sbyte)newSlot);
    }


    private void PickupCheck(RaycastHit hit)
    {
        for (int i = 0; i < InventorySlots.Length; i++)
        {
            if (InventorySlots[i].itemNetworkObject == null)
            {
                if (hit.collider.TryGetComponent(out NetworkObject networkObject))
                {
                    int slot;

                    if (InventorySlots[i] == InventorySlots[CurrentSlot.Value] || InventorySlots[CurrentSlot.Value] == null) 
                    {
                        slot = CurrentSlot.Value;
                    }
                    else 
                    {
                        slot = i;
                    }

                    PickupItemRpc((sbyte)slot ,networkObject);
                    BatteryUpdateCheck?.Invoke(InventorySlots[CurrentSlot.Value]);

                    return;
                }
            }
        }
    }
    [Rpc(SendTo.Everyone)]
    private void EquipSuitRpc(int suitValue)
    {
        foreach (var item in GameManager.Singleton.DivingSuits)
        {
            if (item.Level == suitValue)
            {
                DivingSuit suit = GameManager.Singleton.DivingSuits[suitValue];
                if (currentSuit.Level < suit.Level)
                {
                    if (IsOwner)
                    {
                        suit.SetEffects(NetworkObject);

                        UISetArmorEvent?.Invoke(suit.SuitImage);

                        RemoveItemBySlotRpc(true, CurrentSlot.Value);
                    }

                    currentSuit = suit;

                    skinnedMeshRenderer.material = suit.SuitMaterial;
                    skinnedMeshRenderer.sharedMesh = suit.SuitMesh;
                    return;
                }
                else
                {
                    if (IsOwner)
                    {
                        Debug.LogWarning("Your Equipped Suit is better then that one!");
                    }
                    return;
                }
            }
            
        }

    }

    [Rpc(SendTo.Server)]
    public void PickupItemRpc(sbyte slot, NetworkObjectReference itemReference)
    {
        if (itemReference.TryGet(out NetworkObject itemNetworkObject))
        {
            itemNetworkObject.TrySetParent(NetworkObject, false);

            itemNetworkObject.GetComponent<NetworkTransform>().Teleport(handPos.position, Quaternion.identity, Vector3.one);

            itemNetworkObject.transform.LookAt(handPos.forward);

            PickupItemClientRpc(slot, itemReference);

        }
        
    }

    [Rpc(SendTo.Everyone)]
    public void PickupItemClientRpc(sbyte slot, NetworkObjectReference itemReference)
    {
        if (itemReference.TryGet(out NetworkObject itemNetworkObject))
        {
            ParentConstraint parentConstraint = itemNetworkObject.GetComponent<ParentConstraint>();

            ItemInfo info = itemNetworkObject.GetComponent<ItemInfo>();

            info.rb.isKinematic = true;
            info.GetComponent<Collider>().isTrigger = true;

            if (IsOwner)
            {
                CurrentWeight.Value += itemNetworkObject.GetComponent<ItemInfo>().ItemWeight.Value;
                
                SelectSlotRpc(slot);

                info.OnPickedUp(NetworkObject);

                info.TogglePickedUpRpc(true);

                UISetSlotImageEvent?.Invoke(slot, true);
            }

            if (parentConstraint.sourceCount > 0)
            {
                parentConstraint.RemoveSource(0);
            }

            ConstraintSource constraintSource = new()
            {
                sourceTransform = handPos,
                weight = 1
            };

            parentConstraint.AddSource(constraintSource);

            itemNetworkObject.gameObject.layer = 2;


            InventorySlots[slot] = new()
            {
                itemNetworkObject = itemNetworkObject,
                
                itemInfo = info,

            };

        }
        else Debug.LogError("Failed to get =ITEM= Reference in Inventory at PickupItemRpc");
    }
    
    [Rpc(SendTo.Server)]
    public void DropItemServerRpc()
    {
        InventorySlot slot = InventorySlots[CurrentSlot.Value];

        slot.itemNetworkObject.TryRemoveParent();

        slot.itemNetworkObject.GetComponent<NetworkTransform>().Teleport(dropPos.position, Quaternion.identity, Vector3.one);

        slot.itemNetworkObject.GetComponent<Rigidbody>().position = dropPos.position;

        DropItemClientRpc();

    }

    [Rpc(SendTo.Everyone)]
    public void DropItemClientRpc()
    {
        InventorySlot slot = InventorySlots[CurrentSlot.Value];

        slot.itemInfo.rb.isKinematic = false;

        slot.itemNetworkObject.GetComponent<Collider>().isTrigger = false;

        slot.itemNetworkObject.GetComponent<ParentConstraint>().RemoveSource(0);

        if (IsOwner)
        {

            UISetSlotImageEvent?.Invoke(CurrentSlot.Value, false);

            slot.itemInfo.OnDropped(NetworkObject);

            slot.itemInfo.TogglePickedUpRpc(false);

            CurrentWeight.Value -= slot.itemInfo.ItemWeight.Value;
        }   

        slot.itemNetworkObject.GetComponent<Rigidbody>().rotation = Quaternion.identity;

        slot.itemNetworkObject.gameObject.layer = 6;

        InventorySlots[CurrentSlot.Value] = new()
        {
            itemInfo = null,
            itemNetworkObject = null,
        };

    }
    [Rpc(SendTo.Everyone)]
    public void SelectSlotRpc(sbyte value)
    {
        if (IsOwner) 
        {
            CurrentSlot.Value = value;
            UISelectSlotEvent?.Invoke(value);
            BatteryUpdateCheck?.Invoke(InventorySlots[value]);
        }
        for (int i = 0; i < InventorySlots.Length; i++)
        {
            if (InventorySlots[i].itemNetworkObject != null)
            {
                if (InventorySlots[i] == InventorySlots[value])
                {
                    InventorySlots[i].itemNetworkObject.gameObject.SetActive(true);
                }
                else 
                {
                    InventorySlots[i].itemNetworkObject.gameObject.SetActive(false);
                }
            }
        }
    }
    [Rpc(SendTo.Everyone)]
    public void RemoveItemBySlotRpc(bool despawn, sbyte value)
    {
        if (IsServer && despawn && InventorySlots[value].itemNetworkObject != null)
        {
            InventorySlots[value].itemNetworkObject.Despawn(true);
        }
        
        if (IsOwner)
        {
            BatteryUpdateCheck?.Invoke(InventorySlots[CurrentSlot.Value]);
            UISetSlotImageEvent?.Invoke(value, false);
        }

        InventorySlots[value] = new()
        {
            itemInfo = null,
            itemNetworkObject = null,
        };


        
    }
    [Rpc(SendTo.Server)]
    public void AddItemToInventoryRpc(int id, NetworkObjectReference reference)
    {
        if (reference.TryGet(out NetworkObject playerNetworkObject))
        {
            if (playerNetworkObject.TryGetComponent(out Inventory inventory))
            {
                foreach (var item in GameManager.Singleton.Items)
                {
                    if (id == item.ID)
                    {
                        NetworkObject networkObject = Instantiate(item.gameObject, inventory.dropPos.position + Vector3.up, Quaternion.identity).GetComponent<NetworkObject>();
                        networkObject.Spawn();

                        for (int i = 0; i < inventory.InventorySlots.Length; i++)
                        {
                            if (inventory.InventorySlots[i].itemNetworkObject == null)
                            {
                                int slot;

                                if (inventory.InventorySlots[i] == inventory.InventorySlots[inventory.CurrentSlot.Value] || inventory.InventorySlots[inventory.CurrentSlot.Value] == null) 
                                {
                                    slot = inventory.CurrentSlot.Value;
                                }
                                else 
                                {
                                    slot = i;
                                }

                                inventory.PickupItemRpc((sbyte)slot ,networkObject);
                                inventory.BatteryUpdateCheck?.Invoke(InventorySlots[CurrentSlot.Value]);

                                return;
                            }
                        }
                        
                    }
                }
            }
        }
    }
}