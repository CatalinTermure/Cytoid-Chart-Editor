using CCE.Data;
using CCE.Rendering;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

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

        [Test]
        public void CalculatesVisuals_SetsColors()
        {
            // Setup chart with specific ring color and fill colors
            var chart = new Chart
            {
                RingColor = "#111111",
                PageList = new List<Page> { new() { ScanLineDirection = 1 } },
                NoteList = new List<Note>()
            };
            
            // Note 0: Explicit Fill and Ring Color
            chart.NoteList.Add(new Note 
            { 
                Type = (int)NoteType.Click, 
                FillColor = "#FF0000", 
                RingColor = "#00FF00",
                PageIndex = 0
            });

            // Note 1: Implicit Colors (Fallback to Chart/Defaults), Scanline Up (Dir 1)
            // ColorIndex for Click is usually set in Chart.ColorIndexByNoteType.
            chart.NoteList.Add(new Note
            {
                Type = (int)NoteType.Click,
                PageIndex = 0
            });

            // Note 2: Implicit Colors, Scanline Down (Dir -1) -> No +1 to index
            chart.PageList.Add(new Page { ScanLineDirection = -1 });
            chart.NoteList.Add(new Note
            {
                Type = (int)NoteType.Click,
                PageIndex = 1
            });

            // Note 3: Invalid/Missing Chart Ring Color -> Fallback to White
            var chart2 = new Chart();
            chart2.PageList.Add(new Page());
            chart2.NoteList.Add(new Note 
            { 
                Type = (int)NoteType.Click, 
                PageIndex = 0 
            });

            // Run calculator on chart 1
            _ = new NoteVisualsCalculator(chart);

            // Assert Note 0
            var note0 = chart.NoteList[0];
            Assert.AreEqual(Color.red, note0.ActualFillColor, "Explicit FillColor should be Red.");
            Assert.AreEqual(Color.green, note0.ActualRingColor, "Explicit RingColor should be Green.");

            // Assert Note 1
            var note1 = chart.NoteList[1];
            // Chart RingColor was #111111
            ColorUtility.TryParseHtmlString("#111111", out Color expectedRing);
            Assert.AreEqual(expectedRing, note1.ActualRingColor, "Implicit RingColor should use Chart.RingColor.");
            
            int clickIndex = Chart.ColorIndexByNoteType[(int)NoteType.Click];
            string expectedFillHexUp = Chart.DefaultFillColors[clickIndex + 1];
            ColorUtility.TryParseHtmlString(expectedFillHexUp, out Color expectedFillUp);
            Assert.AreEqual(expectedFillUp, note1.ActualFillColor, "Implicit FillColor (Up) should match default.");

            // Assert Note 2
            _ = new NoteVisualsCalculator(chart);
            var note2 = chart.NoteList[2];
            string expectedFillHexDown = Chart.DefaultFillColors[clickIndex];
            ColorUtility.TryParseHtmlString(expectedFillHexDown, out Color expectedFillDown);
            Assert.AreEqual(expectedFillDown, note2.ActualFillColor, "Implicit FillColor (Down) should match default.");

            // Run calculator on chart 2
            _ = new NoteVisualsCalculator(chart2);
            var note3 = chart2.NoteList[0];
            Assert.AreEqual(Color.white, note3.ActualRingColor, "Missing Chart RingColor should fallback to White.");
        }
    }
}