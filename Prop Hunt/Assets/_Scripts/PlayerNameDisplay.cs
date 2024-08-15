using UnityEngine;
using TMPro;
using Photon.Pun;
using UnityEngine.UI;

public class PlayerNameDisplay : MonoBehaviourPun
{
    public TextMeshProUGUI nameDisplay;
    public string playerName;

    private void Start()
    {
        if (photonView.IsMine)
        {
            nameDisplay.GetComponentInParent<Image>().enabled = false;

            photonView.RPC(nameof(RPC_SyncName), RpcTarget.All, PhotonNetwork.NickName);
        }
        else
        {
            nameDisplay.text = photonView.Owner.NickName;
            playerName = photonView.Owner.NickName;
        }
    }

    [PunRPC]
    private void RPC_SyncName(string playerName)
    {
        if (!photonView.IsMine)
        {
            nameDisplay.text = playerName;
        }
    }
}
