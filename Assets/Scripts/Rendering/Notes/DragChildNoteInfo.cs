using System;
using UnityEngine;

namespace CCE.Rendering.Notes
{
    public class DragChildNoteInfo : MonoBehaviour
    {
        public Transform NoteTransform;
        public SpriteRenderer NoteFill;
        [NonSerialized] public double StartTime;
        [NonSerialized] public double EndTime;
        [NonSerialized] public float Size;
        [NonSerialized] public float Opacity;
        [NonSerialized] public float X;
        [NonSerialized] public float Y;
    }
}
