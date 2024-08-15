using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsKey : MonoBehaviour
{
    public GameObject fps;
    private bool on;
    [SerializeField] private KeyCode fpsKey;

    private void Awake()
    {
        fps.gameObject.SetActive(true);
        on = true;
    }
    private void Update()
    {
        if (Input.GetKeyDown(fpsKey))
        {
            if (on)
            {
                fps.gameObject.SetActive(false);
                on = false;
            }
            else if (!on)
            {
                fps.gameObject.SetActive(true);
                on = true;
            }

        }

    }
}
