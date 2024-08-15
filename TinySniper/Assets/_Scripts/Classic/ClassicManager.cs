using System;
using System.Collections;
using System.Collections.Generic;
using FirstGearGames.SmoothCameraShaker;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public enum PlayerType
{
    Assassin,
    Thief,
    Sniper,
}

[Serializable]
public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
{
    public ulong Id;
    public PlayerType Type;
    public bool IsAlive;
    public bool HasWon;

    public bool Equals(PlayerData other)
    {
        return other.Id == Id && other.Type == Type && other.IsAlive == IsAlive && other.HasWon == HasWon;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Id);
        serializer.SerializeValue(ref Type);
        serializer.SerializeValue(ref IsAlive);
        serializer.SerializeValue(ref HasWon);
    }
}

public class ClassicManager : NetworkBehaviour
{
    public static ClassicManager Instance;

    public NetworkVariable<int> GameTime = new(); // Tracks game time in seconds

    public NetworkVariable<bool> IsEveryPlayerDead = new();

    [SerializeField] private int prefabCount;
    [SerializeField] private int pedestalCount;
    [SerializeField] private int CivilianCount;
    public ParticleSystem killParticles;
    public ParticleSystem stealParticles;
    public GameObject EndGameScreen;
    public TMP_Text EndScreenText;
    public GameObject EndScreenButton;
    public NetworkObject[] ThiefPrefabs;
    public NetworkObject[] AssassinPrefabs;
    [SerializeField] private NetworkObject[] Civilians;
    [SerializeField] private List<NetworkObject> pedestals = new();
    [SerializeField] private List<NetworkObject> prefabs = new();
    [SerializeField] private List<Transform> prefabLocations = new();
    [SerializeField] private NetworkObject Sniper;
    public Transform[] spawningPositions;
    public bool spawnAsNotSniper = false;
    private PregameData pregameData;

    public TMP_Text timerText;
    public SpriteRenderer mapSpriteRenderer;

    public NetworkList<PlayerData> playerData;
    public List<PlayerData> playerDataList; // Local copy for readability

    private int expectedPlayerCount;
    public int TargetTimeInSeconds = 300;

    private void Awake()
    {
        playerData = new();
        playerDataList = new();

        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(this);
    }

    public void Start()
    {
        playerData.OnListChanged += OnPlayerDataListChanged;

        GameTime.OnValueChanged += OnGameTimeChanged;

        NetworkManager.Singleton.OnClientDisconnectCallback += OnNetworkManagerDisconnectCallback;


        if (IsServer)
        {
            pregameData = FindObjectOfType<PregameData>();

            // Spawn civilians
            for (int i = 0; i < CivilianCount; i++)
            {
                int random = UnityEngine.Random.Range(0, Civilians.Length);
                NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(Civilians[random], 0, true, false, false, spawningPositions[UnityEngine.Random.Range(0, spawningPositions.Length)].position);
            }

            // Initialize expected player count
            expectedPlayerCount = SteamManager.currentLobby.Value.MemberCount;
            print($"Expected Players: {expectedPlayerCount}");

            PlacePrefabsAndPedestals();
            
            StartCoroutine(CheckAndSetTypes());

            StartCoroutine(GameTimer());
        }


    }

    private void OnNetworkManagerDisconnectCallback(ulong id)
    {
        if (id == 0)
        {
            BackToLobby();
        }
    }

