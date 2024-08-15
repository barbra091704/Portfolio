using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class Target : MonoBehaviourPun
{

    public float maxHealth = 100;
    public float currentHealth;
    public Transform canvas;
    public GameObject body;
    public GameObject camFollow;
    public GameObject healthbar;
    private Slider slider;
    EnableUI eui;
    private int hidnum;
    private int hunnum;

    void Start()
    {
        eui = gameObject.GetComponent<EnableUI>();
        slider = healthbar.GetComponentInChildren<Slider>();
        currentHealth = maxHealth;
        slider.value = currentHealth;

    }
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0f)
        {
            if (EnableUI.isHunter == true)
            {
                currentHealth += 25;
            }
            Die();
        }
        if (photonView.IsMine)
        {
            slider.value = currentHealth;
        }

    }
    public void Die()
    {
        GameObject.Find("Game Manager").GetComponent<InstantiatePlayers>().TimeLeft += 30;
        camFollow.SetActive(false);
        body.gameObject.SetActive(false);
        canvas.gameObject.SetActive(false);
        gameObject.transform.GetChild(4).gameObject.SetActive(false);
        gameObject.GetComponent<PlayerColor>().enabled = false;
        gameObject.GetComponent<GunRPCs>().enabled = false;
        gameObject.GetComponent<PlayerNameDisplay>().enabled = false;
        gameObject.GetComponent<Taunt>().enabled = false;
        healthbar.SetActive(false);
        gameObject.transform.position = new Vector3(transform.position.x, 23, transform.position.z);
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonView view = GameObject.Find("Game Manager").GetComponent<PhotonView>();
            if (gameObject.CompareTag("Hider"))
            {
                if (InstantiatePlayers.HiderCount > 0)
                {
                    InstantiatePlayers.HiderCount--;
                }
            }
            else
            {
                if (InstantiatePlayers.HunterCount > 0)
                {
                    InstantiatePlayers.HunterCount--;
                }
            }
            if (InstantiatePlayers.HiderCount <= 0 || InstantiatePlayers.HunterCount <= 0)
            {
                view.RPC(nameof(InstantiatePlayers.Lose), RpcTarget.AllBuffered);
            }
            view.RPC(nameof(InstantiatePlayers.UpdateHunterandHiderNum), RpcTarget.All, InstantiatePlayers.HunterCount, InstantiatePlayers.HiderCount);
        }
        photonView.RPC(nameof(GunRPCs.PlayerDeath), RpcTarget.All, photonView.ViewID);
    }
}