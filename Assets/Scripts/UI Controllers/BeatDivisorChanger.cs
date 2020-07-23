using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BeatDivisorChanger : MonoBehaviour
{
    public void ChangeDivisor(GameObject slider)
    {
        gameObject.GetComponent<Text>().text = $"1/{slider.GetComponent<Slider>().value}";
    }
}
