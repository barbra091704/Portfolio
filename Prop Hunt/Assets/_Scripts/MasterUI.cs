using Photon.Pun;
using UnityEngine;
public class MasterUI : MonoBehaviourPun
{
    public PhotonView view;
    PhotonView iview;


    public void Start()
    {
        iview = GameObject.Find("Game Manager").gameObject.GetComponent<PhotonView>();
        view = PhotonView.Find(1003);
    }
    public void OnBackToLobbyButtonClicked()
    {
        iview.RPC(nameof(InstantiatePlayers.Lose), RpcTarget.AllBuffered);
    }
    public void Update()
    {
        if (view.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.F8))
            {
                iview.RPC(nameof(InstantiatePlayers.Lose), RpcTarget.AllBuffered);
            }
        }
    }
}
