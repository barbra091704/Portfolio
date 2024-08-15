using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
public class ItemGenerator : NetworkBehaviour
{
    [SerializeField] private List<GameObject> itemPrefabs = new();
    private LevelGenerator.Scripts.LevelGenerator levelGenerator;
    public List<Transform> itemsSpawned = new();
    public List<Vector3> SpawnPoints = new();
    public int minValue;
    public int maxValue;
    public int totalValue;
    private int previousSpawnNumber;

    void Start()
    {
        levelGenerator = GetComponent<LevelGenerator.Scripts.LevelGenerator>();
    }

    public GameObject PickRandomItemPrefab()
    {
        return itemPrefabs[Random.Range(0, itemPrefabs.Count)];
    }
    public Vector3 PickRandomPosition(){
        if(SpawnPoints.Count == 0) {
            Debug.LogError("SpawnPoints list is empty.");
            return Vector3.zero;
        }

        previousSpawnNumber = Random.Range(0, SpawnPoints.Count);
        if (SpawnPoints[previousSpawnNumber] == Vector3.zero) PickRandomPosition();
        return SpawnPoints[previousSpawnNumber];
    }
    private void UpdateItemValues()
    {
        int creditMultiplier = GameManager.Singleton.Credits.Value / 1000;
        maxValue = 80 + 20 * creditMultiplier; // Increase maxValue by 20 for every 1000 credits
        minValue = 10 + 10 * creditMultiplier; // Increase minValue by 10 for every 1000 credits
    }
    public void GenerateItemsUpToExpectedValue()
    {
        totalValue = 0;
        UpdateItemValues();
        while (totalValue < levelGenerator.ExpectedValue)
        {
            Vector3 position = PickRandomPosition();
            GameObject itemObject = Instantiate(PickRandomItemPrefab(), position, Quaternion.identity);
            NetworkObject itemNetworkObject = itemObject.GetComponent<NetworkObject>();
            itemNetworkObject.Spawn(true);
            itemNetworkObject.TrySetParent(NetworkObject);
            SpawnPoints.RemoveAt(previousSpawnNumber);
            if (itemObject.TryGetComponent<ItemInfo>(out var itemInfo))
            {
                int itemValue = Random.Range(minValue, maxValue + 1);
                itemInfo.ItemValue.Value = itemValue;
                totalValue += itemValue;

                // Break the loop if total value exceeds or equals ExpectedValue
                itemsSpawned.Add(itemObject.transform);
                if (totalValue >= levelGenerator.ExpectedValue) break;
            }
        }
    }
    public void RemoveAllItemsSpawned(Collider collider)
    {
        Bounds bounds = collider.bounds;

        for (int i = itemsSpawned.Count - 1; i >= 0; i--)
        {
            Transform itemTransform = itemsSpawned[i];
            if (itemTransform != null && !bounds.Contains(itemTransform.position))
            {
                if (itemTransform.gameObject.TryGetComponent<NetworkObject>(out var networkObject))
                {
                    itemsSpawned.RemoveAt(i); // Remove the item from the list
                    networkObject.Despawn(); // Despawn the item
                }
            }
        }
    }
}
