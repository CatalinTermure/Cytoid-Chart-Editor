using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorOptionsChanger : MonoBehaviour
{
    public Slider HitsoundVolumeSlider, VerticalLineCountSlider, NoteSizeSlider;
    public GameObject VerticalLineCountLabel, NoteSizeLabel;
    public Toggle PreciseOffsetToggle, HoldEndHitsoundsToggle, ShowApproachingNotesToggle, MoveTimelineDuringPlaybackToggle, DebugModeToggle;

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
            GlobalState.Config.DefaultNoteSize = value / 10;
            NoteSizeLabel.GetComponent<Text>().text = $"Note size: {value / 10}";
        });
        NoteSizeSlider.value = GlobalState.Config.DefaultNoteSize * 10;

        PreciseOffsetToggle.SetIsOnWithoutNotify(GlobalState.Config.PreciseOffsetDelta);
        PreciseOffsetToggle.onValueChanged.AddListener((bool value) => GlobalState.Config.PreciseOffsetDelta = value);

        HoldEndHitsoundsToggle.SetIsOnWithoutNotify(GlobalState.Config.PlayHitsoundsOnHoldEnd);
        HoldEndHitsoundsToggle.onValueChanged.AddListener((bool value) => GlobalState.Config.PlayHitsoundsOnHoldEnd = value);

        ShowApproachingNotesToggle.SetIsOnWithoutNotify(GlobalState.Config.ShowApproachingNotesWhilePaused);
        ShowApproachingNotesToggle.onValueChanged.AddListener((bool value) => GlobalState.Config.ShowApproachingNotesWhilePaused = value);

        MoveTimelineDuringPlaybackToggle.SetIsOnWithoutNotify(GlobalState.Config.UpdateTimelineWhileRunning);
        MoveTimelineDuringPlaybackToggle.onValueChanged.AddListener((bool value) => GlobalState.Config.UpdateTimelineWhileRunning = value);

        DebugModeToggle.SetIsOnWithoutNotify(GlobalState.Config.DebugMode);
        DebugModeToggle.onValueChanged.AddListener((bool value) => GlobalState.Config.DebugMode = value);

        GameObject.Find("NotchFixToggle").GetComponent<Toggle>().SetIsOnWithoutNotify(GlobalState.Config.IsNotchNotWorking);
        GameObject.Find("NotchFixToggle").GetComponent<Toggle>().onValueChanged.AddListener((bool value) => GlobalState.Config.IsNotchNotWorking = value);
    }

    public void SaveOptions()
    {
        GlobalState.SaveConfig();
    }
}
