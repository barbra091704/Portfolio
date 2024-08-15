using BrewedInk.CRT;
using UnityEngine;
using UnityEngine.SceneManagement;

public class enabledisable : MonoBehaviour
{

    public bool CRTenabled = true;

    void Awake(){

        DontDestroyOnLoad(this);
    }

    void Update(){

        if(SceneManager.GetActiveScene().name == "Setup")
            return;

        ToggleCRT(!CRTenabled);
    }

    public void SetCRTToggle(){

        ToggleCRT(CRTenabled);
        CRTenabled = !CRTenabled;
    }

    void ToggleCRT(bool state){

        
        // yield return new WaitForSeconds(0.01f);

        CRTCameraBehaviour[] crtA = FindObjectsOfType<CRTCameraBehaviour>();

        foreach(CRTCameraBehaviour c in crtA){

            c.enabled = state;
        }
    }
}
