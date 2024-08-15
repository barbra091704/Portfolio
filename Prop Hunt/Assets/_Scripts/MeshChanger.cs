using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class MeshChanger : MonoBehaviourPun
{
    public GameObject body;
    Transform PropObject;
    Transform canvName;
    public LayerMask meshlayer;
    private AudioSource audiosource;
    public AudioClip changeprop;
    public AudioClip cantchange;
    string childObject;
    int prophealth;


    private void Start()
    {
        audiosource = gameObject.GetComponent<AudioSource>();
        canvName = transform.Find("Canvas");
        InvokeRepeating(nameof(PropRandom), 180, 180);
    }
    private void PropRandom()
    {
        if (!EnableUI.isHunter && photonView.IsMine)
        {
            if (PropObject != null)
            {
                PropObject.gameObject.SetActive(false);
                photonView.RPC(nameof(SyncActiveState), RpcTarget.Others, PropObject.name, false);
            }
            int rand = Random.Range(1, 90);
            string RandomObject = gameObject.transform.GetChild(4).transform.GetChild(rand).name;
            PropObject = transform.Find("PROPS").Find(RandomObject);
            canvName.gameObject.SetActive(false);
            photonView.RPC(nameof(SyncCanvas), RpcTarget.Others, false);
            body.SetActive(false);
            photonView.RPC(nameof(SyncPlayer), RpcTarget.Others, false);
            PropObject.gameObject.SetActive(true);
            photonView.RPC(nameof(SyncActiveState), RpcTarget.Others, RandomObject, true);
            audiosource.PlayOneShot(changeprop);
            Debug.Log("TOO");
        }
    }
    private void Update()
    {
        if (photonView.IsMine && !EnableUI.isHunter)
        {
                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out var hit, 10, meshlayer))
                    {
                        if (hit.transform.CompareTag("Prop"))
                        {
                            childObject = hit.transform.name;
                            if (PropObject != null)
                            {
                                if (PropObject != transform.Find("PROPS").Find(childObject))
                                {
                                    PropObject.gameObject.SetActive(false);
                                    photonView.RPC(nameof(SyncActiveState), RpcTarget.Others, PropObject.name,false);
                                }
                            }
                            PropObject = transform.Find("PROPS").Find(childObject);
                            if (!PropObject.gameObject.activeSelf)
                            {
                                audiosource.PlayOneShot(changeprop);
                                canvName.gameObject.SetActive(false);
                                photonView.RPC(nameof(SyncCanvas), RpcTarget.Others, false);
                                body.SetActive(false);
                                photonView.RPC(nameof(SyncPlayer), RpcTarget.Others, false);
                                PropObject.gameObject.SetActive(true);
                                photonView.RPC(nameof(SyncActiveState), RpcTarget.Others, childObject, true);
                            }
                            else if (PropObject.gameObject.activeSelf)
                            {
                                audiosource.PlayOneShot(cantchange);
                                Debug.Log("your already that object!");
                                return;
                            }
                        }
                            else
                            {
                                if (SceneManager.GetSceneByName("Game").isLoaded)
                                {
                                    if (EnableUI.isHunter == false)
                                    audiosource.PlayOneShot(cantchange);
                                }
                            }
                    }
                }
            
        
            if (Input.GetKeyDown(KeyCode.Y))
            {
                if (gameObject.tag != "Dead")
                {
                    canvName.gameObject.SetActive(true);
                    photonView.RPC(nameof(SyncCanvas), RpcTarget.Others, true);
                    body.SetActive(true);
                    photonView.RPC(nameof(SyncPlayer), RpcTarget.Others, true);
                    PropObject.gameObject.SetActive(false);
                    photonView.RPC(nameof(SyncActiveState), RpcTarget.Others, PropObject.name, false);
                }
            }
        }
    }

    [PunRPC]
    private void SyncActiveState(string childObject, bool active)
    {
        Transform obj = transform.Find("PROPS").Find(childObject);
        obj.gameObject.SetActive(active);
    }
    [PunRPC]
    private void SyncPlayer(bool state)
    {
        body.SetActive(state);
    }
    [PunRPC]
    private void SyncCanvas(bool state)
    {
        canvName.gameObject.SetActive(state);
    }
}