    private void PlacePrefabsAndPedestals()
    {
        List<Transform> locationsCopy = new(prefabLocations);

        // Place pedestals first
        for (int i = 0; i < pedestalCount; i++)
        {
            if (locationsCopy.Count == 0)
            {
                Debug.LogWarning("No more locations available to place pedestals.");
                break;
            }
            int locationIndex = UnityEngine.Random.Range(0, locationsCopy.Count);
            int random = UnityEngine.Random.Range(0, pedestals.Count);
            NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(pedestals[random], 0, true, false, false, locationsCopy[locationIndex].position, Quaternion.identity);
            locationsCopy.RemoveAt(locationIndex);
        }

        // Place remaining prefabs
        for (int i = 0; i < prefabCount; i++)
        {
            if (locationsCopy.Count == 0)
            {
                Debug.LogWarning("No more locations available to place prefabs.");
                break;
            }
            int locationIndex = UnityEngine.Random.Range(0, locationsCopy.Count);
            int random = UnityEngine.Random.Range(0, prefabs.Count);
            NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(prefabs[random], 0, true, false, false, locationsCopy[locationIndex].position, Quaternion.identity);
        }
    }

    private void OnGameTimeChanged(int previousValue, int newValue)
    {
        int minutes = newValue / 60;
        int seconds = newValue % 60;

        string minutesText = minutes.ToString();
        string secondsText = seconds.ToString("D2");

        timerText.text = $"{minutesText}:{secondsText}";
    }


    private IEnumerator CheckAndSetTypes()
    {
        while (NetworkManager.Singleton.ConnectedClients.Count != expectedPlayerCount)
        {
            yield return new WaitForEndOfFrame();
        }

        SetTypesRandom();
    }

    private void SetTypesRandom()
    {
        Debug.Log("All players are connected. Setting player types.");

        List<ulong> clientIds = new(NetworkManager.Singleton.ConnectedClientsIds);
        
        if (!spawnAsNotSniper)
        {

            System.Random random = new();
            int sniperIndex = random.Next(clientIds.Count);
            ulong sniperId = clientIds[sniperIndex];

            playerData.Add(new PlayerData
            {
                Id = sniperId,
                Type = PlayerType.Sniper,
                IsAlive = true
            });
            SpawnPlayer(PlayerType.Sniper, sniperId);

            clientIds.RemoveAt(sniperIndex);
        }

        foreach (var clientId in clientIds)
        {
            PlayerType type = (PlayerType)UnityEngine.Random.Range(0, 2); // 0 for Assassin, 1 for Thief
            playerData.Add(new PlayerData
            {
                Id = clientId,
                Type = type,
                IsAlive = true
            });
            SpawnPlayer(type, clientId, FindIndexFromPregameData(clientId));
        }
    }

    private void SpawnPlayer(PlayerType playerType, ulong clientID, int playerIndex = default)
    {
        NetworkObject playerPrefab = playerType switch
        {
            PlayerType.Sniper => Sniper,
            PlayerType.Assassin => AssassinPrefabs[playerIndex],
            PlayerType.Thief => ThiefPrefabs[playerIndex],
            _ => throw new ArgumentOutOfRangeException(nameof(playerType), playerType, null),
        };
        Vector3 position = spawningPositions[UnityEngine.Random.Range(0, spawningPositions.Length)].position;

        NetworkManager.SpawnManager.InstantiateAndSpawn(playerPrefab, clientID, true, true, false, new(position.x,position.y,-0.5f), default);
    }

