using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorOptionsChanger : MonoBehaviour
{
    public Slider HitsoundVolumeSlider, VerticalLineCountSlider, NoteSizeSlider;
    public GameObject VerticalLineCountLabel, NoteSizeLabel;

    private void Start()
    {
        HitsoundVolumeSlider.SetValueWithoutNotify(GlobalState.Config.HitsoundVolume);
        HitsoundVolumeSlider.onValueChanged.AddListener((float volume) => GlobalState.Config.HitsoundVolume = volume);
        VerticalLineCountSlider.onValueChanged.AddListener((float value) =>
        {
            GlobalState.Config.VerticalDivisors = (int)Math.Round(value);
            VerticalLineCountLabel.GetComponent<Text>().text = $"Vertical lines: {GlobalState.Config.VerticalDivisors}";
        });
        VerticalLineCountSlider.value = GlobalState.Config.VerticalDivisors;
        NoteSizeSlider.onValueChanged.AddListener((float value) =>
        {
            GlobalState.Config.DefaultNoteSize = value;
            NoteSizeLabel.GetComponent<Text>().text = $"Note size: {value}";
        });
        NoteSizeSlider.value = GlobalState.Config.DefaultNoteSize;
    }

    public void SaveOptions()
    {
        GlobalState.SaveConfig();
    }
}
