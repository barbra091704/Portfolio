using UnityEngine;
using Steamworks;
using Steamworks.Data;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using UnityEngine.UI;

public class SteamManager : MonoBehaviour
{
    public enum Mode
    {
        OnevOne,
        Classic,
    }

    public static SteamManager Instance;
    public static Lobby? currentLobby;
    [SerializeField] private Toggle IsLobbyPrivate;
    private Lobby[] lobbies;

    public Mode mode;

    private void Start()
    {
        SearchForLobbies();
        mode = Mode.Classic;
        DontDestroyOnLoad(this);
    }

    public async void SearchForLobbies()
    {
        // Clear current lobbies array
        lobbies = new Lobby[0];
        
        // Wait for a second before searching (to ensure lobby data is set)
        await System.Threading.Tasks.Task.Delay(1000);

        lobbies = await SteamMatchmaking.LobbyList.WithSlotsAvailable(1).RequestAsync();
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void OnEnable()
    {
        SteamMatchmaking.OnLobbyCreated += LobbyCreated;
        SteamMatchmaking.OnLobbyEntered += LobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested += FriendsListJoinRequested;
    }

    void OnDisable()
    {
        SteamMatchmaking.OnLobbyCreated -= LobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= LobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested -= FriendsListJoinRequested;
    }

    public async void HostLobby()
    {
        if (mode == Mode.OnevOne)
        {
            await SteamMatchmaking.CreateLobbyAsync(2);
        }
        else if (mode == Mode.Classic)
        {
            await SteamMatchmaking.CreateLobbyAsync(5);
        }
    }

    private void LobbyCreated(Result result, Lobby lobby)
    {
        if (result == Result.OK)
        {
            if (IsLobbyPrivate.isOn)
            {
                lobby.SetPrivate();
                lobby.SetData("TinySniperLobbyName", $"{lobby.Id}");
                print($"Set Lobby data to {lobby.Id}");
            }
            else
            {
                lobby.SetPublic();
                lobby.SetData("TinySniperLobbyName", "Quickplay");
                print($"Set Lobby data to Quickplay");
            }

            lobby.SetData("TinySniperMode", $"{mode}");
            lobby.SetJoinable(true);

            NetworkManager.Singleton.StartHost();

            NetworkManager.Singleton.SceneManager.LoadScene("Pregame", LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError("Lobby creation failed: " + result);
        }
    }

    public async void JoinLobby(SteamId lobbyId)
    {
        await SteamMatchmaking.JoinLobbyAsync(lobbyId);
    }

    private async void FriendsListJoinRequested(Lobby lobby, SteamId id)
    {
        await lobby.Join();
    }

    private void LobbyEntered(Lobby lobby)
    {
        currentLobby = lobby;
        Debug.Log($"Joined [ {lobby.Owner.Name} ]'s Lobby With ID: {lobby.Id}");

        if (NetworkManager.Singleton.IsHost) return;

        NetworkManager.Singleton.transform.GetComponent<Netcode.Transports.Facepunch.FacepunchTransport>().targetSteamId = lobby.Owner.Id;
        NetworkManager.Singleton.StartClient();
    }

    public void LeaveLobby()
    {
        currentLobby?.Leave();
        currentLobby = null;
        NetworkManager.Singleton.Shutdown();
    }

    public void SetMode(int i)
    {
        if (i == 0)
        {
            mode = Mode.Classic;
        }
        else
        {
            mode = Mode.OnevOne;
        }
    }

    public void QuickPlay()
    {
        foreach (var lobby in lobbies)
        {
            string lobbyname = lobby.GetData("TinySniperLobbyName");
            if (lobbyname == "Quickplay" && lobby.MemberCount < 5)
            {
                JoinLobby(lobby.Id);
                break;  // Exit after joining a suitable lobby
            }
        }
    }
}
