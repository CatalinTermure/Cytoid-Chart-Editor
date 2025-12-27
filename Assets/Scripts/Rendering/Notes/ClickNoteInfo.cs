using System;
using UnityEngine;

namespace CCE.Rendering.Notes
{
    public class ClickNoteInfo : MonoBehaviour
    {
        public Transform NoteTransform;
        public Transform NoteFillTransform;
        public SpriteRenderer NoteFill;
        public SpriteRenderer NoteRing;
        [NonSerialized] public double StartTime;
        [NonSerialized] public double EndTime;
        [NonSerialized] public float Size;
        [NonSerialized] public float Opacity;
    }
}
