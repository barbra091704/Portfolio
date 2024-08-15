using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SetupLoader : MonoBehaviour
{
    public void Play()
    {
        StartCoroutine(LoadMainScene());   
    }
    public void Quit()
    {
        Application.Quit();
    }

    IEnumerator LoadMainScene(){
        yield return new WaitUntil(()=> NetworkManager.Singleton != null);
        SceneManager.LoadScene("Menu");
    }
}
