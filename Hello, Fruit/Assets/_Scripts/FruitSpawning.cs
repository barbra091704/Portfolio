using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitSpawning : MonoBehaviour
{
    public Grabbing g;
    [SerializeField] private GameObject[] Fruits;
    [SerializeField] private Transform[] Spawns;
    public GameObject randomFruit;
    [SerializeField] private Transform randomSpawn;
    public GameObject currentFruit;
    public bool randomfReady;

    private void Awake()
    {
        randomfReady = true;
        RandomFruit();

    }

    public void RandomFruit()
    {
        if (g.fruitCollected < g.fruitRemaining)
        {
            if (randomfReady == true)
            {
                randomSpawn = Spawns[Random.Range(0, Spawns.Length)];
                randomFruit = Fruits[Random.Range(0, Fruits.Length)];
                currentFruit = GameObject.Instantiate(randomFruit, randomSpawn);
                Debug.Log("SPAWNED " + randomFruit + "AT: " + randomSpawn);
                g.fruitToGet = "" + randomFruit;
                randomfReady = false;
            }
        }
    }
}
