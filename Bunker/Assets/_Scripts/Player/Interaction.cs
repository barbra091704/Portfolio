using System;
using UnityEngine;
using Unity.Netcode;
using Core;

public class Interaction : NetworkBehaviour
{
    [SerializeField] private float interactDelay = 1.0f; // Delay for interaction
    [SerializeField] private float interactDistance = 2f;
    [SerializeField] private int delayedInteractionLayer = 7; // Specific layer for delayed interaction
    [SerializeField] private float delayedInteractionTime = 0.5f;
    [SerializeField] private LayerMask layerMask; // All interactable layers
    public Camera mainCamera;
    public bool canInteract = true;
    private float interactTimer = 0f;
    private bool isInteractingWithDelayedObject = false;
    private bool hasInteractedDelayed = false;
    private bool interactionInProgress = false; // Flag to track whether interaction is in progress
    private InputManager inputManager;

    public event Action<RaycastHit> E_OnItemPickup;
    public event Action<float> E_OnInteractSliderValueChanged;

    private void Start()
    {
        inputManager = InputManager.Instance;
    }

    private void LateUpdate()
    {
        if (!IsOwner || !canInteract) return;

        if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out RaycastHit hitInfo, interactDistance, layerMask))
        {
            if (!interactionInProgress && inputManager.InteractedThisFrame())
            {
                if (hitInfo.collider.gameObject.layer != delayedInteractionLayer && Time.time - interactTimer >= interactDelay)
                {
                    TriggerInstantInteraction(hitInfo);
                    interactionInProgress = true; // Set flag to indicate interaction is in progress
                }
            }
        }
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
                    E_OnInteractSliderValueChanged?.Invoke(0);
                }
            }
            else
            {
                // Reset if raycast loses sight of the delayed interaction object
                isInteractingWithDelayedObject = false;
                E_OnInteractSliderValueChanged?.Invoke(0);
            }
        }
        else // If interact button is released
        {
            if (inputManager.InteractReleased())
            {
                interactionInProgress = false; // Reset interaction state when interact key is released
                hasInteractedDelayed = false; // Reset delayed interaction flag
                isInteractingWithDelayedObject = false;
                E_OnInteractSliderValueChanged?.Invoke(0);
            }
        }
    }

    private void TriggerInstantInteraction(RaycastHit hitInfo)
    {
        IInteractable interactable = hitInfo.collider.GetComponent<IInteractable>() ?? hitInfo.collider.transform.parent.GetComponent<IInteractable>();


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
        E_OnInteractSliderValueChanged?.Invoke(progress);

        // Check if the timer has elapsed and interaction hasn't occurred yet
        if (progress >= delayedInteractionTime && !hasInteractedDelayed)
        {
            TriggerInstantInteraction(hitInfo);
            hasInteractedDelayed = true; // Mark as interacted
            E_OnInteractSliderValueChanged?.Invoke(0);
        }
        else if (hasInteractedDelayed && progress < delayedInteractionTime)
        {
            hasInteractedDelayed = false; // Reset interaction state if the key is re-held before delay time
            E_OnInteractSliderValueChanged?.Invoke(0);
        }
    }
}
