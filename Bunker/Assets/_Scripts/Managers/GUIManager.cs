using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GUIManager : NetworkBehaviour
{

    public static GUIManager Singleton { get; private set;}

    public NetworkVariable<bool> isInGui = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isInventoryLocked = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public List<IGUI> openGUIs = new();
    public int openGUICount;
    public Canvas UI;
    private InputManager inputManager;
    void Start()
    {
        if (!IsOwner) return;

        if (Singleton != null && Singleton != this) Destroy(this);
        else Singleton = this;

        isInGui.OnValueChanged += ToggleUI;

        inputManager = GetComponent<InputManager>();

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
            UI.enabled = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            UI.enabled = false; 
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
    }

    public void ToggleInventoryLock(bool value)
    {

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
                openGUIs[^1].OnCloseGUI();
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
