using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GUIManager : NetworkBehaviour
{

    public static GUIManager Singleton { get; private set;}

    public NetworkVariable<bool> isInGui = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public List<IGUI> openGUIs = new();
    public int openGUICount;
    public Canvas UI;
    private PlayerMovement playerMovement;
    private PlayerCamera playerCamera;
    private Interaction interaction;

    private InputManager inputManager;
    void Start()
    {
        if (!IsOwner) return;

        if (Singleton != null && Singleton != this) Destroy(this);
        else Singleton = this;

        isInGui.OnValueChanged += ToggleUI;

        inputManager = GetComponent<InputManager>();
        playerMovement = GetComponent<PlayerMovement>();
        interaction = GetComponent<Interaction>();
        playerCamera = GetComponent<PlayerCamera>();
    }

    void Update()
    {
        if (!IsOwner) return;

        if (inputManager.EscapePressed() && openGUIs.Count > 0)
        {
            RemoveGUI();
        }
    }
    public void ToggleUI(bool _, bool newValue)
    {
        if(!newValue)
        {
            playerMovement.CanMove = true;
            interaction.canInteract = true;
            playerCamera.canLook = true;
            UI.enabled = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            playerMovement.CanMove = false;
            interaction.canInteract = false;
            playerCamera.canLook = false;
            UI.enabled = false; 
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
    }

    public void AddGUI(IGUI newGui)
    {
        if (IsOwner)
        {
            if (openGUIs.Count == 0)
            {
                isInGui.Value = true;
            }
            
            openGUIs.Add(newGui);
            openGUICount++;
        }
    }

    public void RemoveGUI()
    {
        if (IsOwner)
        {
            if (openGUIs.Count > 0)
            {
                openGUIs[^1].CloseGUI();
                openGUIs.RemoveAt(openGUIs.Count -1);
                openGUICount--;

                if (openGUIs.Count == 0)
                {
                    isInGui.Value = false;
                }
            }
        }
    }

}
