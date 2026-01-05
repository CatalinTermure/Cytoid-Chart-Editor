using UnityEngine;
using CCE.Data;
using System.Collections.Generic;
using System;

namespace CCE.Rendering
{
    /// <summary>
    /// Calculates note approach time, note Y position, note opacity and note size for all notes
    /// in the chart.
    /// </summary>
    public class NoteVisualsCalculator : IChartChangedListener
    {
        private readonly Chart _chart;
        private readonly List<Color> _defaultFillColors;
        private readonly Color _defaultRingColor;

        public NoteVisualsCalculator(Chart chart)
        {
            _chart = chart;

            if (!String.IsNullOrEmpty(_chart.RingColor))
            {
                _defaultRingColor = FromHex(_chart.RingColor);
            }
            else
            {
                _defaultRingColor = Color.white;
            }

            _defaultFillColors = new List<Color>();
            for (int i = 0; i < Chart.DefaultFillColors.Length; i++)
            {
                if (!String.IsNullOrEmpty(_chart.FillColors[i]))
                {
                    _defaultFillColors.Add(FromHex(_chart.FillColors[i]));
                }
                else
                {
                    _defaultFillColors.Add(FromHex(Chart.DefaultFillColors[i]));
                }
            }

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

            // Calculate note fill color
            int colorIndex = Chart.ColorIndexByNoteType[note.Type];
            if (notePage.ScanLineDirection == 1)
            {
                colorIndex += 1;
            }
            if (!String.IsNullOrEmpty(note.FillColor))
            {
                note.ActualFillColor = FromHex(note.FillColor);
            }
            else
            {
                note.ActualFillColor = _defaultFillColors[colorIndex];
            }

            // Calculate note ring color
            if (!String.IsNullOrEmpty(note.RingColor))
            {
                note.ActualRingColor = FromHex(note.RingColor);
            }
            else
            {
                note.ActualRingColor = _defaultRingColor;
            }

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

        private static Color FromHex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                return color;
            }
            return Color.white;
        }
    }
}