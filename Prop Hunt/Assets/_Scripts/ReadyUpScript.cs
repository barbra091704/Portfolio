using UnityEngine;
using Photon.Pun;
using TMPro;

public class ReadyUpScript : MonoBehaviourPun
{
    public GameObject startButton;
    public TextMeshProUGUI RoomText;
    public TextMeshProUGUI PlayerText;
    public GameObject backtoLobbyButton;
    public GameObject changemapButtons;
    public GameObject roomnum;
    public int PlayerAmount;
    public static int map;
    private bool loading;
    public string mapname;
    public TextMeshProUGUI maptext;

    private void Awake()
    {
        loading = false;
        if (PhotonNetwork.IsMasterClient)
        {
            mapname = "Town";
            map = 1;
            changemapButtons.SetActive(true);
            backtoLobbyButton.SetActive(true);
            RoomText.text = CreateAndJoinRooms.RoomName;
            startButton.SetActive(true);
        }
        else
        {
            backtoLobbyButton.SetActive(true);
            roomnum.SetActive(false);
            startButton.SetActive(false);
        }
        photonView.RPC(nameof(PlayerNum), RpcTarget.All);
    }
    public void OnMapChangeButton()
    {
        if (map == 2)
        {
            map--;
            mapname = "Town";
        }
        else if (map == 1)
        {
            map++;
            mapname = "Farm";
        }
        maptext.text = mapname;
    }
    public void OnStartButtonClicked()
    {
        if (!loading)
        {
            loading = true;
            photonView.RPC(nameof(LoadLevel), RpcTarget.Others, mapname);
            Invoke(nameof(wait),1.5f);
        }
    }
    private void wait()
    {
        photonView.RPC(nameof(LoadLevel), RpcTarget.MasterClient, mapname);
    }
    [PunRPC]
    public void LoadLevel(string name)
    {
        PhotonNetwork.LoadLevel(name);
    }
    
    [PunRPC]
    public void PlayerNum()
    {
        int ListLength = PhotonNetwork.PlayerList.Length;
        PlayerText.text = "" + ListLength + " / 12";
    }

    public void OnBackToLobbyButtonClicked()
    {
        PhotonNetwork.Destroy(gameObject);
        PhotonNetwork.Disconnect();
        PhotonNetwork.LoadLevel("Loading"); 
    }
}