    private int FindIndexFromPregameData(ulong clientID)
    {
        foreach (var item in pregameData.playerSelections)
        {
            if (item.clientId == clientID)
            {
                return item.selectedPlayerModelIndex;
            }
        }
        return 0;
    }   

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            playerData.Clear();
        }
    }

    [Rpc(SendTo.Server)]
    public void AddPlayerDataRpc(PlayerType playerType, ulong id)
    {
        PlayerData data = new()
        {
            Id = id,
            IsAlive = true,
            Type = playerType,
        };
        playerData.Add(data);
    }

    public PlayerType GetPlayerTypeByID(ulong id)
    {
        foreach (var item in playerData)
        {
            if (item.Id == id)
            {
                return item.Type;
            }
        }
        return PlayerType.Assassin;
    }

    [Rpc(SendTo.Server)]
    public void SetTaskCompletionRpc(ulong id, string name)
    {
        for (int i = 0; i < playerData.Count; i++)
        {
            if (playerData[i].Id == id)
            {
                PlayerData data = playerData[i];

                data.HasWon = true;

                playerData[i] = data;

                EndGameRpc(playerData[i].Type, name);
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    public void EndGameRpc(PlayerType playerType, FixedString32Bytes name, bool win = true)
    {
        PlayerMovement[] playerMovements = FindObjectsByType<PlayerMovement>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var movement in playerMovements)
        {
            movement.enabled = false;
        }

        EndGameScreen.SetActive(true);

        if (win)
        {
            EndScreenText.text = $"{name} Won!<br> ({playerType})";

        }
        else
        {
            EndScreenText.text = $"{name}'s a murderer";
        }
        EndScreenButton.SetActive(true);
    }

    [Rpc(SendTo.Server)]
    public void SetIsAliveRpc(ulong id, bool value)
    {
        for (int i = 0; i < playerData.Count; i++)
        {
            if (playerData[i].Id == id)
            {
                PlayerData data = playerData[i];

                data.IsAlive = value;

                playerData[i] = data;

                IsEveryPlayerDead.Value = AreAllNonSniperPlayersDead();
            }
        }
    }

    public bool AreAllNonSniperPlayersDead()
    {
        foreach (var data in playerDataList)
        {
            if (data.Type != PlayerType.Sniper && data.IsAlive)
            {
                return false; // If any non-sniper player is alive, return false immediately
            }
        }
        
        return true; // If no non-sniper players are alive, return true
    }
    
    public int CountAlivePlayers()
    {
        int aliveCount = 0;

        foreach (var player in playerData)
        {
            if (player.IsAlive)
            {
                aliveCount++;
            }
        }

        return aliveCount;
    }

    private void OnPlayerDataListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        // Rebuild the entire list to ensure it stays in sync
        playerDataList.Clear();
        
        foreach (var player in playerData)
        {
            playerDataList.Add(player);
        }
    }

    private IEnumerator GameTimer()
    {
        GameTime.Value = TargetTimeInSeconds;
        while (GameTime.Value > 0)
        {
            yield return new WaitForSeconds(1f);

            if (IsServer)
            {
                GameTime.Value--;
            }
        }

        EndGameDueToTimeLimit();
        yield break;
        
    }

    public void BackToLobby()
    {
        SteamManager.currentLobby?.Leave();

        CameraShakerHandler.StopAll();

        DestroyAllDontDestroyOnLoadObjects();

        SendToLobbyRpc();
    }

    public bool IsPlayerDead(ulong id)
    {
        foreach (var item in playerDataList)
        {
            if (item.Id == id)
            {
                return item.IsAlive;
            }
        }
        return false;
    }

    [Rpc(SendTo.Everyone)]
    public void SendToLobbyRpc()
    {
        SceneManager.LoadScene("Setup", LoadSceneMode.Single);
    }

    public void DestroyAllDontDestroyOnLoadObjects() 
    {
        foreach(var root in NetworkManager.gameObject.scene.GetRootGameObjects())
        {
            if (root.name != "FirstGearGames DDOL")
            Destroy(root);
        }

    }

    public void RemoveAllNetworkObjects()
    {
        NetworkObject[] networkObjects = FindObjectsOfType<NetworkObject>(true);

        foreach (var networkObject in networkObjects)
        {
            if (networkObject != NetworkManager)
            networkObject.Despawn(true);
        }
    }

    private void EndGameDueToTimeLimit()
    {
        PlayerMovement[] playerMovements = FindObjectsByType<PlayerMovement>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var movement in playerMovements)
        {
            movement.enabled = false;
        }

        EndGameScreen.SetActive(true);

        EndScreenText.text = "DRAW TIME's UP";

        if (IsServer)
        {
            EndScreenButton.SetActive(true);
        }
        else
        {
            EndScreenButton.SetActive(false);
        }
    }
}
