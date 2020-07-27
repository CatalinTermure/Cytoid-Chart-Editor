using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorConfig
{
    public string DirPath = Application.persistentDataPath;
    public float HitsoundVolume = 0.25f, DefaultNoteSize = 2;
    public int VerticalDivisors = 24;
    public bool EnableLetterboxing = true;
    public bool ShowFPS = false;
    public bool ShowApproachingNotesWhilePaused = false;
    public int UserOffset = 0;
    public bool PreciseOffsetDelta = false;
    public bool PlayHitsoundsOnHoldEnd = true;
}
