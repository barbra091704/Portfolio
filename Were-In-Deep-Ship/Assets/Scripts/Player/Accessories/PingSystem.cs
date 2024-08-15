using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PingSystem : NetworkBehaviour
{
    public Transform lookAt;
    public NetworkObject PingPrefab;
    private Ping spawnedPing;

    [Rpc(SendTo.Server)]
    public void PingRpc(Vector3 position, string name, RpcParams rpcParams = default)
    {   
        NetworkObject playerObj = NetworkManager.Singleton.ConnectedClients[rpcParams.Receive.SenderClientId].PlayerObject;
        PingSystem pingSystem = playerObj.GetComponent<PingSystem>();

        if (pingSystem.spawnedPing != null && pingSystem.spawnedPing.GetComponent<NetworkObject>().IsSpawned)
        {
            pingSystem.spawnedPing.GetComponent<NetworkObject>().Despawn(true);
        }
        NetworkObject pingObj = Instantiate(PingPrefab, position, Quaternion.identity);
        pingObj.GetComponent<NetworkObject>().Spawn();

        pingSystem.spawnedPing = pingObj.GetComponent<Ping>();
        
        pingSystem.spawnedPing.name = $"{name}'s Ping";

        PingClientRpc(spawnedPing.GetComponent<NetworkObject>(), playerObj, name);
    }
    [Rpc(SendTo.NotServer)]
    public void PingClientRpc(NetworkObjectReference reference, NetworkObjectReference playerReference, string name)
    {
        if (reference.TryGet(out NetworkObject networkObject))
        {
            if (playerReference.TryGet(out NetworkObject playerObject))
            {
                PingSystem pingSystem = playerObject.GetComponent<PingSystem>();

                pingSystem.spawnedPing = networkObject.GetComponent<Ping>();

                pingSystem.spawnedPing.name = $"{name}'s Ping";
            }
        }
    }

    public void Start()
    {
        PlayerCamera[] networkObjects = FindObjectsByType<PlayerCamera>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (var item in networkObjects)
        {
            if (item.NetworkObject.IsLocalPlayer)
            {
                lookAt = item.camHolder;
            }
        }
    }

    public void FixedUpdate()
    {
        if (spawnedPing == null) return;
        RotatePingTowardsCamera();
    }
    private void RotatePingTowardsCamera()
    {
        // Calculate the distance between the ping object and the camera
        float distanceToCamera = Vector3.Distance(lookAt.position, spawnedPing.transform.position);

        // Calculate the scale factor based on distance (adjust the scale factor as needed)
        float scaleFactor = 0.2f * Mathf.Max(distanceToCamera, 1f); // Prevent division by zero

        // Set the ping object's scale based on the scaleFactor
        spawnedPing.transform.localScale = Vector3.one * scaleFactor;

        // Rotate the ping object towards the camera
        spawnedPing.transform.LookAt(lookAt.position);

        // Update the distance text (optional)
        spawnedPing.distance.text = $"{(int)distanceToCamera}m";
    }

}
