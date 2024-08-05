using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CCE.Core
{
    public static class HotkeyManager
    {
        public static KeyValuePair<KeyCode, KeyCode> Copy = new(KeyCode.LeftControl, KeyCode.C);
        public static KeyValuePair<KeyCode, KeyCode> Paste = new(KeyCode.LeftControl, KeyCode.V);
        public static KeyValuePair<KeyCode, KeyCode> SelectAll = new(KeyCode.LeftControl, KeyCode.A);
        public static KeyValuePair<KeyCode, KeyCode> Mirror = new(KeyCode.LeftControl, KeyCode.M);

        public static KeyValuePair<KeyCode, KeyCode> NudgeLeft = new(KeyCode.None, KeyCode.A);
        public static KeyValuePair<KeyCode, KeyCode> NudgeRight = new(KeyCode.None, KeyCode.D);
        public static KeyValuePair<KeyCode, KeyCode> NudgeUp = new(KeyCode.None, KeyCode.W);
        public static KeyValuePair<KeyCode, KeyCode> NudgeDown = new(KeyCode.None, KeyCode.S);

        public static KeyValuePair<KeyCode, KeyCode> MoveTool = new(KeyCode.None, KeyCode.Z);
        public static KeyValuePair<KeyCode, KeyCode> LockY = new(KeyCode.None, KeyCode.X);
        public static KeyValuePair<KeyCode, KeyCode> ToggleSnapX = new(KeyCode.None, KeyCode.Y);

        public static KeyValuePair<KeyCode, KeyCode> ClickNote = new(KeyCode.None, KeyCode.Alpha1);
        public static KeyValuePair<KeyCode, KeyCode> HoldNote = new(KeyCode.None, KeyCode.Alpha2);
        public static KeyValuePair<KeyCode, KeyCode> LongHoldNote = new(KeyCode.None, KeyCode.Alpha3);
        public static KeyValuePair<KeyCode, KeyCode> DragNote = new(KeyCode.None, KeyCode.Alpha4);
        public static KeyValuePair<KeyCode, KeyCode> CDragNote = new(KeyCode.None, KeyCode.Alpha5);
        public static KeyValuePair<KeyCode, KeyCode> FlickNote = new(KeyCode.None, KeyCode.Alpha6);
        public static KeyValuePair<KeyCode, KeyCode> ScanlineNote = new(KeyCode.None, KeyCode.Alpha7);

        public static KeyValuePair<KeyCode, KeyCode> ClickNoteTransform = new(KeyCode.LeftControl, KeyCode.Alpha1);
        public static KeyValuePair<KeyCode, KeyCode> HoldNoteTransform = new(KeyCode.LeftControl, KeyCode.Alpha2);
        public static KeyValuePair<KeyCode, KeyCode> LongHoldNoteTransform = new(KeyCode.LeftControl, KeyCode.Alpha3);
        public static KeyValuePair<KeyCode, KeyCode> DragNoteTransform = new(KeyCode.LeftControl, KeyCode.D);
        public static KeyValuePair<KeyCode, KeyCode> CDragNoteTransform = new(KeyCode.LeftControl, KeyCode.Alpha5);
        public static KeyValuePair<KeyCode, KeyCode> FlickNoteTransform = new(KeyCode.LeftControl, KeyCode.Alpha6);

        public static KeyValuePair<KeyCode, KeyCode> NextPage = new(KeyCode.None, KeyCode.RightArrow);
        public static KeyValuePair<KeyCode, KeyCode> PreviousPage = new(KeyCode.None, KeyCode.LeftArrow);

        public static KeyValuePair<KeyCode, KeyCode> IncreaseHoldTime = new(KeyCode.None, KeyCode.UpArrow);
        public static KeyValuePair<KeyCode, KeyCode> DecreaseHoldTime = new(KeyCode.None, KeyCode.DownArrow);

        public static KeyValuePair<KeyCode, KeyCode> BackToStart = new(KeyCode.None, KeyCode.F);

        public static KeyValuePair<KeyCode, KeyCode> PlayPause = new(KeyCode.None, KeyCode.Space);
        public static KeyValuePair<KeyCode, KeyCode> Save = new(KeyCode.LeftControl, KeyCode.S);
        public static KeyValuePair<KeyCode, KeyCode> Delete = new(KeyCode.None, KeyCode.Delete);
        public static KeyValuePair<KeyCode, KeyCode> Flip = new(KeyCode.LeftControl, KeyCode.F);

        public static KeyValuePair<KeyCode, KeyCode> IncreasePlaybackSpeed = new(KeyCode.None, KeyCode.Equals);
        public static KeyValuePair<KeyCode, KeyCode> DecreasePlaybackSpeed = new(KeyCode.None, KeyCode.Minus);

        public static KeyValuePair<KeyCode, KeyCode> Undo = new(KeyCode.LeftControl, KeyCode.Z);
        public static KeyValuePair<KeyCode, KeyCode> Redo = new(KeyCode.LeftControl, KeyCode.Y);

        public static void LoadCustomHotkeys()
        {
            if (!File.Exists(Path.Combine(Application.persistentDataPath, "Hotkeys.txt"))) return;

            var hotkeys = typeof(HotkeyManager).GetFields(BindingFlags.Public | BindingFlags.Static);
            var hotkeyOverrides = File.ReadAllLines(Path.Combine(Application.persistentDataPath, "Hotkeys.txt"));
            foreach (var hotkeyOverride in hotkeyOverrides)
            {
                var hotkeyName = hotkeyOverride.Split(':')[0];
                var hotkeyMap1 = hotkeyOverride.Split(':')[1].Split('+')[0];
                var hotkeyMap2 = hotkeyOverride.Split(':')[1].Split('+')[1];
                Array.Find(hotkeys,
                    f => f.Name == hotkeyName).SetValue(null,
                    new KeyValuePair<KeyCode, KeyCode>(
                        (KeyCode)Enum.Parse(typeof(KeyCode), hotkeyMap1),
                        (KeyCode)Enum.Parse(typeof(KeyCode), hotkeyMap2)
                    )
                );
            }
        }
    }
}