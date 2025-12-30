using System;
using System.Collections.Generic;
using CCE.Data;
using CCE.Rendering.Notes;
using UnityEngine;

namespace CCE.Rendering
{
    /// <summary>
    /// Manages the creation and destruction of notes.
    /// </summary>
    public class NoteSpawner : INoteProvider
    {
        private readonly ChartObjectPool _chartObjectPool;
        private readonly Chart _chart;
        private List<ClickNoteInfo> _clickNotes;
        private List<HoldNoteInfo> _holdNotes;
        private List<FlickNoteInfo> _flickNotes;
        private List<DragChildNoteInfo> _dragChildNotes;

        private const float DRAG_CHILD_SIZE_MULTIPLIER = 0.65f;

        public NoteSpawner(ChartObjectPool chartObjectPool, Chart chart)
        {
            _chartObjectPool = chartObjectPool;
            _chart = chart;
            _clickNotes = new List<ClickNoteInfo>();
            _holdNotes = new List<HoldNoteInfo>();
            _flickNotes = new List<FlickNoteInfo>();
            _dragChildNotes = new List<DragChildNoteInfo>();
        }

        /// <summary>
        /// Updates all the notes on screen to what should be displayed at a specific time.
        /// Much slower than <see cref="UpdateTimeIncremental"/>, but works regardless of the
        /// value of the time parameter.
        /// </summary>
        public void UpdateTime(double time)
        {
            ClearNotes();

            foreach (Note note in _chart.NoteList)
            {
                var noteEndTime = note.Time;
                if (note.Type == (int)NoteType.Hold || note.Type == (int)NoteType.LongHold)
                {
                    noteEndTime += note.HoldTime;
                }
                if (note.Type == (int)NoteType.DragHead || note.Type == (int)NoteType.CDragHead)
                {
                    noteEndTime = GetDragChain(note)[^1].Time;
                }

                if (time >= note.Time - note.ApproachTime && time <= noteEndTime)
                {
                    var noteObject = _chartObjectPool.GetNote((NoteType)note.Type);
                    PopulateNoteInfo(noteObject, note);
                    noteObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Incrementally updates the notes on screen to what should be displayed at a specific
        /// time. This is much faster than <see cref="UpdateTime"/> for values of time that are
        /// close to the previous update's time.
        /// </summary>
        public void UpdateTimeIncremental(double time)
        {
            throw new System.NotImplementedException();
        }

        public List<ClickNoteInfo> GetClickNotes()
        {
            return _clickNotes;
        }

        public List<HoldNoteInfo> GetHoldNotes()
        {
            return _holdNotes;
        }


        public List<FlickNoteInfo> GetFlickNotes()
        {
            return _flickNotes;
        }

        public List<DragChildNoteInfo> GetDragChildNotes()
        {
            return _dragChildNotes;
        }

        private List<Note> GetDragChain(Note headNote)
        {
            List<Note> dragChain = new() { headNote };
            var currentNote = headNote;
            while (currentNote.NextID != -1)
            {
                currentNote = _chart.NoteList[currentNote.NextID];
                dragChain.Add(currentNote);
            }
            return dragChain;
        }

        private Color GetRingColor(Note note)
        {
            if (!String.IsNullOrEmpty(note.RingColor))
            {
                return ColorExtensions.FromHex(note.RingColor);
            }
            if (!String.IsNullOrEmpty(_chart.RingColor))
            {
                return ColorExtensions.FromHex(_chart.RingColor);
            }
            return Color.white;
        }

        private Color GetFillColor(Note note)
        {
            if (note.Type == (int)NoteType.DragChild || note.Type == (int)NoteType.CDragChild)
            {
                return GetRingColor(note);
            }

            if (!String.IsNullOrEmpty(note.FillColor))
            {
                return ColorExtensions.FromHex(note.FillColor);
            }
            int colorIndex = Chart.ColorIndexByNoteType[note.Type] +
                (_chart.PageList[note.PageIndex].ScanLineDirection > 0 ? 1 : 0);
            if (!String.IsNullOrEmpty(_chart.FillColors[colorIndex]))
            {
                return ColorExtensions.FromHex(_chart.FillColors[colorIndex]);
            }
            return ColorExtensions.FromHex(Chart.DefaultFillColors[colorIndex]);
        }

        private void PopulateNoteInfo(GameObject noteObject, Note note)
        {
            if (note.Type == (int)NoteType.Flick)
            {
                var flickNoteInfo = noteObject.GetComponent<FlickNoteInfo>();
                flickNoteInfo.IntroTime = note.Time - note.ApproachTime;
                flickNoteInfo.Time = note.Time;
                flickNoteInfo.Size = (float)note.ActualSize;
                flickNoteInfo.Opacity = (float)note.ActualOpacity;
                flickNoteInfo.X = (float)(note.X * 10.0 - 5.0);
                flickNoteInfo.Y = (float)(note.Y * 10.0 - 5.0);
                flickNoteInfo.LeftArrow.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
                flickNoteInfo.RightArrow.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
                flickNoteInfo.NoteFill.color = GetFillColor(note).WithAlpha(0.0f);
                flickNoteInfo.NoteRing.color = GetRingColor(note).WithAlpha(0.0f);
                _flickNotes.Add(flickNoteInfo);
            }
            else if (note.Type == (int)NoteType.DragChild || note.Type == (int)NoteType.CDragChild)
            {
                var dragChildNoteInfo = noteObject.GetComponent<DragChildNoteInfo>();
                dragChildNoteInfo.IntroTime = note.Time - note.ApproachTime;
                dragChildNoteInfo.Time = note.Time;
                dragChildNoteInfo.Size = (float)note.ActualSize * DRAG_CHILD_SIZE_MULTIPLIER;
                dragChildNoteInfo.Opacity = (float)note.ActualOpacity;
                dragChildNoteInfo.X = (float)(note.X * 10.0 - 5.0);
                dragChildNoteInfo.Y = (float)(note.Y * 10.0 - 5.0);
                dragChildNoteInfo.NoteFill.color = GetFillColor(note).WithAlpha(0.0f);
                _dragChildNotes.Add(dragChildNoteInfo);
            }
            else if (note.Type == (int)NoteType.Hold)
            {
                var holdNoteInfo = noteObject.GetComponent<HoldNoteInfo>();
                var page = _chart.PageList[note.PageIndex];
                holdNoteInfo.IntroTime = note.Time - note.ApproachTime;
                holdNoteInfo.StartTime = note.Time;
                holdNoteInfo.EndTime = note.Time + note.HoldTime;
                holdNoteInfo.Size = (float)note.ActualSize;
                holdNoteInfo.Opacity = (float)note.ActualOpacity;
                holdNoteInfo.X = (float)(note.X * 10.0 - 5.0);
                holdNoteInfo.Y = (float)(note.Y * 10.0 - 5.0);
                holdNoteInfo.NoteFill.color = GetFillColor(note).WithAlpha(0.0f);
                holdNoteInfo.NoteRing.color = GetRingColor(note).WithAlpha(0.0f);
                holdNoteInfo.NoteCompletedBody.color = holdNoteInfo.NoteFill.color;
                holdNoteInfo.NoteCompletedBody.size = new Vector3(0.0f, 0.0f);
                holdNoteInfo.NoteBodyBackground.color = Color.white.WithAlpha(0.0f);
                holdNoteInfo.NoteBodyBackground.size = new Vector3(0.0f, 10.0f * note.HoldTick / page.ActualPageSize);
                holdNoteInfo.NoteBodyTransform.localScale = new Vector2(0, 1.0f);
                if (page.ScanLineDirection < 0)
                {
                    holdNoteInfo.NoteBodyTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
                }
                _holdNotes.Add(holdNoteInfo);
            }
            else
            {
                var clickNoteInfo = noteObject.GetComponent<ClickNoteInfo>();
                clickNoteInfo.IntroTime = note.Time - note.ApproachTime;
                clickNoteInfo.Time = note.Time;
                clickNoteInfo.Size = (float)note.ActualSize;
                clickNoteInfo.Opacity = (float)note.ActualOpacity;
                clickNoteInfo.X = (float)(note.X * 10.0 - 5.0);
                clickNoteInfo.Y = (float)(note.Y * 10.0 - 5.0);
                clickNoteInfo.NoteFill.color = GetFillColor(note).WithAlpha(0.0f);
                clickNoteInfo.NoteRing.color = GetRingColor(note).WithAlpha(0.0f);
                _clickNotes.Add(clickNoteInfo);
            }
        }

        private void ClearNotes()
        {
            foreach (var clickNote in _clickNotes)
            {
                _chartObjectPool.ReturnToPool(clickNote.gameObject, NoteType.Click);
            }
            _clickNotes.Clear();

            foreach (var flickNote in _flickNotes)
            {
                _chartObjectPool.ReturnToPool(flickNote.gameObject, NoteType.Flick);
            }
            _flickNotes.Clear();

            foreach (var dragChildNote in _dragChildNotes)
            {
                _chartObjectPool.ReturnToPool(dragChildNote.gameObject, NoteType.DragChild);
            }
            _dragChildNotes.Clear();

            foreach (var holdNote in _holdNotes)
            {
                _chartObjectPool.ReturnToPool(holdNote.gameObject, NoteType.Hold);
            }
            _holdNotes.Clear();
        }
    }
}
