
using UnityEngine;
using TMPro;
using Photon.Pun;



public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    public TMP_InputField createInput;
    public TMP_InputField joinInput;
    public TMP_InputField inputname;
    public static string RoomName;

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(createInput.text);
    }
    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinInput.text);
    }

    public override void OnJoinedRoom()
    {
        RoomName = createInput.text;
        SetName();
        PhotonNetwork.LoadLevel("ReadyUp");
    }
    public void SetName()
    {
        if (inputname.text != "")
        {
            PhotonNetwork.NickName = inputname.text;
        }
        else
        {
            PhotonNetwork.NickName = "anonymous";
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F11))
        {
            ConnectToServer.fs = !ConnectToServer.fs;
            Screen.fullScreen = ConnectToServer.fs;
        }
    }
}
