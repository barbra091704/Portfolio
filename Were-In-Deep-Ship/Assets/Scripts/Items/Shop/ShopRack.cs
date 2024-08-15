using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ShopRack : NetworkBehaviour
{
    public List<Transform> rackLocation = new();
    public ShopItem currentSelectedItem;
    public Canvas purchaseCanvas;
    public TMP_Text cost;
    private NetworkObject player;

    public void OpenMenu(NetworkObject player)
    {
        if (player != null)
        {
        }
    }
    public void CloseMenu()
    {
        if (player != null)
        {
            purchaseCanvas.enabled = false;
            GUIManager.Singleton.RemoveGUI();
        }
    }
    public void PurchaseItem()
    {
        CloseMenu();
    }

    [Rpc(SendTo.Server)]
    public void PurchaseItemRpc(NetworkObjectReference playerReference, int id, int cost)
    {
        if (playerReference.TryGet(out NetworkObject networkObject))
        {
            if (networkObject.TryGetComponent(out Inventory inventory))
            {
                print("purhcased");
                inventory.AddItemToInventoryRpc(id, playerReference);
                GameManager.Singleton.Credits.Value -= cost;
            }
        }
    }
}
