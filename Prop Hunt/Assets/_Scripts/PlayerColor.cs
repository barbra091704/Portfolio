using UnityEngine;
using System.Collections;
using Photon.Pun;

public class PlayerColor : MonoBehaviourPunCallbacks
{
    public static Color[] playerColors = { Color.white, Color.cyan, Color.green, Color.black, Color.yellow, Color.blue, Color.grey, Color.magenta, Color.red, Color.white, Color.cyan, Color.green, Color.black, Color.yellow, Color.blue, Color.grey};
    public Color MyColor;
    public Renderer playerRenderer;
    public int playerColorIndex;

    void Start()
    {
        if (photonView.IsMine)
        {
            MyColor = playerColors[playerColorIndex];
            playerColorIndex = PhotonNetwork.LocalPlayer.ActorNumber % playerColors.Length - 1;
            playerRenderer.material.color = playerColors[playerColorIndex];

            photonView.RPC("UpdatePlayerColor", RpcTarget.OthersBuffered, playerColorIndex);
        }
    }

    [PunRPC]
    void UpdatePlayerColor(int colorIndex)
    {
        playerRenderer.material.color = playerColors[colorIndex];
    }
}
