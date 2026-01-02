using System;
using UnityEngine;

namespace CCE.Rendering.Notes
{
    public class CDragHeadNoteInfo : MonoBehaviour
    {
        public Transform NoteTransform;
        public Transform NoteFillTransform;
        public SpriteRenderer NoteFill;
        public SpriteRenderer NoteRing;
        public SpriteRenderer NoteArrow;
        [NonSerialized] public double IntroTime;
        [NonSerialized] public double StartTime;
        [NonSerialized] public double EndTime;
        [NonSerialized] public float Size;
        [NonSerialized] public float Opacity;
        [NonSerialized] public float X;
        [NonSerialized] public float Y;
    }
}
