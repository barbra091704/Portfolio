using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using TMPro;


public class ConnectToServer : MonoBehaviourPunCallbacks
{
    public static bool fs;
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        SceneManager.LoadScene("Lobby");
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F11))
        {
            fs = !fs;
            Screen.fullScreen = fs;
        }
    }
}
