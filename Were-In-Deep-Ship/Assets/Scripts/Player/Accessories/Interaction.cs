using System;
using UnityEngine;
using Unity.Netcode;
public class Interaction : NetworkBehaviour
{
    [SerializeField] private float interactDelay = 1.0f; // Delay for interaction
    [SerializeField] private float interactDistance = 2f;
    [SerializeField] private int delayedInteractionLayer = 7; // Specific layer for delayed interaction
    [SerializeField] private float delayedInteractionTime = 0.5f;
    [SerializeField] private LayerMask layerMask; // All interactable layers
    public Camera mainCamera;
    public bool canInteract = true;
    private InputManager inputManager;
    private float interactTimer = 0f;
    private bool isInteractingWithDelayedObject = false;
    private bool hasInteractedDelayed = false;

    public event Action<RaycastHit> ItemPickupEvent;
    public event Action<Collider> HoveredItemInfoEvent;
    public event Action<float> SliderValueEvent;

    private void Start()
    {
        inputManager = InputManager.Instance;
    }

    private IHoverable lastHoveredObject;

    private void LateUpdate()
    {
        if (!IsOwner || !canInteract) return;

        IHoverable currentlyHoveredObject = null;

        if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out RaycastHit hitInfo, interactDistance, layerMask))
        {
            if (inputManager.InteractedThisFrame())
            {
                if (hitInfo.collider.gameObject.layer != delayedInteractionLayer && CanInstantInteract())
                {
                    TriggerInstantInteraction(hitInfo);
                }
            }
            else
            {
                if (hitInfo.collider.TryGetComponent(out IHoverable hoverableComponent))
                {
                    hoverableComponent.OnHover(transform);
                }

                if (hitInfo.collider.CompareTag("Item"))
                {
                    HoveredItemInfoEvent?.Invoke(hitInfo.collider);
                }
            }
        }

        if (lastHoveredObject != null && lastHoveredObject != currentlyHoveredObject)
        {
            lastHoveredObject.Disable(transform);
            lastHoveredObject = null;
        }

        lastHoveredObject = currentlyHoveredObject;

        // Check if the interact button is held
        if (inputManager.InteractIsHeld())
        {
            if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out RaycastHit hitInfo3, interactDistance, layerMask))
            {
                // Check if the raycast hit an object of the delayed interaction layer
                if (hitInfo3.collider.gameObject.layer == delayedInteractionLayer)
                {
                    HandleDelayedInteraction(hitInfo3);
                }
                else
                {
                    // Reset if raycast loses sight of the delayed interaction object
                    isInteractingWithDelayedObject = false;
                    SliderValueEvent?.Invoke(0);
                }
            }
            else
            {
                // Reset if raycast loses sight of the delayed interaction object
                isInteractingWithDelayedObject = false;
                SliderValueEvent?.Invoke(0);
            }
        }
        else // If interact button is released
        {
            // Reset interaction state when interact key is released
            isInteractingWithDelayedObject = false;
            SliderValueEvent?.Invoke(0);
            hasInteractedDelayed = false; // Reset delayed interaction flag
        }
    }

    private void TriggerInstantInteraction(RaycastHit hitInfo)
    {
        if (hitInfo.collider.transform.CompareTag("Item"))
        {
            ItemPickupEvent?.Invoke(hitInfo);
            return;
        }
        
        IInteractable interactable = hitInfo.collider.GetComponent<IInteractable>() ?? hitInfo.collider.transform.parent.GetComponent<IInteractable>();
        
        if (hitInfo.collider.transform.CompareTag("TerminalButtonUp"))
        {
            interactable?.Interact(hitInfo, NetworkObject, true);
        }
        else if (hitInfo.collider.transform.CompareTag("TerminalButtonDown"))
        {
            interactable?.Interact(hitInfo, NetworkObject, false);
        }

        interactable?.Interact(hitInfo, NetworkObject, -1);
    }

    private void HandleDelayedInteraction(RaycastHit hitInfo)
    {
        if (!isInteractingWithDelayedObject)
        {
            // Reset timer when first hitting a delayed interaction object
            interactTimer = Time.time;
            isInteractingWithDelayedObject = true;
        }

        // Calculate the progress of the interaction
        float progress = (Time.time - interactTimer) / interactDelay;
        SliderValueEvent?.Invoke(progress);

        // Check if the timer has elapsed and interaction hasn't occurred yet
        if (progress >= delayedInteractionTime && !hasInteractedDelayed)
        {
            TriggerInstantInteraction(hitInfo);
            hasInteractedDelayed = true; // Mark as interacted
            SliderValueEvent?.Invoke(0);
        }
        else if (hasInteractedDelayed && progress < delayedInteractionTime)
        {
            hasInteractedDelayed = false; // Reset interaction state if the key is re-held before delay time
            SliderValueEvent?.Invoke(0);
        }
    }

    private bool CanInstantInteract()
    {
        return Time.time - interactTimer >= interactDelay;
    }
}
