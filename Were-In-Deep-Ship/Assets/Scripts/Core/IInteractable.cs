using Unity.Netcode;
using UnityEngine;
public interface IInteractable
{
    void Interact<T>(RaycastHit hit, NetworkObject player, T type = default);
}   
