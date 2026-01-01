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
        private List<LongHoldNoteInfo> _longHoldNotes;
        private List<FlickNoteInfo> _flickNotes;
        private List<DragChildNoteInfo> _dragChildNotes;
        private readonly IChartToScreenCoordinatesConverter _chartToScreenConverter;

        private const float LONG_HOLD_BODY_SIZE = 4.0f;

        public NoteSpawner(ChartObjectPool chartObjectPool, Chart chart,
                            IChartToScreenCoordinatesConverter chartToScreenConverter)
        {
            _chartObjectPool = chartObjectPool;
            _chart = chart;
            _clickNotes = new List<ClickNoteInfo>();
            _holdNotes = new List<HoldNoteInfo>();
            _longHoldNotes = new List<LongHoldNoteInfo>();
            _flickNotes = new List<FlickNoteInfo>();
            _dragChildNotes = new List<DragChildNoteInfo>();
            _chartToScreenConverter = chartToScreenConverter;
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


        public List<LongHoldNoteInfo> GetLongHoldNotes()
        {
            return _longHoldNotes;
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
                flickNoteInfo.Size = (float)note.ActualSize * _chartToScreenConverter.FlickNoteSize;
                flickNoteInfo.Opacity = (float)note.ActualOpacity;
                flickNoteInfo.X = _chartToScreenConverter.ScreenXFromChartX(note.X);
                flickNoteInfo.Y = _chartToScreenConverter.ScreenYFromChartY(note.Y);
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
                dragChildNoteInfo.Size = (float)note.ActualSize * _chartToScreenConverter.DragChildNoteSize;
                dragChildNoteInfo.Opacity = (float)note.ActualOpacity;
                dragChildNoteInfo.X = _chartToScreenConverter.ScreenXFromChartX(note.X);
                dragChildNoteInfo.Y = _chartToScreenConverter.ScreenYFromChartY(note.Y);
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
                holdNoteInfo.Size = (float)note.ActualSize * _chartToScreenConverter.HoldNoteSize;
                holdNoteInfo.Opacity = (float)note.ActualOpacity;
                holdNoteInfo.X = _chartToScreenConverter.ScreenXFromChartX(note.X);
                holdNoteInfo.Y = _chartToScreenConverter.ScreenYFromChartY(note.Y);
                Color fillColor = GetFillColor(note);
                holdNoteInfo.NoteFill.color = fillColor.WithAlpha(0.0f);
                holdNoteInfo.NoteRing.color = GetRingColor(note).WithAlpha(0.0f);
                holdNoteInfo.NoteCompletedBody.color = fillColor;
                holdNoteInfo.NoteCompletedBody.size = new Vector3(0.0f, 0.0f);
                holdNoteInfo.NoteBodyBackground.color = Color.white.WithAlpha(0.0f);
                holdNoteInfo.NoteBodyBackground.size = new Vector3(0.0f,
                        _chartToScreenConverter.ScreenSize * ((float)note.HoldTick / page.ActualPageSize));
                holdNoteInfo.NoteBodyTransform.localScale = new Vector2(0, 1.0f);
                if (page.ScanLineDirection < 0)
                {
                    holdNoteInfo.NoteBodyTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
                }
                else
                {
                    holdNoteInfo.NoteBodyTransform.localRotation = Quaternion.identity;
                }
                _holdNotes.Add(holdNoteInfo);
            }
            else if (note.Type == (int)NoteType.LongHold)
            {
                var longHoldNoteInfo = noteObject.GetComponent<LongHoldNoteInfo>();
                longHoldNoteInfo.IntroTime = note.Time - note.ApproachTime;
                longHoldNoteInfo.StartTime = note.Time;
                longHoldNoteInfo.EndTime = note.Time + note.HoldTime;
                longHoldNoteInfo.Size = (float)note.ActualSize * _chartToScreenConverter.LongHoldNoteSize;
                longHoldNoteInfo.Opacity = (float)note.ActualOpacity;
                longHoldNoteInfo.X = _chartToScreenConverter.ScreenXFromChartX(note.X);
                longHoldNoteInfo.Y = _chartToScreenConverter.ScreenYFromChartY(note.Y);
                Color fillColor = GetFillColor(note);
                longHoldNoteInfo.NoteFill.color = fillColor.WithAlpha(0.0f);
                longHoldNoteInfo.NoteRing.color = GetRingColor(note).WithAlpha(0.0f);
                longHoldNoteInfo.NoteCompletedBodyTop.color = fillColor;
                longHoldNoteInfo.NoteCompletedBodyTop.size = new Vector3(0.0f, 0.0f);
                longHoldNoteInfo.NoteCompletedBodyBottom.color = fillColor;
                longHoldNoteInfo.NoteCompletedBodyBottom.size = new Vector3(0.0f, 0.0f);
                longHoldNoteInfo.NoteBodyBackgroundTop.color = Color.white.WithAlpha(0.0f);
                longHoldNoteInfo.NoteBodyBackgroundTop.size = new Vector3(1.0f,
                        _chartToScreenConverter.ScreenSize * LONG_HOLD_BODY_SIZE);
                longHoldNoteInfo.NoteBodyBackgroundBottom.color = Color.white.WithAlpha(0.0f);
                longHoldNoteInfo.NoteBodyBackgroundBottom.size = new Vector3(1.0f,
                        _chartToScreenConverter.ScreenSize * LONG_HOLD_BODY_SIZE);
                longHoldNoteInfo.NoteBodyTransform.localScale = new Vector2(0, 1.0f);
                _longHoldNotes.Add(longHoldNoteInfo);
            }
            else
            {
                var clickNoteInfo = noteObject.GetComponent<ClickNoteInfo>();
                clickNoteInfo.IntroTime = note.Time - note.ApproachTime;
                clickNoteInfo.Time = note.Time;
                clickNoteInfo.Size = (float)note.ActualSize * _chartToScreenConverter.ClickNoteSize;
                clickNoteInfo.Opacity = (float)note.ActualOpacity;
                clickNoteInfo.X = _chartToScreenConverter.ScreenXFromChartX(note.X);
                clickNoteInfo.Y = _chartToScreenConverter.ScreenYFromChartY(note.Y);
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

            foreach (var longHoldNote in _longHoldNotes)
            {
                _chartObjectPool.ReturnToPool(longHoldNote.gameObject, NoteType.LongHold);
            }
            _longHoldNotes.Clear();
        }
    }
}
