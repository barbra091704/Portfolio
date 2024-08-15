using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneySpawner : MonoBehaviour
{
    public Transform[] moneySpawns;
    public GameObject Bill;
    public Transform moneyS;
    public int amountSpawned = 0;
    private bool isrunning;
    public GameObject SpawnedMoney;

    private void Start()
    {
        isrunning = false;
        SpawnRandomMoney();
    }
    public void SpawnRandomMoney()
    {
        if (amountSpawned < 25)
        {
            if (isrunning == false)
            isrunning = true;
            amountSpawned++;
            moneyS = moneySpawns[Random.Range(0, moneySpawns.Length)];
            SpawnedMoney = GameObject.Instantiate(Bill, moneyS);
            isrunning = false;

        }
    }
}