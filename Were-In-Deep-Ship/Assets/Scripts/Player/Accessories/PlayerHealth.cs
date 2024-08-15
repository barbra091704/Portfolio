using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour, IDamagable
{
    public NetworkVariable<int> Health = new(100);
    public NetworkVariable<int> Resistance = new(0);

    public override void OnNetworkSpawn()
    {
        GetComponent<Oxygen>().AsphyxiateEvent += TakeDamageRpc;
    }

    public void TakeDamage(int value, bool applyResistance)
    {
        TakeDamageRpc(value, applyResistance);
    }

    [Rpc(SendTo.Server)]
    public void SetResistanceRpc(int value)
    {
        Resistance.Value = value;
    }

    [Rpc(SendTo.Server)]
    public void TakeDamageRpc(int amount, bool applyResistance, RpcParams rpcParams = default)
    {
        PlayerHealth health = NetworkManager.Singleton.ConnectedClients[rpcParams.Receive.SenderClientId].PlayerObject.GetComponent<PlayerHealth>();
        
        float resistanceMultiplier = 1;

        if (applyResistance) resistanceMultiplier = 1 - (health.Resistance.Value / 100f); // Calculate resistance multiplier (e.g., 10 resistance = 0.9 multiplier)

        int damageTaken = Mathf.RoundToInt(amount * resistanceMultiplier);

        health.Health.Value -= damageTaken;
        health.Health.Value = Mathf.Clamp(Health.Value, 0, 100);

        if (health.Health.Value <= 0)
        {
            DieRpc(rpcParams.Receive.SenderClientId, NetworkManager.Singleton.ConnectedClients[rpcParams.Receive.SenderClientId].PlayerObject);
        }
    }
    [Rpc(SendTo.Everyone)]
    public void DieRpc(ulong id, NetworkObjectReference reference)
    {
        if (reference.TryGet(out NetworkObject networkObject))
        {
            networkObject.gameObject.SetActive(false);

            if (IsServer)
                GameManager.Singleton.SetPlayerAliveStateServerRpc(id, false);
        }
    }
}
