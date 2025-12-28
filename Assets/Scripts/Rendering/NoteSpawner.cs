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
        private List<FlickNoteInfo> _flickNotes;
        private List<DragChildNoteInfo> _dragChildNotes;

        private const float DRAG_CHILD_SIZE_MULTIPLIER = 0.65f;

        public NoteSpawner(ChartObjectPool chartObjectPool, Chart chart)
        {
            _chartObjectPool = chartObjectPool;
            _chart = chart;
            _clickNotes = new List<ClickNoteInfo>();
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

        private void PopulateNoteInfo(GameObject noteObject, Note note)
        {
            if (note.Type == (int)NoteType.Flick)
            {
                var flickNoteInfo = noteObject.GetComponent<FlickNoteInfo>();
                flickNoteInfo.StartTime = note.Time - note.ApproachTime;
                flickNoteInfo.EndTime = note.Time;
                flickNoteInfo.Size = (float)note.ActualSize;
                flickNoteInfo.Opacity = (float)note.ActualOpacity;
                flickNoteInfo.X = (float)(note.X * 10.0 - 5.0);
                flickNoteInfo.Y = (float)(note.Y * 10.0 - 5.0);
                _flickNotes.Add(flickNoteInfo);
            }
            else if (note.Type == (int)NoteType.DragChild)
            {
                var dragChildNoteInfo = noteObject.GetComponent<DragChildNoteInfo>();
                dragChildNoteInfo.StartTime = note.Time - note.ApproachTime;
                dragChildNoteInfo.EndTime = note.Time;
                dragChildNoteInfo.Size = (float)note.ActualSize * DRAG_CHILD_SIZE_MULTIPLIER;
                dragChildNoteInfo.Opacity = (float)note.ActualOpacity;
                dragChildNoteInfo.X = (float)(note.X * 10.0 - 5.0);
                dragChildNoteInfo.Y = (float)(note.Y * 10.0 - 5.0);
                _dragChildNotes.Add(dragChildNoteInfo);
            }
            else
            {
                var clickNoteInfo = noteObject.GetComponent<ClickNoteInfo>();
                clickNoteInfo.StartTime = note.Time - note.ApproachTime;
                clickNoteInfo.EndTime = note.Time;
                clickNoteInfo.Size = (float)note.ActualSize;
                clickNoteInfo.Opacity = (float)note.ActualOpacity;
                clickNoteInfo.X = (float)(note.X * 10.0 - 5.0);
                clickNoteInfo.Y = (float)(note.Y * 10.0 - 5.0);
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
        }
    }
}
