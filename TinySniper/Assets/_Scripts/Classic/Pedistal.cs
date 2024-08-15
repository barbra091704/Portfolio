using Unity.Netcode;
using UnityEngine;

public class Pedistal : NetworkBehaviour
{
    public Transform pedistalTop;
    public NetworkObject[] valueables;
    public NetworkObject spawnedValuable;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        if (spawnedValuable == null)
        {
            spawnedValuable = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(valueables[Random.Range(0, valueables.Length)], 0, true, false, false, pedistalTop.position, pedistalTop.rotation);
        }
    }
}
