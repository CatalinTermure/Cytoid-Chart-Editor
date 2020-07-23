using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorOptionsChanger : MonoBehaviour
{
    public Slider HitsoundVolumeSlider;

    private void Start()
    {
        HitsoundVolumeSlider.SetValueWithoutNotify(GlobalState.HitsoundVolume);
        HitsoundVolumeSlider.onValueChanged.AddListener((float volume) => GlobalState.HitsoundVolume = volume);
    }

    public void SaveOptions()
    {
        GlobalState.SaveHitsoundVolume();
    }
}
