
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GraveSpawner : NetworkBehaviour
{
    [SerializeField] private List<NetworkObject> _graves = new();
    [SerializeField] private List<NetworkObject> _coffins = new();
    [SerializeField] private List<Transform> _graveSpawns = new();
    [SerializeField] private List<Transform> _coffinSpawns = new();
    public void Start()
    {
        StartCoroutine(SpawnGravesAndCoffins(15, 15));
    }

    IEnumerator SpawnGravesAndCoffins(int graveAmount, int coffinAmount){
        for (int i = 0; i < _graveSpawns.Count; i++)
        {
            if (Physics.Raycast(_graveSpawns[i].transform.position, -_graveSpawns[i].transform.up, out RaycastHit hit)){
                yield return new WaitForEndOfFrame();
                int grave = Random.Range(0, _graves.Count);
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                GameObject spawnedGrave = Instantiate(_graves[grave].gameObject, hit.point, rotation);
                spawnedGrave.GetComponent<NetworkObject>().Spawn();
            }
        } 
        if (_coffinSpawns.Count != 0){
            for (int i = 0; i < _coffinSpawns.Count; i++)
            {
                if (Random.value > 0.5f)
                {
                    if (Physics.Raycast(_coffinSpawns[i].transform.position, -_coffinSpawns[i].transform.up, out RaycastHit hit)){
                        yield return new WaitForEndOfFrame();
                        int coffin = Random.Range(0, _coffins.Count);
                        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                        GameObject spawnedCoffin = Instantiate(_coffins[coffin].gameObject, hit.point, rotation);
                        spawnedCoffin.GetComponent<NetworkObject>().Spawn();
                    }
                }
            } 
        }   
    }
}
