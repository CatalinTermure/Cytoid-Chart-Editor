using System;
using System.Collections.Generic;
using UnityEngine;

namespace CCE.Rendering.Notes
{
    public class DragHeadNoteInfo : MonoBehaviour
    {
        public Transform NoteTransform;
        public SpriteRenderer NoteFill;
        public SpriteRenderer NoteRing;
        [NonSerialized] public double IntroTime;
        [NonSerialized] public double StartTime;
        [NonSerialized] public double EndTime;
        [NonSerialized] public float Size;
        [NonSerialized] public float Opacity;
        [NonSerialized] public List<DragPathNode> DragPath;
    }
}
