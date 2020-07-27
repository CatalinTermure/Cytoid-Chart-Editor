using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorOptionsChanger : MonoBehaviour
{
    public Slider HitsoundVolumeSlider, VerticalLineCountSlider, NoteSizeSlider;
    public GameObject VerticalLineCountLabel, NoteSizeLabel;
    public Toggle PreciseOffsetToggle, HoldEndHitsoundsToggle, ShowApproachingNotesToggle;

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
            GlobalState.Config.DefaultNoteSize = (float)Math.Round(value, 2);
            NoteSizeLabel.GetComponent<Text>().text = $"Note size: {value}";
        });
        NoteSizeSlider.value = GlobalState.Config.DefaultNoteSize;

        PreciseOffsetToggle.SetIsOnWithoutNotify(GlobalState.Config.PreciseOffsetDelta);
        PreciseOffsetToggle.onValueChanged.AddListener((bool value) => GlobalState.Config.PreciseOffsetDelta = value);

        HoldEndHitsoundsToggle.SetIsOnWithoutNotify(GlobalState.Config.PlayHitsoundsOnHoldEnd);
        HoldEndHitsoundsToggle.onValueChanged.AddListener((bool value) => GlobalState.Config.PlayHitsoundsOnHoldEnd = value);

        ShowApproachingNotesToggle.SetIsOnWithoutNotify(GlobalState.Config.ShowApproachingNotesWhilePaused);
        ShowApproachingNotesToggle.onValueChanged.AddListener((bool value) => GlobalState.Config.ShowApproachingNotesWhilePaused = value);
    }

    public void SaveOptions()
    {
        GlobalState.SaveConfig();
    }
}
