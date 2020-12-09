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
        gameObject.GetComponent<Image>().preserveAspect = true;
        (gameObject.transform as RectTransform).sizeDelta = new Vector2(gameObject.GetComponentInParent<RectTransform>().sizeDelta.x, gameObject.GetComponentInParent<RectTransform>().sizeDelta.x);
    }
}
