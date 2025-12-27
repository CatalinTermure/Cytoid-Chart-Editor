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
            throw new System.NotImplementedException();
        }
    }
}
