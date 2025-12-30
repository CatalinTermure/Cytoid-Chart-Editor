using System;
using UnityEngine;

namespace CCE.Rendering.Notes
{
    public class HoldNoteInfo : MonoBehaviour
    {
        public Transform NoteTransform;
        public Transform NoteBodyTransform;
        public SpriteRenderer NoteFill;
        public SpriteRenderer NoteRing;
        public SpriteRenderer NoteBodyBackground;
        public SpriteRenderer NoteCompletedBody;
        [NonSerialized] public double IntroTime;
        [NonSerialized] public double StartTime;
        [NonSerialized] public double EndTime;
        [NonSerialized] public float Size;
        [NonSerialized] public float Opacity;
        [NonSerialized] public float X;
        [NonSerialized] public float Y;
    }
}
