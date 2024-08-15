using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class ShipWreckTunnel : MonoBehaviour, IInteractable
{
    [Range(1, 9)] public int Channel = 1;
    public bool OverridePosition = false;
    public Transform overrideTransform;

    private static Dictionary<int, List<ShipWreckTunnel>> tunnelsByChannel = new();

    void Awake()
    {
        if (!tunnelsByChannel.ContainsKey(Channel))
        {
            tunnelsByChannel[Channel] = new();
        }

        // Add this tunnel to the list for its channel
        tunnelsByChannel[Channel].Add(this);
    }

    void OnDestroy()
    {
        // Remove this tunnel from its channel list
        if (tunnelsByChannel.ContainsKey(Channel))
        {
            tunnelsByChannel[Channel].Remove(this);
            // Optionally, clean up the dictionary entry if no tunnels are left for this channel
            if (tunnelsByChannel[Channel].Count == 0)
            {
                tunnelsByChannel.Remove(Channel);
            }
        }
    }

    public void Interact<T>(RaycastHit hit, NetworkObject player, T type)
    {
        if (player == null) return;

        // Check if theres another tunnel for this channel
        if (tunnelsByChannel.TryGetValue(Channel, out List<ShipWreckTunnel> channelTunnels))
        {
            // Find the other tunnel that is not this
            foreach (var tunnel in channelTunnels)
            {
                if (tunnel != this)
                {
                    var transform = player.GetComponent<NetworkTransform>();

                    if (tunnel.OverridePosition && tunnel.overrideTransform != null)
                    {
                        transform.Teleport(tunnel.overrideTransform.position, Quaternion.identity, Vector3.one);
                    }
                    else
                    {
                        transform.Teleport(tunnel.overrideTransform.position, Quaternion.identity, Vector3.one);
                    }
                    break;
                }
            }
        }
    }

}
