using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FrameCounter : MonoBehaviour
{
    private int cnt = 0;

    void Update()
    {
        cnt++;
        if (cnt == 10)
        {
            gameObject.GetComponent<Text>().text = (1.0 / Time.deltaTime).ToString();
            cnt = 0;
        }
    }
}
