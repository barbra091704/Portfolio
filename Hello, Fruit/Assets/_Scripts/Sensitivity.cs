using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sensitivity : MonoBehaviour
{
    public Slider slider;
    public static float sensitivity;
    public Text sensText;

    private void Start()
    {
        sensitivity = 2;
        slider.value = 2;
    }
    private void Update()
    {
        sensitivity = slider.value;
        sensText.text = "" + sensitivity;
    }
}
