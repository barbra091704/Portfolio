using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Quit : MonoBehaviour
{
    public Button quitbutton;

    private void Start()
    {
        Button qb = quitbutton.GetComponent<Button>();
        qb.onClick.AddListener(Quitnow);
    }

    public void Quitnow()
    {
        Application.Quit();
        Debug.Log("quitting;(");
    }


}
