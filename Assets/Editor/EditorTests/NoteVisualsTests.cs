using CCE.Data;
using CCE.Rendering;
using NUnit.Framework;
using System.Collections.Generic;

namespace CCE.EditorTests
{
    public class NoteVisualsTests
    {
        [Test]
        public void CalculatesVisuals_FirstPage_Upward_Click()
        {
            var chart = new Chart
            {
                Opacity = 0.5f,
                Size = 0.8f,
                PageList = new List<Page>
                {
                    new() { ScanLineDirection = 1, ActualStartTick = 0, EndTick = 100, ActualStartTime = 0, EndTime = 2 }
                },
                NoteList = new List<Note>
                {
                    new() { PageIndex = 0, Tick = 50, Type = (int)NoteType.Click, ApproachRate = 1.0, Opacity = -1, Size = -1 }
                }
            };

            _ = new NoteVisualsCalculator(chart);

            var note = chart.NoteList[0];
            Assert.AreEqual(0.5, note.Y, 0.0001, "Y position should be 0.5 for middle of upward page.");
            Assert.AreEqual(1.367, note.ApproachTime, 0.0001, "ApproachTime should be base 1.367 for Click on page 0.");
            Assert.AreEqual(0.5f, note.ActualOpacity, "Should fallback to chart opacity.");
            Assert.AreEqual(0.8f, note.ActualSize, "Should fallback to chart size.");
        }

        [Test]
        public void CalculatesVisuals_FirstPage_Downward_Drag()
        {
            var chart = new Chart
            {
                Opacity = 1f,
                Size = 1f,
                PageList = new List<Page>
                {
                    new() { ScanLineDirection = -1, ActualStartTick = 0, EndTick = 100, ActualStartTime = 0, EndTime = 2 }
                },
                NoteList = new List<Note>
                {
                    new() { PageIndex = 0, Tick = 25, Type = (int)NoteType.DragHead, ApproachRate = 2.0, Opacity = 0.9f, Size = 1.2f }
                }
            };

            _ = new NoteVisualsCalculator(chart);

            var note = chart.NoteList[0];
            Assert.AreEqual(0.75, note.Y, 0.0001, "Y position should be 0.75 (1.0 - 0.25) for downward page.");
            Assert.AreEqual(1.175 / 2.0, note.ApproachTime, 0.0001, "ApproachTime should be base 1.175 / AR 2.0 for Drag.");
            Assert.AreEqual(0.9f, note.ActualOpacity, "Should use note opacity.");
            Assert.AreEqual(1.2f, note.ActualSize, "Should use note size.");
        }

        [Test]
        public void CalculatesVisuals_SecondPage_HighTempo()
        {
            var chart = new Chart
            {
                PageList = new List<Page>
                {
                    new() { ActualStartTick = 0, EndTick = 100, ActualStartTime = 0, EndTime = 1.0 },
                    new() { ActualStartTick = 100, EndTick = 200, ActualStartTime = 1.0, EndTime = 2.0 }
                },
                NoteList = new List<Note>
                {
                    new() { PageIndex = 1, Tick = 100, Type = (int)NoteType.Click, ApproachRate = 1.0, Opacity = 1, Size = 1 }
                }
            };

            _ = new NoteVisualsCalculator(chart);

            var note = chart.NoteList[0];
            Assert.AreEqual(1.367, note.ApproachTime, 0.0001);
        }

        [Test]
        public void CalculatesVisuals_SecondPage_LowTempo()
        {
            var chart = new Chart
            {
                PageList = new List<Page>
                {
                    new() { ActualStartTick = 0, EndTick = 100, ActualStartTime = 0, EndTime = 0.5 }, // Short previous page
                    new() { ActualStartTick = 100, EndTick = 200, ActualStartTime = 0.5, EndTime = 1.5 }
                },
                NoteList = new List<Note>
                {
                    new() { PageIndex = 1, Tick = 110, Type = (int)NoteType.Click, ApproachRate = 1.0, Opacity = 1, Size = 1 }
                }
            };

            _ = new NoteVisualsCalculator(chart);

            var note = chart.NoteList[0];
            Assert.AreEqual(0.7334999, note.ApproachTime, 0.0001);
        }
    }
}