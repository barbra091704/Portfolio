using Photon.Pun;
using UnityEngine;
public class GunRPCs : MonoBehaviourPun
{
    public GameObject impactEffect;
    public TrailRenderer trailrenderer;
    public InstantiatePlayers instP;

    [PunRPC]
    public void rpcImpactEffect(Vector3 hit)
    {
        GameObject impactGO = Instantiate(impactEffect, hit, Quaternion.LookRotation(Camera.main.transform.position));
        destroyImpact(impactGO);
    }
    void destroyImpact(GameObject impactGO)
    {
        Destroy(impactGO, 0.5f);
    }

    [PunRPC]
    public void rpcWeaponDamage(int viewid, float damage)
    {
        PhotonView view = PhotonView.Find(viewid);
        Target targ = view.transform.GetComponent<Target>();
        targ.TakeDamage(damage);
    }
    [PunRPC]
    public void rpcMuzzleFlash(int viewid)
    {
        PhotonView view = PhotonView.Find(viewid);
        view.gameObject.GetComponentInChildren<Gun>().muzzleFlash.Play();
    }
    [PunRPC]
    public void rpcTrail(Vector3 barrelpos, Vector3 fpscamforward, float range)
    {
        var bullet = Instantiate(trailrenderer, barrelpos, Quaternion.identity);
        bullet.AddPosition(barrelpos);
        {
            bullet.transform.position = transform.position + (fpscamforward * range);
            destroyTrail(bullet);
        }
    }
    void destroyTrail(TrailRenderer bullet)
    {
        Destroy(bullet, 0.5f);
    }
    [PunRPC]
    public void PlayShootSound(int viewid)
    {
        PhotonView view = PhotonView.Find(viewid);
        AudioClip clip = view.gameObject.transform.GetChild(7).GetComponentInChildren<Gun>().gunShoot;
        view.gameObject.GetComponent<AudioSource>().PlayOneShot(clip);
    }
    [PunRPC]
    public void TauntPlayer(int viewid, int num)
    {
        PhotonView view = PhotonView.Find(viewid);
        AudioClip clip = view.gameObject.GetComponent<Taunt>().taunts[num];
        view.gameObject.GetComponent<AudioSource>().PlayOneShot(clip);
    }

    [PunRPC]
    public void PlayerDeath(int viewid)
    {
        PhotonView view = PhotonView.Find(viewid);
        GameObject.Find("Game Manager").GetComponent<InstantiatePlayers>().popupdeath.SetActive(true);
        Invoke(nameof(ResetPlayerDeath), 5f);
        string name = view.gameObject.GetComponent<PlayerNameDisplay>().playerName;
        GameObject.Find("Game Manager").GetComponent<InstantiatePlayers>().deathText.text = name;
    }
    public void ResetPlayerDeath()
    {
        GameObject.Find("Game Manager").GetComponent<InstantiatePlayers>().popupdeath.SetActive(false);
    }

}
 
