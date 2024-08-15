using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SniperGUI : NetworkBehaviour
{
    private Sniper sniper;
    private SniperMovement sniperMovement;
    private Animator infoPanelAnim;
    private Camera cam;
    public Camera renderCamera;
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private GameObject scope;
    [SerializeField] private GameObject sniperVisual;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private Button InfoPanelButton, ReloadButton;

    private bool isInfoPanelOpen = false;

    private void Start()
    {

        cam = GetComponent<Camera>();
        sniper = GetComponent<Sniper>();
        sniperMovement = GetComponent<SniperMovement>();
        infoPanelAnim = infoPanel.GetComponent<Animator>();
        
        if (!IsOwner)
        {
            GetComponent<AudioListener>().enabled = false;
            renderCamera.enabled = false;
            cam.enabled = false;
            mainCanvas.gameObject.SetActive(false);
            return;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        
        HandleScopeToggle();
        UpdateUIElements();
    }

    private void HandleScopeToggle()
    {
        sniper.isScoped = Input.GetMouseButton(1);
        scope.SetActive(sniper.isScoped);
        sniperVisual.SetActive(!sniper.isScoped);
        sniperMovement.canZoom = sniper.isScoped;
        cam.orthographicSize = !sniper.isScoped ? 20 : cam.orthographicSize;
        if(Input.GetMouseButtonDown(1)){

            if(isInfoPanelOpen){
                InfoPanelClicked();
            }
            cam.orthographicSize = sniper.lastZoom;
        }
        Cursor.visible = !sniper.isScoped;

        InfoPanelButton.interactable = !sniper.isScoped;
        ReloadButton.interactable = !sniper.isScoped;
    }


    private void UpdateUIElements()
    {
        bool shouldShowUI = !sniper.isScoped && !isInfoPanelOpen;

        ReloadButton.gameObject.SetActive(shouldShowUI);
    }

    public void InfoPanelClicked()
    {
        string animation = isInfoPanelOpen ? "InfoPanel_Slide_Out" : "InfoPanel_Slide_In";

        isInfoPanelOpen = !isInfoPanelOpen;

        infoPanelAnim.Play(animation);
    }

}
