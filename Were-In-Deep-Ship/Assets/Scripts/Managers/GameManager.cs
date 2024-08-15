using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System;
using Unity.Collections;

[Serializable]
public struct LocationData : INetworkSerializable
{
    public string SceneName;
    public int ID;
    public int Cost;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref SceneName);
        serializer.SerializeValue(ref ID);
        serializer.SerializeValue(ref Cost);
    }
}
[Serializable]
public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
{
    public FixedString64Bytes Name;
    public NetworkObjectReference Reference;
    public ulong clientID;
    public bool IsAlive;

    public readonly bool Equals(PlayerData other)
    {
        if (other.Name == Name && other.clientID == clientID)
        {
            return true;
        }
        return false;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Name);
        serializer.SerializeValue(ref Reference);
        serializer.SerializeValue(ref clientID);
        serializer.SerializeValue(ref IsAlive);
    }
}
[Serializable]
public enum GameOverReason
{
    Quest,
    Death,
} 
public class GameManager : NetworkBehaviour
{
    public static GameManager Singleton;

    public NetworkVariable<int> Credits = new(100);
    public Transform PierTransform;
    public LocationData[] Locations;
    public NetworkVariable<LocationData> CurrentLocation = new();
    [HideInInspector] public NetworkList<PlayerData> PlayerDatas;
    [SerializeField] private List<PlayerData> serverPlayerDatas;
    public List<DivingSuit> DivingSuits = new();
    public List<ItemInfo> Items = new();
    private Scene CurrentLocationScene;
    private Scene PreviousLocationScene;
    public void Awake()
    {
        PlayerDatas = new();
        Singleton = Singleton != null && Singleton != this ? null : this;
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.OnClientDisconnectCallback += RemovePlayerDataRpc;

            PlayerDatas.OnListChanged += SetServerPlayerDataList;

            CurrentLocation.Value = Locations[0];

            SceneManager.SetActiveScene(gameObject.scene);
        }

        CurrentLocation.OnValueChanged += HandleNewLocation;
    }

    private void SetServerPlayerDataList(NetworkListEvent<PlayerData> changeEvent)
    {
        switch(changeEvent.Type)
        {
            case NetworkListEvent<PlayerData>.EventType.Add:
                serverPlayerDatas.Add(changeEvent.Value);
                break;
            case NetworkListEvent<PlayerData>.EventType.Remove:
                serverPlayerDatas.Remove(changeEvent.Value);
                break;
            case NetworkListEvent<PlayerData>.EventType.Clear:
                serverPlayerDatas.Clear();
                break;
        }
    }

    public override void OnNetworkDespawn()
    {
        CurrentLocation.OnValueChanged -= HandleNewLocation;
        if (IsServer)
        {
            NetworkManager.OnClientDisconnectCallback -= RemovePlayerDataRpc;
        }
    }
    [Rpc(SendTo.Server)]
    public void LoadLocationRpc(int id)
    {
        foreach (var item in Locations)
        {
            if (item.ID == id)
            {
                CurrentLocation.Value = item;
            }
        }
    }
    private void HandleNewLocation(LocationData previousValue, LocationData newValue)
    {
        StartCoroutine(DelayedHandleNewLocation(previousValue, newValue));
    }
    private IEnumerator DelayedHandleNewLocation(LocationData previousValue, LocationData newValue)
    {
        yield return new WaitForSeconds(1f); // Wait for 1 second

        if (IsServer && !previousValue.Equals(newValue))
        {
            PreviousLocationScene = CurrentLocationScene;

            if (newValue.ID != 0) 
            {
                if (previousValue.ID != 0) FindObjectOfType<LevelGenerator.Scripts.LevelGenerator>().Cleanup();

                NetworkManager.SceneManager.OnLoadEventCompleted += UnloadPreviousLocation;

                NetworkManager.SceneManager.LoadScene(newValue.SceneName, LoadSceneMode.Additive);
                
                CurrentLocationScene = SceneManager.GetSceneByName(newValue.SceneName);

                TimeManager.Singleton.StartGameTime();
            }
            else 
            {
                FindObjectOfType<LevelGenerator.Scripts.LevelGenerator>().Cleanup();

                NetworkManager.SceneManager.OnLoadEventCompleted += UnloadPreviousLocation;

                NetworkManager.SceneManager.UnloadScene(PreviousLocationScene);

                TimeManager.Singleton.ResetGameTime();
            }
        }
        if (newValue.ID == 0)
        {
            PierTransform.gameObject.SetActive(true);
        }
        else
        {
            PierTransform.gameObject.SetActive(false);
        }
    }

    private void UnloadPreviousLocation(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (!PreviousLocationScene.isLoaded) return;
        // Unsubscribe from the OnLoadEventCompleted event
        NetworkManager.SceneManager.OnLoadEventCompleted -= UnloadPreviousLocation;

        // Unload the previous scene
        NetworkManager.SceneManager.UnloadScene(PreviousLocationScene);

    }
    [Rpc(SendTo.Server)]
    public void EndGameRpc(GameOverReason reason)
    {

    }
    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void AddPlayerDataRpc(FixedString32Bytes name, ulong id, bool isAlive, NetworkObjectReference reference)
    {
        PlayerData playerdata = new()
        {
            Name = name,
            clientID = id,
            IsAlive = isAlive,
            Reference = reference
        };
        PlayerDatas.Add(playerdata);
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void RemovePlayerDataRpc(ulong disconnectedClientId)
    {
        for (int i = 0; i < PlayerDatas.Count; i++)
        {
            if (PlayerDatas[i].clientID == disconnectedClientId)
            {
                PlayerDatas.RemoveAt(i);
            }
        }
    }

    [Rpc(SendTo.Server)]
    public void SetPlayerAliveStateServerRpc(ulong ClientId, bool value)
    {
        for (int i = 0; i < PlayerDatas.Count; i++)
        {
            if (PlayerDatas[i].clientID == ClientId)
            {
                PlayerData information = PlayerDatas[i];
                information.IsAlive = value;
                PlayerDatas[i] = information;
            }
        }
    }
    public ItemInfo GetItemFromID(int id)
    {
        foreach (var item in Items)
        {
            if (item.ID == id)
            {
                return item;
            }
        }
        return null;
    }
}


