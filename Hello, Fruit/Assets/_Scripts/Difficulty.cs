using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Difficulty : MonoBehaviour
{
    public Text difficultyText;
    public Button difficultybutton;
    public static int _Difficulty;

    private void Start()
    {
        Button db = difficultybutton.GetComponent<Button>();
        db.onClick.AddListener(ChangeDifficulty);
        _Difficulty = 2;
    }

    public void ChangeDifficulty()
    {

        if (_Difficulty == 4)
        {
            _Difficulty = 0;
        }
        else
        {
            _Difficulty++;
        }

    }

    private void LateUpdate()
    {
        if (_Difficulty == 0)
        {
            difficultyText.text = "Baby";
        }
        if (_Difficulty == 1)
        {
            difficultyText.text = "Easy";
        }
        if (_Difficulty == 2)
        {
            difficultyText.text = "Normal";
        }
        if (_Difficulty == 3)
        {
            difficultyText.text = "Hard";
        }
        if (_Difficulty == 4)
        {
            difficultyText.text = "Insane";
        }
    }

}
