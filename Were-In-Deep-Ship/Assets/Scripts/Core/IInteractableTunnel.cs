using Unity.Netcode;
using UnityEngine;

public class IInteractableTunnel : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject interactableObject;

    public bool UseBool;
    public bool boolValue;

    public void Interact<T>(RaycastHit hit, NetworkObject Player, T type)
    {
        if(interactableObject.TryGetComponent(out IInteractable interactable)){
            if (UseBool) interactable.Interact(hit,Player, boolValue);
            else interactable.Interact<T>(hit,Player);
        }
    }
}
