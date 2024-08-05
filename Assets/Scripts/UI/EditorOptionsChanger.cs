using System;
using CCE.Core;
using UnityEngine;
using UnityEngine.UI;

namespace CCE.UI
{
    public class EditorOptionsChanger : MonoBehaviour
    {
        public Slider HitsoundVolumeSlider;
        public Slider VerticalLineCountSlider;
        public Slider NoteSizeSlider;
        public GameObject VerticalLineCountLabel;
        public GameObject NoteSizeLabel;
        public Toggle PreciseOffsetToggle;
        public Toggle HoldEndHitsoundsToggle;
        public Toggle ShowApproachingNotesToggle;
        public Toggle MoveTimelineDuringPlaybackToggle;
        public Toggle DebugModeToggle;

        private void Start()
        {
            HitsoundVolumeSlider.SetValueWithoutNotify(GlobalState.Config.HitsoundVolume);
            HitsoundVolumeSlider.onValueChanged.AddListener(volume => GlobalState.Config.HitsoundVolume = volume);

            VerticalLineCountSlider.onValueChanged.AddListener(value =>
            {
                GlobalState.Config.VerticalDivisors = (int)Math.Round(value);
                VerticalLineCountLabel.GetComponent<Text>().text =
                    $"Vertical lines: {GlobalState.Config.VerticalDivisors + 1}";
            });
            VerticalLineCountSlider.value = GlobalState.Config.VerticalDivisors;

            NoteSizeSlider.onValueChanged.AddListener(value =>
            {
                GlobalState.Config.DefaultNoteSize = value / 10;
                NoteSizeLabel.GetComponent<Text>().text = $"Note size: {value / 10}";
            });
            NoteSizeSlider.value = GlobalState.Config.DefaultNoteSize * 10;

            PreciseOffsetToggle.SetIsOnWithoutNotify(GlobalState.Config.PreciseOffsetDelta);
            PreciseOffsetToggle.onValueChanged.AddListener(value => GlobalState.Config.PreciseOffsetDelta = value);

            HoldEndHitsoundsToggle.SetIsOnWithoutNotify(GlobalState.Config.PlayHitsoundsOnHoldEnd);
            HoldEndHitsoundsToggle.onValueChanged.AddListener(value => GlobalState.Config.PlayHitsoundsOnHoldEnd = value);

            ShowApproachingNotesToggle.SetIsOnWithoutNotify(GlobalState.Config.ShowApproachingNotesWhilePaused);
            ShowApproachingNotesToggle.onValueChanged.AddListener(value =>
                GlobalState.Config.ShowApproachingNotesWhilePaused = value);

            MoveTimelineDuringPlaybackToggle.SetIsOnWithoutNotify(GlobalState.Config.UpdateTimelineWhileRunning);
            MoveTimelineDuringPlaybackToggle.onValueChanged.AddListener(value =>
                GlobalState.Config.UpdateTimelineWhileRunning = value);

            GameObject.Find("NoteInteractionToggle").GetComponent<Toggle>()
                .SetIsOnWithoutNotify(GlobalState.Config.InteractWithNotesOnOtherPages);
            GameObject.Find("NoteInteractionToggle").GetComponent<Toggle>().onValueChanged
                .AddListener(value => GlobalState.Config.InteractWithNotesOnOtherPages = value);

            GameObject.Find("HorizontalSnapToggle").GetComponent<Toggle>()
                .SetIsOnWithoutNotify(GlobalState.Config.HorizontalSnap);
            GameObject.Find("HorizontalSnapToggle").GetComponent<Toggle>().onValueChanged
                .AddListener(value => GlobalState.Config.HorizontalSnap = value);

            GameObject.Find("HorizontalAccentsToggle").GetComponent<Toggle>()
                .SetIsOnWithoutNotify(GlobalState.Config.HorizontalLineAccents);
            GameObject.Find("HorizontalAccentsToggle").GetComponent<Toggle>().onValueChanged
                .AddListener(value => GlobalState.Config.HorizontalLineAccents = value);

            GameObject.Find("VerticalAccentsToggle").GetComponent<Toggle>()
                .SetIsOnWithoutNotify(GlobalState.Config.VerticalLineAccent);
            GameObject.Find("VerticalAccentsToggle").GetComponent<Toggle>().onValueChanged
                .AddListener(value => GlobalState.Config.VerticalLineAccent = value);
        }
    }
}