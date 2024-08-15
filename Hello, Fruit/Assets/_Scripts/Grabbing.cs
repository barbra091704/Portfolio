using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Grabbing : MonoBehaviour
{
    public MoneySpawner MS;
    public Monster M;
    public FruitSpawning fs;
    public SHoppingCart sc;
    public Timer T;
    public GameObject Lights;
    public GameObject heart1;
    public GameObject heart2;
    public Text moneyText;
    [SerializeField] private GameObject player;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource audioSourceThud;
    public Transform startingPosition;
    private Vector3 startingPos;
    public int fruitCollected;
    public int fruitRemaining;
    public string fruitToGet;
    public int Money;


    private void Start()
    {
        if (M._health == 1)
        {
            heart1.gameObject.SetActive(true);
            heart2.gameObject.SetActive(false);
        }
        if (M._health == 2)
        {
            heart2.gameObject.SetActive(true);
            heart1.gameObject.SetActive(true);
        }
    }
    private void Awake()
    {
        moneyText.text = "$0";
        startingPos = startingPosition.position;
        fruitCollected = 0;
        T.timerOn = true;
        audioSourceThud.Play();
        player.transform.position = startingPos;

        if (Difficulty._Difficulty == 0)
        {
            fruitRemaining = 3;
            M.sightRange = 17;
            sc.sprintTime = 10;
            M.difficultySpeed = 3;
            sc.sprintRecovery = 3;
            M._health = 2;
            M.difficultySpeedRun = 6f;
            T.timeLeft = 1200;
        }
        if (Difficulty._Difficulty == 1)
        {
            fruitRemaining = 6;
            M.sightRange = 21;
            M.difficultySpeedRun = 6.5f;
            M.difficultySpeed = 4;
            sc.sprintRecovery = 4;
            sc.sprintTime = 8;
            M._health = 2;
            T.timeLeft = 600;
        }
        if (Difficulty._Difficulty == 2)
        {
            fruitRemaining = 8;
            T.timeLeft = 400;
            M.difficultySpeed = 5;
            sc.sprintTime = 5;
            sc.sprintRecovery = 5;
            M.difficultySpeedRun = 7.5f;
            M.sightRange = 26;
            M._health = 2;
        }
        if (Difficulty._Difficulty == 3)
        {
            fruitRemaining = 10;
            T.timeLeft = 300;
            M.difficultySpeed = 6;
            sc.sprintTime = 4;
            M._health = 1;
            sc.sprintRecovery = 6;
            M.difficultySpeedRun = 8;
            M.sightRange = 30;
        }
        if (Difficulty._Difficulty == 4)
        {
            fruitRemaining = 12;
            T.timeLeft = 250;
            sc.sprintTime = 3;
            M._health = 1;
            sc.sprintRecovery = 7;
            M.difficultySpeed = 6.5f;
            M.difficultySpeedRun = 8.5f;
            M.sightRange = 35;
        }
        if (M._health == 1)
        {
            heart1.gameObject.SetActive(true);
            heart2.gameObject.SetActive(false);
        }
        if (M._health == 2)
        {
            heart1.gameObject.SetActive(true);
            heart2.gameObject.SetActive(true);
        }

    }
    void Update()
    {
        if (fruitCollected == fruitRemaining)
        {
            M.monsterObject.gameObject.SetActive(false);
            Lights.gameObject.SetActive(true);
        }
      
       
    }
    void OnTriggerStay(Collider other)
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            switch (other.tag)
            {
                case "Fruit":
                    Destroy(fs.currentFruit);
                    fruitCollected++;
                    Money = Money + 100;
                    moneyText.text = "$" + Money;
                    fs.randomfReady = true;
                    fs.RandomFruit();
                    audioSource.Play();
                    break;
                case "Money":
                    Destroy(MS.SpawnedMoney);
                    MS.amountSpawned++;
                    Money = Money + 10;
                    moneyText.text = "$" + Money;
                    fs.randomfReady = true;
                    MS.SpawnRandomMoney();
                    audioSource.Play();
                    break;
            }
            
        }
    }
}

