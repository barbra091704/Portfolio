using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject PlayerObj;
    [SerializeField] private Vector3 SpawnPoint;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        
        NetworkObject player = Instantiate(PlayerObj, SpawnPoint, Quaternion.identity);
        player.SpawnAsPlayerObject(NetworkManager.LocalClientId);
        GameManager.Singleton.AddPlayerDataRpc(new(Steamworks.SteamClient.Name), NetworkManager.LocalClientId, true, player);
        
        NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;
        
    }
    public override void OnDestroy()
    {
        if (NetworkManager.Singleton != null && IsServer && SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Game"))
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= SpawnPlayer;
        }
    }
    private void SpawnPlayer(ulong clientId)
    {
        NetworkObject player = Instantiate(PlayerObj, SpawnPoint, Quaternion.identity);
        player.SpawnAsPlayerObject(clientId);
        UpdateGameManagerPlayerDataRpc(player, RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }
    [Rpc(SendTo.SpecifiedInParams)]
    public void UpdateGameManagerPlayerDataRpc(NetworkObjectReference reference, RpcParams rpcParams){
        GameManager.Singleton.AddPlayerDataRpc(new(Steamworks.SteamClient.Name), NetworkManager.LocalClientId, true, reference);
    }
}
