using System;
using System.IO;
using CCE.UI;
using UnityEngine;

namespace CCE.Core
{
    public class EditorConfig
    {
        public string LevelStoragePath = Path.Combine(Application.persistentDataPath, "charts");
        public string DirPath = Application.persistentDataPath;
        public string TempStoragePath = Path.Combine(Application.persistentDataPath, "temps");
        public int UserOffset = 0;
        
        [Displayable(Section = "Audio", Name = "Hitsound Volume:", MinValue = 0f, MaxValue = 1f)]
        public float HitsoundVolume = 1.0f;

        [Displayable(Section = "Audio", Name = "Music Volume:", MinValue = 0f, MaxValue = 1f)]
        public float MusicVolume = 1.0f;

        [Displayable(Section = "Audio", Name = "Play hitsounds on end of hold notes:")]
        public bool PlayHitsoundsOnHoldEnd = true;

        [Displayable(Section = "Audio", Name = "Precise calibration of offset(1ms interval):")]
        public bool PreciseOffsetDelta = false;
        
        [Displayable(Section = "Edit mode", Name = "Snap X position to vertical lines:")]
        public bool HorizontalSnap = true;

        [Displayable(Section = "Edit mode", Name = "Interact with notes on other pages:")]
        public bool InteractWithNotesOnOtherPages = true;

        [Displayable(Section = "Visual", Name = "Note size:", MinValue = 0.1f, MaxValue = 4f)]
        public float DefaultNoteSize = 2;
        
        [Displayable(Section = "Visual", Name = "Accentuate frequently used horizontal lines:")]
        public bool HorizontalLineAccents = true;

        [Displayable(Section = "Visual", Name = "Load background previews in level select:")]
        public bool LoadBackgroundsInLevelSelect = true;

        [Displayable(Section = "Visual", Name = "Show approaching notes when paused:")]
        public bool ShowApproachingNotesWhilePaused = false;
        
        [Displayable(Section = "Visual", Name = "Update timeline while chart is playing:")]
        public bool UpdateTimelineWhileRunning = true;

        [Displayable(Section = "Visual", Name = "Number of vertical lines:", MinValue = 0, MaxValue = 64)]
        public int VerticalDivisors = 24;

        [Displayable(Section = "Visual", Name = "Accentuate vertical lines:")]
        public bool VerticalLineAccent = true;
    }
}