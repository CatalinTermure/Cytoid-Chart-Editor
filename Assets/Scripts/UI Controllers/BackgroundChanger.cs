using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundChanger : MonoBehaviour
{
    [Range(0f, 1f)]
    public float Brightness;

    void Start()
    {
        if(GlobalState.BackgroundSprite != null)
        {
            gameObject.GetComponent<Image>().sprite = GlobalState.BackgroundSprite;
        }
        gameObject.GetComponent<Image>().color = new Color(Brightness, Brightness, Brightness);
    }
}
