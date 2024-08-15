using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour
{
    public Button playbutton;

    private void Start()
    {
        Button pb = playbutton.GetComponent<Button>();
        pb.onClick.AddListener(Playg);
    }

    public void Playg()
    {
        SceneManager.LoadScene("Game");
        SceneManager.UnloadSceneAsync("Start");
    }
}


