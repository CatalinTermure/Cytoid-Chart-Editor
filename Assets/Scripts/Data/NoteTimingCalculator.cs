namespace CCE.Data
{
    public class NoteTimingCalculator : IChartChangedListener
    {
        private readonly Chart _chart;
        private const int MICROSECONDS_PER_SECOND = 1_000_000;

        public NoteTimingCalculator(Chart chart)
        {
            _chart = chart;

            CalculateTimings();
        }

        private void CalculateTimings()
        {
            CalculateTempoTimes();
            CalculatePageTimes();
            CalculateNoteTimes();
        }

        private void CalculateTempoTimes()
        {
            var tempos = _chart.TempoList;
            if (tempos[0].Tick != 0)
            {
                throw new System.Exception("First tempo must start at tick 0");
            }
            tempos[0].Time = 0;
            double currentTime = 0;

            for (int i = 0; i < tempos.Count - 1; i++)
            {
                var currentTempo = tempos[i];
                var nextTempo = tempos[i + 1];

                // Duration (in seconds) = (microseconds / beat) * ticks / (ticks / beat) / (microseconds / second)
                double duration = (double)currentTempo.Value * (nextTempo.Tick - currentTempo.Tick) / _chart.TimeBase / MICROSECONDS_PER_SECOND;
                currentTime += duration;
                nextTempo.Time = currentTime;
            }
        }

        private void CalculatePageTimes()
        {
            var pages = _chart.PageList;
            var tempos = _chart.TempoList;
            int tempoIndex = 0;

            for (int i = 0; i < pages.Count; i++)
            {
                var page = pages[i];

                // Link pages together
                if (i > 0)
                {
                    page.ActualStartTick = pages[i - 1].EndTick;
                    page.ActualStartTime = pages[i - 1].EndTime;
                }
                else
                {
                    page.ActualStartTick = 0;
                    page.ActualStartTime = 0;
                }

                // Calculate StartTime
                // Advance tempo index to the tempo active at page.StartTick
                while (tempoIndex + 1 < tempos.Count && tempos[tempoIndex + 1].Tick <= page.StartTick)
                {
                    tempoIndex++;
                }
                page.StartTime = CalculateTimeAtTick(page.StartTick, tempos[tempoIndex]);

                // Calculate EndTime
                // Use a temporary index to scan forward for the end tick, so we don't mess up the main index
                int endTempoIndex = tempoIndex;
                while (endTempoIndex + 1 < tempos.Count && tempos[endTempoIndex + 1].Tick <= page.EndTick)
                {
                    endTempoIndex++;
                }
                page.EndTime = CalculateTimeAtTick(page.EndTick, tempos[endTempoIndex]);
            }
        }

        private void CalculateNoteTimes()
        {
            var notes = _chart.NoteList;
            var tempos = _chart.TempoList;
            int tempoIndex = 0;

            foreach (var note in notes)
            {
                // Advance tempo index to the tempo active at note.Tick
                // Since notes are sorted by Tick, this loop runs O(T) times total across all notes
                while (tempoIndex + 1 < tempos.Count && tempos[tempoIndex + 1].Tick <= note.Tick)
                {
                    tempoIndex++;
                }

                note.Time = CalculateTimeAtTick(note.Tick, tempos[tempoIndex]);

                // Calculate Hold Time
                if (note.Type == 1 || note.Type == 2) // Hold or LongHold
                {
                    int holdEndTick = note.Tick + note.HoldTick;

                    // Find tempo for hold end. Start searching from current tempoIndex.
                    int holdEndTempoIndex = tempoIndex;
                    while (holdEndTempoIndex + 1 < tempos.Count && tempos[holdEndTempoIndex + 1].Tick <= holdEndTick)
                    {
                        holdEndTempoIndex++;
                    }

                    double holdEndTime = CalculateTimeAtTick(holdEndTick, tempos[holdEndTempoIndex]);
                    note.HoldTime = holdEndTime - note.Time;
                }
                else
                {
                    note.HoldTime = 0;
                }
            }
        }

        private void CalculateSingleNoteTimings(Note note)
        {
            var tempo = _chart.TempoList.FindLast(t => t.Tick <= note.Tick) ?? _chart.TempoList[0];
            note.Time = CalculateTimeAtTick(note.Tick, tempo);

            if (note.Type == 1 || note.Type == 2)
            {
                int holdEndTick = note.Tick + note.HoldTick;
                var endTempo = _chart.TempoList.FindLast(t => t.Tick <= holdEndTick) ?? _chart.TempoList[0];
                note.HoldTime = CalculateTimeAtTick(holdEndTick, endTempo) - note.Time;
            }
            else
            {
                note.HoldTime = 0;
            }
        }

        public void OnNoteAdded(Note note)
        {
            CalculateSingleNoteTimings(note);
        }

        public void OnNoteChanged(Note note)
        {
            CalculateSingleNoteTimings(note);
        }

        public void OnNoteRemoved(int noteId)
        {
            // Nothing to do
        }

        public void OnPageDirectionModified(Page page)
        {
            // Nothing to do
        }

        public void OnTempoAdded(Tempo tempo)
        {
            CalculateTimings();
        }

        public void OnTempoChanged(Tempo tempo)
        {
            CalculateTimings();
        }

        public void OnTempoRemoved(Tempo tempo)
        {
            CalculateTimings();
        }

        private double CalculateTimeAtTick(int tick, Tempo tempo)
        {
            return tempo.Time + (double)tempo.Value * (tick - tempo.Tick) / _chart.TimeBase / MICROSECONDS_PER_SECOND;
        }
    }
}