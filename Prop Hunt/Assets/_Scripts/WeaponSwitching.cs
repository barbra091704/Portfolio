using UnityEngine;
using Photon.Pun;
public class WeaponSwitching : MonoBehaviourPun
{
    public int selectedWeapon = 0;
    Transform weapon;

    void Start()
    {     
        if (!photonView.IsMine)
            return;
        SelectWeapon();
    }
    void Update()
    {
        if (!photonView.IsMine)
            return;
        int previousselectedWeapon = selectedWeapon;

        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            if (selectedWeapon >= transform.childCount - 1)
                selectedWeapon = 0;
            else
                selectedWeapon++;
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            if (selectedWeapon <= 0)
                selectedWeapon = transform.childCount - 1;
            else
                selectedWeapon--;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedWeapon = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && transform.childCount >=2)
        {
            selectedWeapon = 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) && transform.childCount >=3)
        {
            selectedWeapon = 2;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4) && transform.childCount >=4)
        {
            selectedWeapon = 3;
        }
        if (previousselectedWeapon != selectedWeapon)
        {
            SelectWeapon();
        }

    }
    void SelectWeapon()
    {
        int i = 0;
        foreach (Transform weapon in transform)
        {
            if (i == selectedWeapon)
                photonView.RPC(nameof(WeaponSelect), RpcTarget.AllBuffered, weapon.name, true);        
            else
                photonView.RPC(nameof(WeaponSelect), RpcTarget.AllBuffered, weapon.name, false);     
            i++;
        } 
    }
    [PunRPC]
    public void WeaponSelect(string name, bool state)
    {
        Transform rpcweapon = transform.Find(name);
        rpcweapon.gameObject.SetActive(state);
    }
}