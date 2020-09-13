using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToastMessageManager : MonoBehaviour
{
    private float toastendtime = -1;

    public void CreateToast(string toast)
    {
        gameObject.GetComponent<Text>().text = toast;
        toastendtime = Time.time + 3;
    }

    private void Update()
    {
        if(toastendtime != -1)
        {
            if(toastendtime < Time.time)
            {
                gameObject.GetComponent<Text>().text = null;
                toastendtime = -1;
            }
            else
            {
                Color c = gameObject.GetComponent<Text>().color;
                gameObject.GetComponent<Text>().color = new Color(c.r, c.g, c.b, toastendtime - Time.time);
            }
        }
    }
}
