using UnityEngine;
using Unity.Netcode;
using UnityEngine.Splines;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class EntityManager : NetworkBehaviour
{
    public MapDangerLevel mapDangerLevel;
    
    public SpawnSchedule[] landSpawnSchedules;

    public SpawnSchedule[] waterSpawnSchedules;
    
    public SpawnableEntity[] waterEntities;

    public SpawnableEntity[] landEntities;

    public SplinesByDepth[] splinesByDepths;

    public List<NetworkObject> spawnedEntities = new();

    private ItemGenerator itemGenerator;

    private int waterIndex;
    private int landIndex;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        itemGenerator = GetComponent<ItemGenerator>();
    }
    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;

        CleanupEntites();
    }
    public void FixedUpdate()
    {
        if (!IsServer) return;

        SpawnLandEntityBySchedule();
        SpawnWaterEntityBySchedule();
    }
    public void SpawnLandEntityBySchedule()
    {
        if (landIndex < landSpawnSchedules.Length)
        {
            if (TimeManager.Singleton.GameTime.Value >= landSpawnSchedules[landIndex].spawnTime)
            {
                int amount = landSpawnSchedules[landIndex].enemySpawnAmount;

                int dividen = mapDangerLevel == 
                            MapDangerLevel.D ? amount / 2 : mapDangerLevel == 
                            MapDangerLevel.C ? amount * 1 : mapDangerLevel == 
                            MapDangerLevel.B ? amount * 2 : mapDangerLevel == 
                            MapDangerLevel.A ? amount * 3 : amount;

                for (int i = 0; i < dividen; i++)
                {
                    SpawnableEntity entity = GetRandomEntity(landEntities, true);
                    Vector3 position = GetRandomIndoorPosition();
                    SpawnEntity(entity.prefab, position);
                }
                landIndex++;
            }
        }
        else return;
    }
    public void SpawnWaterEntityBySchedule()
    {
        if (waterIndex < waterSpawnSchedules.Length)
        {
            if (TimeManager.Singleton.GameTime.Value >= waterSpawnSchedules[waterIndex].spawnTime)
            {
                int enemySpawnAmount = waterSpawnSchedules[waterIndex].enemySpawnAmount;
                int friendlySpawnAmount = waterSpawnSchedules[waterIndex].friendlySpawnAmount;

                int dividen = mapDangerLevel == 
                            MapDangerLevel.D ? enemySpawnAmount / 2 : mapDangerLevel == 
                            MapDangerLevel.C ? enemySpawnAmount * 1:     mapDangerLevel == 
                            MapDangerLevel.B ? enemySpawnAmount * 2 : mapDangerLevel == 
                            MapDangerLevel.A ? enemySpawnAmount * 3 : enemySpawnAmount;

                for (int i = 0; i < dividen; i++)
                {
                    SpawnableEntity entity = GetRandomEntity(waterEntities, true);
                    Vector3 position = GetRandomSpawnKnot(entity);
                    SpawnEntity(entity.prefab, position);
                }
                for (int i = 0; i < friendlySpawnAmount; i++)
                {
                    SpawnableEntity entity = GetRandomEntity(waterEntities, false);
                    Vector3 position = GetRandomSpawnKnot(entity);
                    SpawnEntity(entity.prefab, position);  
                }
                waterIndex++;
            }
        }
        else return;
    }
    private SpawnableEntity GetRandomEntity(SpawnableEntity[] entities, bool enemy)
    {
        // Generate a random index to start searching from
        int startIndex = UnityEngine.Random.Range(0, entities.Length);
        
        // Iterate over the entities starting from the randomly chosen index
        for (int i = startIndex; i < entities.Length + startIndex; i++)
        {
            // Wrap around the array index to ensure we loop over all entities
            int index = i % entities.Length;

            // Check if the entity matches the criteria
            if (enemy && entities[index].entityData.CanAttack)
            {
                return entities[index];
            }
            else if (!enemy && !entities[index].entityData.CanAttack)
            {
                return entities[index];
            }
        }

        // Return default if no entity matches the criteria
        return default;
    }


    private Vector3 GetRandomIndoorPosition()
    {
        int numAttempts = 0;
        int maxAttempts = 10; // You can adjust this value

        while (numAttempts < maxAttempts)
        {
            int num = UnityEngine.Random.Range(0, itemGenerator.SpawnPoints.Count);
            Vector3 randomPosition = itemGenerator.SpawnPoints[num];

            // Check if there's a collider within 10m distance
            Collider[] colliders = Physics.OverlapSphere(randomPosition, 10f);
            bool isValidPosition = true;

            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Player"))
                {
                    isValidPosition = false;
                    break;
                }
            }

            if (isValidPosition)
            {
                return randomPosition;
            }

            numAttempts++;
        }

        Debug.LogWarning("Unable to find a valid indoor position.");

        return Vector3.zero;
    }
    private Vector3 GetRandomSpawnKnot(SpawnableEntity entity)
    {
        if (splinesByDepths.Length > 0)
        {
            foreach (var splineDepth in splinesByDepths)
            {
                if (splineDepth.DepthRange.x <= entity.entityData.DepthRange.x && splineDepth.DepthRange.y >= entity.entityData.DepthRange.x)
                {
                    // Get a random spline from the spline container
                    int randomSplineIndex = UnityEngine.Random.Range(0, splineDepth.SplineContainer.Splines.Count);
                    var spline = splineDepth.SplineContainer.Splines[randomSplineIndex];

                    // Get a random knot from the spline
                    int randomKnotIndex = UnityEngine.Random.Range(0, spline.Count);
                    var randomKnot = spline[randomKnotIndex];

                    Vector3 position = transform.TransformPoint(randomKnot.Position);

                    return position;
                }
            }
        }

        return Vector3.zero;
    }
    private void SpawnEntity(NetworkObject prefab, Vector3 spawnPosition)
    {
        if (prefab == null) return; 
        
        NetworkObject entity = Instantiate(prefab, spawnPosition, Quaternion.identity);

        entity.Spawn();

        SceneManager.MoveGameObjectToScene(entity.gameObject, SceneManager.GetSceneByName(GameManager.Singleton.CurrentLocation.Value.SceneName));

        spawnedEntities.Add(entity);
        
        
    }
    private void CleanupEntites()
    {
        foreach (var item in spawnedEntities)
        {
            if (item.IsSpawned) item.Despawn(true);
        }
    }
}

[Serializable]
public enum EntityTerrainType
{
    Water,
    Land,
}   
[Serializable]
public enum EntityType
{
    Predator,
    Friendly
}
[Serializable]
public enum MapDangerLevel
{
    D,
    C,
    B,
    A
}
[Serializable]
public struct SpawnSchedule
{
    public float spawnTime;
    public int enemySpawnAmount;
    public int friendlySpawnAmount;
}
[Serializable]
public struct SpawnableEntity
{
    public NetworkObject prefab;
    public EntityData entityData;
}
[Serializable]
public struct SplinesByDepth
{
    public SplineContainer SplineContainer;
    public Vector2 DepthRange;
}

