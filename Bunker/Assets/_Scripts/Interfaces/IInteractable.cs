using Unity.Netcode;
using UnityEngine;

namespace Core
{
    public interface IInteractable
    {
        void Interact<T>(RaycastHit hit, NetworkObject player, T type = default);
    }   

}
