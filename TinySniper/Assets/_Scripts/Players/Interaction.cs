using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class Interaction : NetworkBehaviour
{
    public static Interaction Instance;

    public BoxCollider2D interactionPoint;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } else Destroy(this);
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    public void Interact()
    {
        Collider2D[] overlap = Physics2D.OverlapAreaAll(interactionPoint.bounds.min, interactionPoint.bounds.max);

        if (overlap.Length > 0)
        {
            foreach (var item in overlap)
            {
                if (item.transform.TryGetComponent(out IInteractable interactable))
                {
                    interactable.Interact();
                    print($"Interacted with : {item.name}");
                }
            }
        }
    }
}
