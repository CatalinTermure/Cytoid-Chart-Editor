using CCE.Data;

namespace CCE.Rendering
{
    /// <summary>
    /// Calculates note approach time, note Y position, note opacity and note size for all notes
    /// in the chart.
    /// </summary>
    public class NoteVisualsCalculator : IChartChangedListener
    {
        private readonly Chart _chart;

        public NoteVisualsCalculator(Chart chart)
        {
            _chart = chart;

            CalculateNoteVisuals();
        }

        public void OnNoteAdded(Note note)
        {
            CalculateSingleNoteVisuals(note);
        }

        public void OnNoteChanged(Note note)
        {
            CalculateSingleNoteVisuals(note);
        }

        public void OnNoteRemoved(int noteId)
        {
            // Nothing to do
        }

        public void OnPageDirectionModified(Page page)
        {
            foreach (Note note in _chart.NoteList)
            {
                if (_chart.PageList[note.PageIndex] == page)
                {
                    CalculateSingleNoteVisuals(note);
                }
            }
        }

        public void OnTempoAdded(Tempo tempo)
        {
            CalculateNoteVisuals();
        }

        public void OnTempoChanged(Tempo tempo)
        {
            CalculateNoteVisuals();
        }

        public void OnTempoRemoved(Tempo tempo)
        {
            CalculateNoteVisuals();
        }

        private void CalculateNoteVisuals()
        {
            foreach (var note in _chart.NoteList)
            {
                CalculateSingleNoteVisuals(note);
            }
        }

        private void CalculateSingleNoteVisuals(Note note)
        {
            var pages = _chart.PageList;
            var notePage = pages[note.PageIndex];

            // Calculate note Y
            if (notePage.ScanLineDirection == 1) // Upward
            {
                note.Y = (double)(note.Tick - notePage.ActualStartTick) / (notePage.EndTick - notePage.ActualStartTick);
            }
            else // Downward
            {
                note.Y = 1.0 - (double)(note.Tick - notePage.ActualStartTick) / (notePage.EndTick - notePage.ActualStartTick);
            }

            // Calculate note approach time
            var noteSpeed = note.PageIndex == 0 ? 1.0 : CalculateNoteSpeed(note);
            noteSpeed *= note.ApproachRate;
            if (note.Type == (int)NoteType.DragHead || note.Type == (int)NoteType.DragChild
                || note.Type == (int)NoteType.CDragHead || note.Type == (int)NoteType.CDragChild)
            {
                note.ApproachTime = 1.175 / noteSpeed;
            }
            else
            {
                note.ApproachTime = 1.367 / noteSpeed;
            }

            // Calculate note opacity
            note.ActualOpacity = note.Opacity < 0 ? _chart.Opacity : note.Opacity;

            // Calculate note size
            note.ActualSize = note.Size < 0 ? _chart.Size : _chart.Size * note.Size;
        }

        // Taken straight from Cytoid source code
        private double CalculateNoteSpeed(Note note)
        {
            var page = _chart.PageList[note.PageIndex];
            var previousPage = _chart.PageList[note.PageIndex - 1];
            var pageRatio = (double)(note.Tick - page.ActualStartTick) / (page.EndTick - page.ActualStartTick);
            var tempo = (page.EndTime - page.ActualStartTime) * pageRatio + (previousPage.EndTime - previousPage.ActualStartTime) * (1.367f - pageRatio);
            return tempo >= 1.367 ? 1.0 : 1.367 / tempo;
        }
    }
}