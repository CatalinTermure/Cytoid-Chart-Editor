using System.IO;
using UnityEngine;

namespace CCE.Core
{
    public class EditorConfig
    {
        public string DirPath = Application.persistentDataPath;
        public float HitsoundVolume = 0.25f;
        public float DefaultNoteSize = 2;
        public int VerticalDivisors = 24;
        public bool HorizontalSnap = true;
        public string LevelStoragePath = Path.Combine(Application.persistentDataPath, "charts");
        public string MusicStoragePath = Path.Combine(Application.persistentDataPath, "music");
        public string BackgroundStoragePath = Path.Combine(Application.persistentDataPath, "backgrounds");
        public string TempStoragePath = Path.Combine(Application.persistentDataPath, "temps");
        public bool ShowApproachingNotesWhilePaused = false;
        public int UserOffset = 0;
        public bool PreciseOffsetDelta = false;
        public bool PlayHitsoundsOnHoldEnd = true;
        public bool UpdateTimelineWhileRunning = true;
        public bool DebugMode = false;
        public bool InteractWithNotesOnOtherPages = true;
        public bool VerticalLineAccent = true;
        public bool HorizontalLineAccents = true;
        public float HitsoundPrepTime = 0.05f;
        public bool LoadBackgroundsInLevelSelect = true;
    }
}
