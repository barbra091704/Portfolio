using UnityEngine;
using System;
using Unity.Netcode;
using UnityEngine.PlayerLoop;

public class QuestManager : NetworkBehaviour, IInteractable
{
    public static QuestManager Singleton;

    public NetworkVariable<int> CurrentQuestID;

    public QuestSO CurrentQuest;

    public QuestSO[] Quests;

    public event Action<Sprite, string, int> OnQuestInfoUpdated;

    void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
            return;
        }
        Singleton = this;
    }

    public void Start()
    {
        CurrentQuestID.OnValueChanged += UpdateQuestInfo;

        if (IsServer)
        {
            SelectRandomQuest();
        }
        else
        {
            UpdateQuestInfo(0, CurrentQuestID.Value);
        }
    }
    public override void OnNetworkDespawn()
    {
        CurrentQuestID.OnValueChanged -= UpdateQuestInfo;
    }

    private void UpdateQuestInfo(int previousValue, int newValue)
    {
        Sprite sprite = Quests[newValue].sprite;
        CurrentQuest = Quests[newValue];
        OnQuestInfoUpdated?.Invoke(sprite, CurrentQuest.ItemInfo.name, CurrentQuest.DaysToComplete);
    }

    public void SelectRandomQuest()
    {
        CurrentQuestID.Value = UnityEngine.Random.Range(0, Quests.Length);

        CurrentQuest = Quests[CurrentQuestID.Value];
    }

    [Rpc(SendTo.Server)]
    public void CompleteQuestRpc()
    {
        int reward = CurrentQuest.Reward;
        GameManager.Singleton.Credits.Value += reward;

        SelectRandomQuest();
    }

    public void Interact<T>(RaycastHit hit, NetworkObject Player, T type)
    {
        if (Player.TryGetComponent(out Inventory inventory))
        {
            if (inventory.InventorySlots[inventory.CurrentSlot.Value].itemInfo != null && inventory.InventorySlots[inventory.CurrentSlot.Value].itemInfo.ID == CurrentQuest.ItemID)
            {
                inventory.RemoveItemBySlotRpc(true, inventory.CurrentSlot.Value);
                CompleteQuestRpc();
            }
        }
    }
}

