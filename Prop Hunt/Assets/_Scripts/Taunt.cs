using Photon.Pun;
using UnityEngine;
using System.Collections;
public class Taunt : MonoBehaviourPun
{
    public AudioClip[] taunts;
    private bool tauntready;

    private void Start()
    {
        tauntready = true;
        if (EnableUI.isHunter == false && photonView.IsMine) 
        {
            InvokeRepeating(nameof(RandomTaunt), 30, 30);
        }
    }
    public void RandomTaunt()
    {
        photonView.RPC(nameof(GunRPCs.TauntPlayer), RpcTarget.All, photonView.ViewID, 2);
    }
    public void Update()
    { 
        if (EnableUI.isHunter == false && photonView.IsMine && tauntready)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                tauntready = false;
                photonView.RPC(nameof(GunRPCs.TauntPlayer), RpcTarget.All, photonView.ViewID, 0);
                Invoke(nameof(resettaunt), 1f);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                tauntready = false;
                photonView.RPC(nameof(GunRPCs.TauntPlayer), RpcTarget.All, photonView.ViewID, 1);
                Invoke(nameof(resettaunt), 1f);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                tauntready = false;
                photonView.RPC(nameof(GunRPCs.TauntPlayer), RpcTarget.All, photonView.ViewID, 2);
                Invoke(nameof(resettaunt), 1f);
            }
        }
    }
    private void resettaunt()
    {
        tauntready = true;
    }
}