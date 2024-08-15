using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class PregameManager : NetworkBehaviour
{
    public int neededPlayers;
    public NetworkVariable<int> currentPlayerCount = new(0);
    public PregameData pregameData;
    public GameObject[] litSprites;
    public GameObject[] playerModels;
    public GameObject[] mapModels;
    public GameObject playButtonVisual;
    public GameObject playButton;
    public int selectedPlayerModel;
    public int selectedMap;
    public bool canPlay = false;
    public bool isTesting;


    public override void OnNetworkSpawn()
    {
        currentPlayerCount.OnValueChanged += OnPlayerCountChanged;

        if (IsServer)
        {
            currentPlayerCount.Value++;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedRpc;
            UpdateLitSprites();
        }
    }

    private void UpdateLitSprites()
    {
        for (int i = 0; i < litSprites.Length; i++)
        {
            litSprites[i].SetActive(i < currentPlayerCount.Value);
        }
    }

    [Rpc(SendTo.Server)]
    private void OnClientConnectedRpc(ulong clientId)
    {
        currentPlayerCount.Value++;
    }

    private void OnPlayerCountChanged(int oldCount, int newCount)
    {
        UpdateLitSprites();

        if (IsServer)
        {
            if (isTesting)
            {
                canPlay = true;
                playButtonVisual.SetActive(true);
                playButton.SetActive(true);
                return;
            }
            else if (newCount >= neededPlayers)
            {
                canPlay = true;
                playButtonVisual.SetActive(true);
                playButton.SetActive(true);
            }
            else
            {
                canPlay = false;
                playButtonVisual.SetActive(false);
                playButton.SetActive(false);
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        currentPlayerCount.OnValueChanged -= OnPlayerCountChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedRpc;
        }
    }

    public void Play()
    {   
        int amount = isTesting ? 0 : neededPlayers;
        if (canPlay && currentPlayerCount.Value >= amount)
        {
            pregameData.SetToDestroyOnLoad();
            if (SteamManager.Instance.mode == SteamManager.Mode.OnevOne)
            {
                NetworkManager.Singleton.SceneManager.LoadScene("1v1", LoadSceneMode.Single);
            }
            else if (SteamManager.Instance.mode == SteamManager.Mode.Classic)
            {
                NetworkManager.Singleton.SceneManager.LoadScene("Classic", LoadSceneMode.Single);
            }
        }
    }

    public void SelectPlayerModel()
    {
        if (playerModels[selectedPlayerModel] != null)
        {
            playerModels[selectedPlayerModel].SetActive(false);
        }

        selectedPlayerModel++;
        if (selectedPlayerModel >= playerModels.Length)
        {
            selectedPlayerModel = 0;
        }

        playerModels[selectedPlayerModel].SetActive(true);

        // Update the player selection in the network list
        UpdatePlayerSelectionServerRpc(NetworkManager.Singleton.LocalClientId, selectedPlayerModel);
    }
    
    public void SelectMap()
    {
        if (!IsServer) return;

        SelectMapRpc();
    }

    [Rpc(SendTo.Everyone)]
    public void SelectMapRpc()
    {
        if (mapModels[selectedMap] != null)
        {
            mapModels[selectedMap].SetActive(false);
        }

        selectedMap++;
        if (selectedMap >= mapModels.Length)
        {
            selectedMap = 0;
        }

        mapModels[selectedMap].SetActive(true);

        if (IsServer)
        {
            pregameData.mapSelection.Value = selectedMap;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePlayerSelectionServerRpc(ulong clientId, int selectedModelIndex)
    {
        bool playerExists = false;

        // Check if the player already exists in the list
        for (int i = 0; i < pregameData.playerSelections.Count; i++)
        {
            if (pregameData.playerSelections[i].clientId == clientId)
            {
                pregameData.playerSelections[i] = new PlayerSelection(clientId, selectedModelIndex);
                playerExists = true;
                break;
            }
        }

        // If the player doesn't exist, add them to the list
        if (!playerExists)
        {
            pregameData.playerSelections.Add(new PlayerSelection(clientId, selectedModelIndex));
        }
    }
}
