using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AmmoCount : MonoBehaviour
{
    public Gun gun;
    public Gun gun2;
    public Gun gun3;
    public Gun gun4;
    public Image one;
    public Image two;
    public Image three;
    public Image four;
    [SerializeField] TextMeshProUGUI ammoText;
    [SerializeField] TextMeshProUGUI ammoReserve;
    public WeaponSwitching ws;

    void Update()
    {
        if (ws.selectedWeapon == 0)
        {
            ammoText.text = gun.currentAmmo + " / " + gun.maxAmmo;
            ammoReserve.text = "" + gun.ammoReserve; 
            one.enabled = true;
            two.enabled = false;
            three.enabled = false;
            four.enabled = false;

        }
        if (ws.selectedWeapon == 1)
        {
            ammoText.text = gun2.currentAmmo + " / " + gun2.maxAmmo;
            ammoReserve.text = "" + gun2.ammoReserve;
            two.enabled = true;
            one.enabled = false;
            three.enabled = false;
            four.enabled = false;
        }
        if (ws.selectedWeapon == 2)
        {
            ammoText.text = gun3.currentAmmo + " / " + gun3.maxAmmo;
            ammoReserve.text = "" + gun3.ammoReserve;
            three.enabled = true;
            one.enabled = false;
            two.enabled = false;
            four.enabled = false;
        }
        if (ws.selectedWeapon == 3)
        {
            ammoText.text = gun4.currentAmmo + " / " + gun4.maxAmmo;
            ammoReserve.text = "" + gun4.ammoReserve;
            four.enabled = true;
            one.enabled = false;
            three.enabled = false;
            two.enabled = false;
        }
    }   
}
