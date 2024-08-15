using UnityEngine;

[CreateAssetMenu(menuName = "Quest")]
public class QuestSO : ScriptableObject
{
    public int QuestID;
    public ItemInfo ItemInfo;
    public int DaysToComplete;
    public int Reward;
    [HideInInspector] public int ItemID;
    [HideInInspector] public Sprite sprite;

    public void Awake()
    {
        ItemID = ItemInfo.ID;
        sprite = ItemInfo.itemImage;
    }
}
