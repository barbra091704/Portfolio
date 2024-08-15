using System;
using Cinemachine;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct ShopItem
{
    public ItemInfo itemInfo;
    public int cost;

}
[Serializable]
public struct ConsolePage
{
    public Image page;
    public string name;
}
public class Console : NetworkBehaviour, IInteractable, IGUI
{
    public NetworkVariable<bool> InUse = new(false);

    public NetworkVariable<NetworkObjectReference> CurrentUserReference = new();

    public ShopItem[] shopItems;

    public Canvas canvas;

    public CinemachineVirtualCamera vcamera;

    public TMP_Text ConfirmationScreenPrice;

    public TMP_Text QuantityText;
    
    public Transform ItemSpawnPosition;

    private int Quantity;

    private int currentItemID = -1;

    private event Action E_OnQuantityChange;

    [Header("Pages")]
    public ConsolePage[] pages;

    void Awake()
    {
        E_OnQuantityChange += SetCheckoutPriceScreen;
    }

    public void Interact<T>(RaycastHit hit, NetworkObject Player, T type)
    {
        if (InUse.Value) return;

        EnterConsole(Player);
    }

    public void EnterConsole(NetworkObject Player)
    {
        GUIManager.Singleton.AddGUI(this);

        vcamera.enabled = true;
        SetPageActiveByName("Main", true);
        SetUserReferenceRpc(Player);
        SetEventCameraRpc(Player);
        SetInUseRpc(true);
    }

    public void CloseGUI()
    {
        vcamera.enabled = false;
        SetPageActiveByName("Main", false);
        SetInUseRpc(false);
    }

    //Console Functions

    public void ToggleTierPage(int tier)
    {
        SetPageActiveByName("Tier1", false);
        SetPageActiveByName("Tier2", false);
        SetPageActiveByName("Tier3", false);
        switch(tier)
        {
            case 1:
                SetPageActiveByName("Tier1", true);
                break;
            case 2:
                SetPageActiveByName("Tier2", true);
                break;
            case 3:
                SetPageActiveByName("Tier3", true);
                break;
            default:
                SetPageActiveByName("Tier1", true);
                break;
        }
    }
    public void SetQuantity(bool add)
    {
        Quantity = add ? Quantity + 1 : Quantity - 1;
        Quantity = Mathf.Clamp(Quantity, 1, 10);
        QuantityText.text = Quantity.ToString();
        E_OnQuantityChange?.Invoke();
    }

    public void OpenCheckoutScreen(int id)
    {
        SetPageActiveByName("ConfirmPurchase", true);
        int cost = GetShopItemByID(id).cost;
        ConfirmationScreenPrice.text = $"${cost}";
        currentItemID = id;
    }
    public void SetCheckoutPriceScreen()
    {
        int cost = GetShopItemByID(currentItemID).cost;
        ConfirmationScreenPrice.text = $"${cost * Quantity}";
    }
    public void CloseCheckoutScreen()
    {
        SetPageActiveByName("ConfirmPurchase", false);
        currentItemID = -1;
        Quantity = 1;
        QuantityText.text = "1";
    }

    [Rpc(SendTo.Server)]
    public void PurchaseItemRpc()
    {
        ShopItem shopItem = GetShopItemByID(currentItemID);
        print("Purchased item with id: " + currentItemID + " with Quantity: " + Quantity);
        
        if (GameManager.Singleton.Credits.Value >= shopItem.cost * Quantity)
        {
            GameManager.Singleton.Credits.Value -= shopItem.cost * Quantity;
            for (int i = 0; i < Quantity; i++)
            {
                NetworkObject obj = Instantiate(GameManager.Singleton.GetItemFromID(shopItem.itemInfo.ID), ItemSpawnPosition.position, Quaternion.identity).GetComponent<NetworkObject>();
                obj.Spawn();
                obj.TrySetParent(GameManager.Singleton.PierTransform);
            }
            CloseCheckoutScreen();
        }
        else
        {
            Debug.LogWarning("Not enough Credits");
        }
        
    }

    //RPC's

    

    [Rpc(SendTo.Everyone)]
    public void SetEventCameraRpc(NetworkObjectReference networkObjectReference)
    {
        if(networkObjectReference.TryGet(out NetworkObject networkObject))
        {
            canvas.worldCamera = networkObject.GetComponentInChildren<Camera>();
        }
    }

    [Rpc(SendTo.Server)]
    public void SetUserReferenceRpc(NetworkObjectReference networkObjectReference)
    {
        CurrentUserReference.Value = networkObjectReference;
    }

    [Rpc(SendTo.Server)]
    public void SetInUseRpc(bool value)
    {
        InUse.Value = value;
    }

    //Helper Functions

    public ConsolePage GetPageByName(string name)
    {
        foreach (var page in pages)
        {
            if (page.name == name)
            {
                return page;
            }
        }
        return new ConsolePage();
    }
    public ShopItem GetShopItemByID(int id)
    {
        foreach (var item in shopItems)
        {
            if (item.itemInfo.ID == id)
            {
                return item;
            }
        }
        return new ShopItem();
    }
    public void TogglePageByName(string name)
    {
        foreach (var page in pages)
        {
            if (page.name == name)
            {
                page.page.gameObject.SetActive(!page.page.gameObject.activeSelf);
            }
        }
    }
    public void SetPageActiveByName(string name, bool value)
    {
        foreach (var page in pages)
        {
            if (page.name == name)
            {
                page.page.gameObject.SetActive(value);
            }
        }
    }
}
