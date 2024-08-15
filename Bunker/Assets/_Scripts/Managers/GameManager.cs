using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using Unity.Collections;

[Serializable]
public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
{
    public FixedString64Bytes Name;
    public NetworkObjectReference Reference;
    public Vector3 Position;
    public ulong clientID;
    public bool IsSleeping;
    public bool IsAlive;
    public bool IsInBunker;

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
        serializer.SerializeValue(ref Position);
        serializer.SerializeValue(ref IsInBunker);
    }
}
public class GameManager : NetworkBehaviour
{
    public static GameManager Singleton;
    public Light Sun;
    public NetworkVariable<bool> AllPlayersSlept = new(true);
    public NetworkVariable<bool> AllPlayersAsleep = new(false);
    [HideInInspector] public NetworkList<PlayerData> networkPlayerDatas;
    [SerializeField] private List<PlayerData> PlayerDatas = new();

    public void Awake()
    {
        networkPlayerDatas = new();
        Singleton = Singleton != null && Singleton != this ? null : this;
    }
    public override void OnNetworkSpawn()
    {
        networkPlayerDatas.OnListChanged += SetPlayerDataList;

        AllPlayersAsleep.OnValueChanged += Sleep;

    }
    public void OnEnable()
    {
        if (!IsServer) return;

        InvokeRepeating(nameof(UpdateAllPlayersPositionAndRotation), 1f, .5f);
    }
    public void OnDisable()
    {
        if (!IsServer) return;

        CancelInvoke(nameof(UpdateAllPlayersPositionAndRotation));
    }

    public void UpdateAllPlayersPositionAndRotation()
    {
        for (int i = 0; i < networkPlayerDatas.Count; i++)
        {
            if (networkPlayerDatas[i].Reference.TryGet(out NetworkObject networkObject) && networkObject.transform.position != networkPlayerDatas[i].Position)
            {
                PlayerData playerData = PlayerDatas[i];
                playerData.Position = networkObject.transform.position;
                networkPlayerDatas[i] = playerData;
            }
        }
    }


    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.OnClientDisconnectCallback -= RemovePlayerDataRpc;
        }
    }
    private void SetPlayerDataList(NetworkListEvent<PlayerData> changeEvent)
    {
        switch(changeEvent.Type)
        {
            case NetworkListEvent<PlayerData>.EventType.Add:
                PlayerDatas.Add(changeEvent.Value);
                break;
            case NetworkListEvent<PlayerData>.EventType.Remove:
                PlayerDatas.Remove(changeEvent.Value);
                break;
            case NetworkListEvent<PlayerData>.EventType.Clear:
                PlayerDatas.Clear();
                break;
            case NetworkListEvent<PlayerData>.EventType.Value:
                PlayerDatas[GetPlayerDataIndex(changeEvent.PreviousValue)] = changeEvent.Value;
                AllPlayersAsleepCheck();
                break;
        }
    }

    private void AllPlayersAsleepCheck()
    {
        if (!IsServer) return;

        int playersSleepingAmount = 0;

        foreach (var item in PlayerDatas)
        {
            if (item.IsSleeping)
            {
                playersSleepingAmount++;
            }
        }

        if (playersSleepingAmount >= PlayerDatas.Count)
        {
            AllPlayersAsleep.Value = true;
        }
        else
        {
            AllPlayersAsleep.Value = false;
        }
    }

    private void Sleep(bool previousValue, bool newValue)
    {
        StopCoroutine(nameof(SleepCoroutine));
        StartCoroutine(SleepCoroutine());
    }
    private IEnumerator SleepCoroutine()
    {
        print("Sleeping");
        yield return new WaitForSeconds(3f);

        if (AllPlayersAsleep.Value)
        {
            print("Slept!");
            GUIManager.Singleton.RemoveGUI();

            if (IsServer)
            {
                AllPlayersSlept.Value = true;
            }
        }
        else 
        {
            print("Sleep failed!");
            yield break;
        }
    }

    public void ToggleSun(bool value)
    {
        Sun.enabled = value;
    }

    //PLAYERDATAS

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
        networkPlayerDatas.Add(playerdata);
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void RemovePlayerDataRpc(ulong disconnectedClientId)
    {
        for (int i = 0; i < networkPlayerDatas.Count; i++)
        {
            if (networkPlayerDatas[i].clientID == disconnectedClientId)
            {
                networkPlayerDatas.RemoveAt(i);
            }
        }
    }

    [Rpc(SendTo.Server)]
    public void SetPlayerAliveStateRpc(ulong ClientId, bool value)
    {
        for (int i = 0; i < networkPlayerDatas.Count; i++)
        {
            if (networkPlayerDatas[i].clientID == ClientId)
            {
                PlayerData information = networkPlayerDatas[i];
                information.IsAlive = value;
                networkPlayerDatas[i] = information;
            }
        }
    }
    [Rpc(SendTo.Server)]
    public void SetPlayerSleepingStateRpc(ulong ClientId, bool value)
    {
        for (int i = 0; i < networkPlayerDatas.Count; i++)
        {
            if (networkPlayerDatas[i].clientID == ClientId)
            {
                PlayerData information = networkPlayerDatas[i];
                information.IsSleeping = value;
                networkPlayerDatas[i] = information;
            }
        }
    }
    [Rpc(SendTo.Server)]
    public void SetPlayerOnShipwreckRpc(ulong ClientId, bool value)
    {
        for (int i = 0; i < networkPlayerDatas.Count; i++)
        {
            if (networkPlayerDatas[i].clientID == ClientId)
            {
                PlayerData information = networkPlayerDatas[i];
                information.IsInBunker = value;
                networkPlayerDatas[i] = information;
            }
        }
    }
    public int GetPlayerDataIndex(ulong ClientId)
    {
        for (int i = 0; i < networkPlayerDatas.Count; i++)
        {
            if (networkPlayerDatas[i].clientID == ClientId)
            {
                return i;
            }
        }
        return 0;
    }
    public int GetPlayerDataIndex(PlayerData data)
    {
        for (int i = 0; i < networkPlayerDatas.Count; i++)
        {
            if (networkPlayerDatas[i].clientID == data.clientID)
            {
                return i;
            }
        }
        return 0;
    }
}


