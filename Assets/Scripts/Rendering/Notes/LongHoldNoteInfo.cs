using System;
using UnityEngine;

namespace CCE.Rendering.Notes
{
    public class LongHoldNoteInfo : MonoBehaviour
    {
        public Transform NoteTransform;
        public Transform NoteBodyTransform;
        public SpriteRenderer NoteFill;
        public SpriteRenderer NoteRing;
        public SpriteRenderer NoteBodyBackgroundTop;
        public SpriteRenderer NoteBodyBackgroundBottom;
        public SpriteRenderer NoteCompletedBodyTop;
        public SpriteRenderer NoteCompletedBodyBottom;
        [NonSerialized] public double IntroTime;
        [NonSerialized] public double StartTime;
        [NonSerialized] public double EndTime;
        [NonSerialized] public float Size;
        [NonSerialized] public float Opacity;
        [NonSerialized] public float X;
        [NonSerialized] public float Y;
    }
}
