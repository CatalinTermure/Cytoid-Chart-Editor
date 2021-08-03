using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class responsible for showing toast messages.
/// Must be attached to a <see cref="GameObject"/> with a <see cref="Text"/> component.
/// </summary>
public class ToastMessageManager : MonoBehaviour
{
    private float _toastEndTime = -1;

    public void CreateToast(string toast, int toastDuration = 3)
    {
        gameObject.GetComponent<Text>().text = toast;
        _toastEndTime = Time.time + toastDuration;
    }

    private void Update()
    {
        if (_toastEndTime > 0)
        {
            if (_toastEndTime < Time.time)
            {
                gameObject.GetComponent<Text>().text = null;
                _toastEndTime = -1;
            }
            else
            {
                Color c = gameObject.GetComponent<Text>().color;
                gameObject.GetComponent<Text>().color = new Color(c.r, c.g, c.b, _toastEndTime - Time.time);
            }
        }
    }
}
