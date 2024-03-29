﻿using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CCE
{
    public static class HotkeyManager
    {
        public static KeyValuePair<KeyCode, KeyCode> Copy = new KeyValuePair<KeyCode, KeyCode>(KeyCode.LeftControl, KeyCode.C);
        public static KeyValuePair<KeyCode, KeyCode> Paste = new KeyValuePair<KeyCode, KeyCode>(KeyCode.LeftControl, KeyCode.V);
        public static KeyValuePair<KeyCode, KeyCode> SelectAll = new KeyValuePair<KeyCode, KeyCode>(KeyCode.LeftControl, KeyCode.A);
        public static KeyValuePair<KeyCode, KeyCode> Mirror = new KeyValuePair<KeyCode, KeyCode>(KeyCode.LeftControl, KeyCode.M);

        public static KeyValuePair<KeyCode, KeyCode> NudgeLeft = new KeyValuePair<KeyCode, KeyCode>(KeyCode.None, KeyCode.A);
        public static KeyValuePair<KeyCode, KeyCode> NudgeRight = new KeyValuePair<KeyCode, KeyCode>(KeyCode.None, KeyCode.D);
        public static KeyValuePair<KeyCode, KeyCode> NudgeUp = new KeyValuePair<KeyCode, KeyCode>(KeyCode.None, KeyCode.W);
        public static KeyValuePair<KeyCode, KeyCode> NudgeDown = new KeyValuePair<KeyCode, KeyCode>(KeyCode.None, KeyCode.S);

        public static KeyValuePair<KeyCode, KeyCode> MoveTool = new KeyValuePair<KeyCode, KeyCode>(KeyCode.None, KeyCode.Z);
        public static KeyValuePair<KeyCode, KeyCode> LockY = new KeyValuePair<KeyCode, KeyCode>(KeyCode.None, KeyCode.X);

        public static KeyValuePair<KeyCode, KeyCode> ClickNote = new KeyValuePair<KeyCode, KeyCode>(KeyCode.None, KeyCode.Alpha1);
        public static KeyValuePair<KeyCode, KeyCode> HoldNote = new KeyValuePair<KeyCode, KeyCode>(KeyCode.None, KeyCode.Alpha2);
        public static KeyValuePair<KeyCode, KeyCode> LongHoldNote = new KeyValuePair<KeyCode, KeyCode>(KeyCode.None, KeyCode.Alpha3);
        public static KeyValuePair<KeyCode, KeyCode> DragNote = new KeyValuePair<KeyCode, KeyCode>(KeyCode.None, KeyCode.Alpha4);
        public static KeyValuePair<KeyCode, KeyCode> CDragNote = new KeyValuePair<KeyCode, KeyCode>(KeyCode.None, KeyCode.Alpha5);
        public static KeyValuePair<KeyCode, KeyCode> FlickNote = new KeyValuePair<KeyCode, KeyCode>(KeyCode.None, KeyCode.Alpha6);
        public static KeyValuePair<KeyCode, KeyCode> ScanlineNote = new KeyValuePair<KeyCode, KeyCode>(KeyCode.None, KeyCode.Alpha7);

        public static KeyValuePair<KeyCode, KeyCode> ClickNoteTransform = new KeyValuePair<KeyCode, KeyCode>(KeyCode.LeftControl, KeyCode.Alpha1);
        public static KeyValuePair<KeyCode, KeyCode> HoldNoteTransform = new KeyValuePair<KeyCode, KeyCode>(KeyCode.LeftControl, KeyCode.Alpha2);
        public static KeyValuePair<KeyCode, KeyCode> LongHoldNoteTransform = new KeyValuePair<KeyCode, KeyCode>(KeyCode.LeftControl, KeyCode.Alpha3);
        public static KeyValuePair<KeyCode, KeyCode> DragNoteTransform = new KeyValuePair<KeyCode, KeyCode>(KeyCode.LeftControl, KeyCode.D);
        public static KeyValuePair<KeyCode, KeyCode> CDragNoteTransform = new KeyValuePair<KeyCode, KeyCode>(KeyCode.LeftControl, KeyCode.Alpha5);
        public static KeyValuePair<KeyCode, KeyCode> FlickNoteTransform = new KeyValuePair<KeyCode, KeyCode>(KeyCode.LeftControl, KeyCode.Alpha6);

        public static KeyValuePair<KeyCode, KeyCode> NextPage = new KeyValuePair<KeyCode, KeyCode>(KeyCode.None, KeyCode.RightArrow);
        public static KeyValuePair<KeyCode, KeyCode> PreviousPage = new KeyValuePair<KeyCode, KeyCode>(KeyCode.None, KeyCode.LeftArrow);

        public static KeyValuePair<KeyCode, KeyCode> IncreaseHoldTime = new KeyValuePair<KeyCode, KeyCode>(KeyCode.None, KeyCode.UpArrow);
        public static KeyValuePair<KeyCode, KeyCode> DecreaseHoldTime = new KeyValuePair<KeyCode, KeyCode>(KeyCode.None, KeyCode.DownArrow);

        public static KeyValuePair<KeyCode, KeyCode> BackToStart = new KeyValuePair<KeyCode, KeyCode>(KeyCode.None, KeyCode.F);

        public static KeyValuePair<KeyCode, KeyCode> PlayPause = new KeyValuePair<KeyCode, KeyCode>(KeyCode.None, KeyCode.Space);
        public static KeyValuePair<KeyCode, KeyCode> Save = new KeyValuePair<KeyCode, KeyCode>(KeyCode.LeftControl, KeyCode.S);
        public static KeyValuePair<KeyCode, KeyCode> Delete = new KeyValuePair<KeyCode, KeyCode>(KeyCode.None, KeyCode.Delete);
        public static KeyValuePair<KeyCode, KeyCode> Flip = new KeyValuePair<KeyCode, KeyCode>(KeyCode.LeftControl, KeyCode.F);

        public static KeyValuePair<KeyCode, KeyCode> IncreasePlaybackSpeed = new KeyValuePair<KeyCode, KeyCode>(KeyCode.None, KeyCode.Equals);
        public static KeyValuePair<KeyCode, KeyCode> DecreasePlaybackSpeed = new KeyValuePair<KeyCode, KeyCode>(KeyCode.None, KeyCode.Minus);
    
        public static KeyValuePair<KeyCode, KeyCode> Undo = new KeyValuePair<KeyCode, KeyCode>(KeyCode.LeftControl, KeyCode.Z);
        public static KeyValuePair<KeyCode, KeyCode> Redo = new KeyValuePair<KeyCode, KeyCode>(KeyCode.LeftControl, KeyCode.Y);

        public static void LoadCustomHotkeys()
        {
            if (!File.Exists(Path.Combine(Application.persistentDataPath, "Hotkeys.txt"))) return;
            
            FieldInfo[] hotkeys = typeof(HotkeyManager).GetFields(BindingFlags.Public | BindingFlags.Static);
            string[] hotkeyOverrides = File.ReadAllLines(Path.Combine(Application.persistentDataPath, "Hotkeys.txt"));
            foreach (string hotkeyOverride in hotkeyOverrides)
            {
                string hotkeyName = hotkeyOverride.Split(':')[0];
                string hotkeyMap1 = hotkeyOverride.Split(':')[1].Split('+')[0];
                string hotkeyMap2 = hotkeyOverride.Split(':')[1].Split('+')[1];
                System.Array.Find(hotkeys,
                    f => f.Name == hotkeyName).SetValue(null,
                    new KeyValuePair<KeyCode, KeyCode>(
                        (KeyCode)System.Enum.Parse(typeof(KeyCode), hotkeyMap1),
                        (KeyCode)System.Enum.Parse(typeof(KeyCode), hotkeyMap2)
                    )
                );
            }
        }
    }
}
