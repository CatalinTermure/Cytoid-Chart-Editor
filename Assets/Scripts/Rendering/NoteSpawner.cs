using System.Collections.Generic;
using CCE.Data;
using CCE.Rendering.Notes;

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

        public NoteSpawner(ChartObjectPool chartObjectPool, Chart chart)
        {
            _chartObjectPool = chartObjectPool;
            _chart = chart;
            _clickNotes = new List<ClickNoteInfo>();
        }

        /// <summary>
        /// Updates all the notes on screen to what should be displayed at a specific time.
        /// Much slower than <see cref="UpdateTimeIncremental"/>, but works regardless of the
        /// value of the time parameter.
        /// </summary>
        public void UpdateTime(double time)
        {
            foreach (var clickNoteInfo in _clickNotes)
            {
                _chartObjectPool.ReturnToPool(clickNoteInfo.gameObject, NoteType.Click);
            }
            _clickNotes.Clear();

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
                    var clickNoteInfo = noteObject.GetComponent<ClickNoteInfo>();
                    clickNoteInfo.StartTime = note.Time - note.ApproachTime;
                    clickNoteInfo.EndTime = note.Time;
                    clickNoteInfo.Size = (float)note.ActualSize;
                    clickNoteInfo.Opacity = (float)note.ActualOpacity;
                    _clickNotes.Add(clickNoteInfo);
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
    }
}
