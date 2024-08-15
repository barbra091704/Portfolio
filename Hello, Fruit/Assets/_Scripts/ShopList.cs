using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopList : MonoBehaviour
{
    public SHoppingCart sC;
    public Animator anim;
    public FruitSpawning fs;
    public Grabbing g;
    public AudioSource sound;
    public Text[] FruitToGetText;
    [SerializeField] private Text fruitCollectedText;
    void Update()
    {
        if (g.fruitCollected < g.fruitRemaining)
        {
            FruitToGetText[g.fruitCollected].text = "" + fs.randomFruit;
        }
        if (Input.GetKeyDown(sC._listkey))
        {
            if (!anim.GetBool("Moving"))
            {
                anim.SetBool("Moving", true);
                sound.Play();
            }
            else if (anim.GetBool("Moving"))
            {
                anim.SetBool("Moving", false);
                sound.Play();
            }

        }
        fruitCollectedText.text = g.fruitCollected + " / " + g.fruitRemaining;

    }

}
