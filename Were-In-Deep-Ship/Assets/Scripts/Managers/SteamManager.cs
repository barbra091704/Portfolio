using UnityEngine;
using Steamworks;
using TMPro;
using Steamworks.Data;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Netcode.Transports.Facepunch;
using UnityEngine.UI;

public class SteamManager : MonoBehaviour
{
    public static SteamManager Instance;
    public static Lobby? currentLobby;
    [SerializeField] private TMP_InputField maxPlayersInputField;
    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private Toggle IsLobbyPublic;

    void Awake(){
        if (Instance == null){
            Instance = this;
        }
    }
    void OnEnable(){
        SteamMatchmaking.OnLobbyCreated += LobbyCreated;
        SteamMatchmaking.OnLobbyEntered += LobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested += FriendsListJoinRequested;
    }
    void OnDisable(){
        SteamMatchmaking.OnLobbyCreated -= LobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= LobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested -= FriendsListJoinRequested;
    }

    public async void HostLobby(){
        int memberLimit = int.Parse(maxPlayersInputField.text);
        if (memberLimit > 6) memberLimit = 6; 
        await SteamMatchmaking.CreateLobbyAsync(memberLimit);
    }
    private void LobbyCreated(Result result, Lobby lobby){
        if (result == Result.OK){
            if (IsLobbyPublic.isOn) lobby.SetPublic();
            lobby.SetData("LobbyName", lobbyNameInputField.text);
            lobby.SetJoinable(true);
            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }
    }
    public async void JoinLobby(SteamId lobbyId) => await SteamMatchmaking.JoinLobbyAsync(lobbyId);

    private async void FriendsListJoinRequested(Lobby lobby, SteamId id) => await lobby.Join();

    private void LobbyEntered(Lobby lobby){
        currentLobby = lobby;
        Debug.Log($"Joined [ {lobby.Owner.Name} ]'s Lobby With ID: {lobby.Id}");

        if (NetworkManager.Singleton.IsHost) return;
         
        NetworkManager.Singleton.GetComponent<FacepunchTransport>().targetSteamId = lobby.Owner.Id;
        NetworkManager.Singleton.StartClient();
    }
    public void LeaveLobby(){
        currentLobby?.Leave();
        currentLobby = null;
        NetworkManager.Singleton.Shutdown();
    }

    //todo remove when done 
    public async void FastCreateForDev(){
        lobbyNameInputField.text = "JOSH's DEV SERVER";
        IsLobbyPublic.isOn = true;
        await SteamMatchmaking.CreateLobbyAsync(6);
    }
}
