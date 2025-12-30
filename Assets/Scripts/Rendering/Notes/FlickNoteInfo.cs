using System;
using UnityEngine;

namespace CCE.Rendering.Notes
{
    public class FlickNoteInfo : MonoBehaviour
    {
        public Transform NoteTransform;
        public Transform NoteFillTransform;
        public Transform LeftArrowTransform;
        public Transform RightArrowTransform;
        public SpriteRenderer NoteFill;
        public SpriteRenderer NoteRing;
        public SpriteRenderer LeftArrow;
        public SpriteRenderer RightArrow;
        [NonSerialized] public double IntroTime;
        [NonSerialized] public double Time;
        [NonSerialized] public float Size;
        [NonSerialized] public float Opacity;
        [NonSerialized] public float X;
        [NonSerialized] public float Y;
    }
}
