using System.Collections;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
public class Gun : MonoBehaviourPun
{
    // Start is called before the first frame update
    public float damage;
    public float range;
    public float impactForce;
    public float fireRate;
    public int ammoReserve;
    public float recoilamount;
    public float Spread;

    public int maxAmmo = 15;
    public int currentAmmo;
    public float reloadTime = 2f;
    private bool isReloading = false;

    private Camera fpsCam;
    public ParticleSystem muzzleFlash;
    public Transform gunBarrell;
    public TrailRenderer bulletTrail;

    private float nextTimeToFire = 0f;
    private PhotonView view;
    public AudioSource audioSource;
    public AudioClip gunShoot;
    public AudioClip gunReload;
    public AudioClip hitmarker;
    public Image crosshair;
    public Image crosshair2;
    public GameObject cameraFollow;



    void Start()
    {

        view = gameObject.transform.root.GetComponent<PhotonView>();
        fpsCam = Camera.main;
        currentAmmo = maxAmmo;
    }

    void OnEnable()
    {
        isReloading = false;
      //  animator.SetBool("Reloading", false);
    }
    // reload button
    void Update()
    {
        if (!view.IsMine)
            return;
        if (isReloading)
            return;
        //fire button
        if (Input.GetMouseButton(0) && Time.time >= nextTimeToFire && currentAmmo > 0 && EnableUI.isreleased == true)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            audioSource.pitch = Random.Range(0.9f, 1.010f);
            Shoot();
        }
            if (currentAmmo < maxAmmo)
        {
            if (Input.GetKeyDown("r"))
                StartCoroutine(Reload());
            return;
        }

    }
    // reload function
    IEnumerator Reload()
    {
        if (ammoReserve > 0)
        {
            isReloading = true;
            Debug.Log("Reloading...");
            audioSource.PlayOneShot(gunReload);
            // animator.SetBool("Reloading", true);
            yield return new WaitForSeconds(reloadTime - .25f);
            // animator.SetBool("Reloading", false);
            yield return new WaitForSeconds(.25f);
            int ammoNeeded = maxAmmo - currentAmmo;
            if (ammoReserve >= ammoNeeded)
            {
                currentAmmo = maxAmmo;
                ammoReserve -= ammoNeeded;
            }
            else    
            {
                currentAmmo += ammoReserve;
                ammoReserve = 0;
            }
            isReloading = false;
        }
        else
            Debug.Log("Out of Ammo!");

    }
    //shoot function
    void Shoot()
    {
        currentAmmo--;
        muzzleFlash.Play();
        audioSource.PlayOneShot(gunShoot);
        Vector3 pos = new Vector3(cameraFollow.transform.localPosition.x, cameraFollow.transform.localPosition.y, cameraFollow.transform.localPosition.z -recoilamount);
        cameraFollow.transform.localPosition = Vector3.Lerp(cameraFollow.transform.localPosition, pos, 0.1f);
        //rpc's of gun attributes
        Vector3 posRecoil = new Vector3(Camera.main.transform.localPosition.x, Camera.main.transform.localPosition.y + recoilamount, Camera.main.transform.localPosition.z);
        Camera.main.transform.localPosition = Vector3.Lerp(Camera.main.transform.localPosition, posRecoil, 0.1f);
        view.RPC(nameof(GunRPCs.PlayShootSound), RpcTarget.Others, view.ViewID);
        view.RPC(nameof(GunRPCs.rpcMuzzleFlash), RpcTarget.Others, view.ViewID);
        float randomAngle = Random.Range(-Spread, Spread);
        Vector3 randomDirection = Random.onUnitSphere;
        var rotation = Quaternion.AngleAxis(randomAngle, randomDirection);
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, rotation * fpsCam.transform.forward, out hit, range))
        {
            view.RPC(nameof(GunRPCs.rpcImpactEffect), RpcTarget.All, hit.point);
            view.RPC(nameof(GunRPCs.rpcTrail), RpcTarget.All, gunBarrell.position, fpsCam.transform.forward, range);
            Target target = hit.collider.gameObject.transform.root.GetComponent<Target>();
            if (target != null)
            {
                if (target.CompareTag("Hider"))
                {
                    crosshair.color = Color.red;
                    crosshair2.color = Color.red;
                    Invoke(nameof(crosshaircolor), 0.5f);
                    audioSource.PlayOneShot(hitmarker);
                    int targetview = hit.transform.root.GetComponent<PhotonView>().ViewID;
                    view.RPC(nameof(GunRPCs.rpcWeaponDamage), RpcTarget.All, targetview, damage);
                }
            }
            else if (target == null && hit.transform.CompareTag("Prop"))
            {
                view.RPC(nameof(GunRPCs.rpcWeaponDamage), RpcTarget.All, view.ViewID, 1f);
            }
            else
            {
                Debug.Log(hit.transform.name);
            }
        }
    }

    void crosshaircolor()
    {
        crosshair.color = Color.white;
        crosshair2.color = Color.white;
    }
}
