using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using TMPro;

public class InstantiatePlayers : MonoBehaviourPun
{
    public GameObject playerPrefab;
    int hunters;
    public float minX = -15;
    public float minZ = -15;
    public float maxX = 15;
    public float maxZ = 15;
    public List<int> playerViewIDs = new List<int>();
    public float TimeLeft = 620;
    public GameObject blackscreen;
    public GameObject masterUI;
    public GameObject HuntersReleased;
    public GameObject popupdeath;
    public TextMeshProUGUI deathText;
    public TextMeshProUGUI HunterAmount;
    public TextMeshProUGUI HiderAmount;
    public AudioSource audios;
    public AudioClip released;
    public bool TimerOn = false;
    public TextMeshProUGUI TimerText;
    public static int HunterCount = 0;
    public static int HiderCount = 0;

    public static bool HidersWin;

    private void Awake()
    {
        TimeLeft = 620;
        HunterCount = 0;
        HiderCount = 0;
        Invoke(nameof(TextEnable), 20f);
        blackscreen.SetActive(true);
        var randomPosition = new Vector3(Random.Range(minX, maxX), 0, Random.Range(minZ, maxZ));         
        var playerObject = PhotonNetwork.Instantiate(playerPrefab.name, randomPosition, Quaternion.identity);
        var playerView = playerObject.GetComponent<PhotonView>();
        playerObject.GetComponent<Animator>().enabled = false;

        if (playerView.IsMine)
            photonView.RPC("UpdatePlayerViewIDs", RpcTarget.All, playerView.ViewID);
    }

    public void TextDisable()
    {
        HuntersReleased.SetActive(false);
    }
    public void TextEnable()
    {
        audios.PlayOneShot(released);
        HuntersReleased.SetActive(true);
        Invoke(nameof(TextDisable), 3f);
    }
    [PunRPC]
    private void UpdatePlayerViewIDs(int viewID)
    {
        playerViewIDs.Add(viewID);
    }   
    private void Start()
    {
        TimerOn = true;
        if (PhotonNetwork.IsMasterClient)
        {
            masterUI.SetActive(true);
            Invoke(nameof(SetRoles),1f);

        }
    }
    public void Update()
    {
        if (TimerOn)
        {
            if (TimeLeft > 0)
            {
                TimeLeft -= Time.deltaTime;
                photonView.RPC(nameof(updateTimer), RpcTarget.All, TimeLeft);
            }
            else
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    Debug.Log("time is up!");
                    TimerOn = false;
                    HidersWin = true;
                    photonView.RPC(nameof(Lose), RpcTarget.All);
                    TimeLeft = 0;
                }
            }
        }
    }
    [PunRPC]
    public void Lose()
    {
        HunterCount = 0;
        HiderCount = 0;
        EnableUI.isHunter = false;
        PhotonNetwork.LoadLevel("ReadyUp");
    }
    public void SetRoles()
    {
        hunters = GetMaxHunters(playerViewIDs.Count);
        ShuffleList(playerViewIDs);
        ShuffleList(playerViewIDs);
        for (int i = 0; i < playerViewIDs.Count; i++)
        {
            PhotonView playerView = PhotonView.Find(playerViewIDs[i]);
            playerView.gameObject.tag = (i < hunters) ? "Hunter" : "Hider";
            photonView.RPC(nameof(SetSettings), RpcTarget.AllBuffered, playerView.ViewID, playerView.gameObject.tag);
            if (playerView.gameObject.tag == "Hider")
            {
                HiderCount++;
            }
        }
        photonView.RPC(nameof(UpdateHunterandHiderNum), RpcTarget.All, HunterCount, HiderCount);
    }

    [PunRPC]
    public void UpdateHunterandHiderNum(int hunterCount, int hiderCount)
    {
        HunterAmount.text = "" + hunterCount;
        HiderAmount.text = "" + hiderCount;
    }

    [PunRPC]
    public void SetSettings(int viewid, string tag)
    {
        PhotonView view = PhotonView.Find(viewid);
        view.gameObject.tag = tag;
        view.gameObject.GetComponent<EnableUI>().enabled = true;
    }

    private static void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i);
            T temp = list[i];
            list[i] = list[rnd];
            list[rnd] = temp;
        }
    }
    private int GetMaxHunters(int count)
    {
        int max;
        if (count < 5)
        {
            max = 1;
            HunterCount = 1;
        }
        else if (count >= 5 && count < 8)
        {
            max = 2;
            HunterCount = 2;
        }
        else
        {
            max = 3;
            HunterCount = 3;
        }
        return Mathf.Min(max, count);

    }


    [PunRPC]
    public void updateTimer(float currentTime)
    {
        currentTime += 1;

        float minutes = Mathf.FloorToInt(currentTime / 60);
        float seconds = Mathf.FloorToInt(currentTime % 60);

        TimerText.text = string.Format("{0:00} : {1:00}", minutes, seconds);
    }

}