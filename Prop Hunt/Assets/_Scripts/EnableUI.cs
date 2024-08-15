using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;

public class EnableUI : MonoBehaviourPun
{
    public GameObject hunterUI;
    public GameObject hiderUI;
    private SkinnedMeshRenderer prenderer;
    public GameObject eyes;
    public GameObject mouth;
    public Transform shoulder;
    public Transform wrist;
    public Transform Wh;
    public Animator anim;
    public GameObject GunUI;
    public Transform hunterspawnpos;
    [SerializeField]
    private float xmin;
    [SerializeField]
    private float xmax;
    [SerializeField]
    private float zmin;
    [SerializeField]
    private float zmax;
    PhotonView view;
    InstantiatePlayers InsP;
    public static bool isHunter = false;
    public static bool isreleased = false;

    private void Start()
    {
        if (!photonView.IsMine)
            return;
        gameObject.GetComponent<Target>().enabled = true;
        hunterspawnpos = GameObject.Find("HunterSpawnPoint").transform;
        gameObject.GetComponent<Target>().healthbar.SetActive(true);
        gameObject.GetComponent<PlayerController>().enabled = false;
        prenderer = gameObject.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>();
        if (gameObject.tag == "Hunter")
        {
            isreleased = false;
            GunUI.SetActive(true);
            hunterUI.SetActive(true);
            prenderer.material.color = Color.red;
            Wh.gameObject.SetActive(true);
            shoulder.localRotation = Quaternion.Euler(0, -75, -50);
            Wh.localPosition = new Vector3(0.77f, 2, 0);
            wrist.parent = Wh;
            gameObject.transform.GetChild(0).transform.localPosition = new Vector3(0, 2.80f, 0.7f);
            eyes.gameObject.SetActive(false);
            mouth.gameObject.SetActive(false);
            photonView.RPC(nameof(UpdateHunterColor), RpcTarget.OthersBuffered, photonView.ViewID);
            photonView.RPC(nameof(UpdateHunter), RpcTarget.OthersBuffered, photonView.ViewID);
            gameObject.transform.position = new Vector3(hunterspawnpos.position.x, gameObject.transform.position.y, hunterspawnpos.position.z);
            isHunter = true;
            Invoke(nameof(ReleaseHunters), 19f);
        }
        if (gameObject.tag == "Hider")
        {
            gameObject.GetComponent<Taunt>().enabled = true;
            gameObject.GetComponent<PlayerController>().sprintSpeed = 4;
            GameObject.Find("BLACKSCREEN").gameObject.SetActive(false);
            hiderUI.SetActive(true);
            Invoke("DisableHiderUI", 3f);
            isHunter = false;
        }
    }
    private void ReleaseHunters()
    {
        GameObject.Find("BLACKSCREEN").gameObject.SetActive(false);
        gameObject.GetComponent<PlayerController>().enabled = true;
        isreleased = true;
        hunterUI.SetActive(false);
        anim.enabled = true;
    }
    private void DisableHiderUI()
    {
        hiderUI.SetActive(false);
        anim.enabled = true;
        gameObject.GetComponent<PlayerController>().enabled = true;
    }

    [PunRPC]
    void UpdateHunterColor(int viewid)
    {
        PhotonView view = PhotonView.Find(viewid);
        view.gameObject.transform.GetChild(1).GetComponent<Renderer>().material.color = Color.red;
    }

    [PunRPC]
    void UpdateHunter(int pview)
    {
        PhotonView view = PhotonView.Find(pview);
        EnableUI viewScript = view.gameObject.GetComponent<EnableUI>();
        viewScript.Wh.gameObject.SetActive(true);
        viewScript.shoulder.localRotation = Quaternion.Euler(0, -75, -50);
        viewScript.Wh.localPosition = new Vector3(0.77f, 2, 0);
        viewScript.wrist.parent = Wh;
        view.gameObject.transform.GetChild(0).transform.localPosition = new Vector3(0, 2.80f, 0.7f);
    }
}
