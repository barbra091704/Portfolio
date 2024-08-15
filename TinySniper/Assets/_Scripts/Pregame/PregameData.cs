using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public struct PlayerSelection : INetworkSerializable, IEquatable<PlayerSelection>
{
    public ulong clientId;
    public int selectedPlayerModelIndex;

    public PlayerSelection(ulong clientId, int selectedPlayerModelIndex)
    {
        this.clientId = clientId;
        this.selectedPlayerModelIndex = selectedPlayerModelIndex;
    }

    public bool Equals(PlayerSelection other)
    {
        return clientId == other.clientId && selectedPlayerModelIndex == other.selectedPlayerModelIndex;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref selectedPlayerModelIndex);
    }
}

public class PregameData : NetworkBehaviour
{
    public NetworkList<PlayerSelection> playerSelections = new();
    public NetworkVariable<int> mapSelection;
    public NetworkObject[] maps;

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void SetToDestroyOnLoad()
    {
        DontDestroyOnLoad(this);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Invoke(nameof(SpawnMap),0.1f);
    }

    private void SpawnMap()
    {
        if (!IsServer) return;
        NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(maps[mapSelection.Value], OwnerClientId, true, false, false, Vector3.zero, Quaternion.identity);
    }
}
