using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Ladder : NetworkBehaviour, IInteractable
{
    [SerializeField] private Transform startPosition;
    [SerializeField] private Transform endPosition;

    public void Interact<T>(RaycastHit hit, NetworkObject player, T type = default)
    {
        Vector3 pos = new(startPosition.position.x, player.transform.position.y, startPosition.position.z);
        player.GetComponent<PlayerMovement>().OnLadder(pos, this);
    }

    public Vector3 GetEndPos()
    {
        return endPosition.position;
    }
}
