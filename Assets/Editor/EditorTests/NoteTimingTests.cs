using CCE.Data;
using NUnit.Framework;

namespace CCE.EditorTests
{
    public class NoteTimingTests
    {
        [Test]
        public void CalculatesTimingsSimple()
        {
            var chart = new Chart
            {
                TimeBase = 480,
                TempoList =
                {
                    new Tempo { Tick = 0, Value = Tempo.ComputeValueFromBpm(120) },
                },
                PageList =
                {
                    new Page { StartTick = 0, EndTick = 480, ScanLineDirection = 1 },
                    new Page { StartTick = 480, EndTick = 960, ScanLineDirection = 1 },
                },
                NoteList =
                {
                    new Note { ID = 1, Tick = 240, PageIndex = 0 },
                    new Note { ID = 2, Tick = 480, PageIndex = 1 }
                }
            };

            _ = new NoteTimingCalculator(chart);

            Assert.AreEqual(0.5, chart.NoteList[0].Time, 1e-6);
            Assert.AreEqual(1.0, chart.NoteList[1].Time, 1e-6);
        }

        [Test]
        public void CalculateTimingsThrowsWhenFirstTempoNotAtZero()
        {
            var chart = new Chart
            {
                TimeBase = 480,
                TempoList =
                {
                    new Tempo { Tick = 120, Value = Tempo.ComputeValueFromBpm(120) },
                },
                PageList =
                {
                    new Page { StartTick = 0, EndTick = 480, ScanLineDirection = 1 },
                },
            };

            Assert.Throws<System.Exception>(() => new NoteTimingCalculator(chart));
        }

        [TestCase(120, TestName = "CalculatesPageActualStartTickOverlappingPages")]
        [TestCase(240, TestName = "CalculatesPageActualStartTickNonOverlappingPages")]
        public void CalculatesPageActualStartTick(int secondPageStartTick)
        {
            var chart = new Chart
            {
                TimeBase = 480,
                TempoList =
                {
                    new Tempo { Tick = 0, Value = Tempo.ComputeValueFromBpm(120) },
                },
                PageList =
                {
                    new Page { StartTick = 0, EndTick = 240, ScanLineDirection = 1 },
                    new Page { StartTick = secondPageStartTick, EndTick = 960, ScanLineDirection = 1 },
                },
            };

            _ = new NoteTimingCalculator(chart);

            Assert.AreEqual(0, chart.PageList[0].ActualStartTick);
            Assert.AreEqual(240, chart.PageList[1].ActualStartTick);
        }

        [TestCase(120, TestName = "CalculatesPageTimesOverlappingPages")]
        [TestCase(240, TestName = "CalculatesPageTimesNonOverlappingPages")]
        public void CalculatesPageTimes(int secondPageStartTick)
        {
            var chart = new Chart
            {
                TimeBase = 480,
                TempoList =
                {
                    new Tempo { Tick = 0, Value = Tempo.ComputeValueFromBpm(120) },
                },
                PageList =
                {
                    new Page { StartTick = 0, EndTick = 240, ScanLineDirection = 1 },
                    new Page { StartTick = secondPageStartTick, EndTick = 480, ScanLineDirection = 1 },
                },
            };

            _ = new NoteTimingCalculator(chart);

            Assert.AreEqual(0, chart.PageList[0].ActualStartTime);
            Assert.AreEqual(0.5, chart.PageList[0].EndTime, 1e-6);
            Assert.AreEqual(0.5, chart.PageList[1].ActualStartTime, 1e-6);
            Assert.AreEqual(1.0, chart.PageList[1].EndTime, 1e-6);
        }

        [TestCase(120, TestName = "CalculatesPageTimesTempoChangeOverlapping")]
        [TestCase(240, TestName = "CalculatesPageTimesTempoChangeNonOverlapping")]
        public void CalculatesPageTimesTempoChange(int secondPageStartTick)
        {
            var chart = new Chart
            {
                TimeBase = 480,
                TempoList =
                {
                    new Tempo { Tick = 0, Value = Tempo.ComputeValueFromBpm(120) },
                    new Tempo { Tick = 120, Value = Tempo.ComputeValueFromBpm(240) },
                    new Tempo { Tick = 200, Value = Tempo.ComputeValueFromBpm(60) },
                    new Tempo { Tick = 240, Value = Tempo.ComputeValueFromBpm(120) },
                },
                PageList =
                {
                    new Page { StartTick = 0, EndTick = 240, ScanLineDirection = 1 },
                    new Page { StartTick = secondPageStartTick, EndTick = 480, ScanLineDirection = 1 },
                },
            };

            _ = new NoteTimingCalculator(chart);

            Assert.AreEqual(0, chart.PageList[0].ActualStartTime);
            Assert.AreEqual(0.5, chart.PageList[0].EndTime, 1e-6);
            Assert.AreEqual(0.5, chart.PageList[1].ActualStartTime, 1e-6);
            Assert.AreEqual(1.0, chart.PageList[1].EndTime, 1e-6);
        }

        [Test]
        public void CalculatesNoteTimesWithTempoChanges()
        {
            var chart = new Chart
            {
                TimeBase = 480,
                TempoList =
                {
                    new Tempo { Tick = 0, Value = Tempo.ComputeValueFromBpm(120) },
                    new Tempo { Tick = 120, Value = Tempo.ComputeValueFromBpm(240) },
                    new Tempo { Tick = 200, Value = Tempo.ComputeValueFromBpm(60) },
                    new Tempo { Tick = 240, Value = Tempo.ComputeValueFromBpm(120) },
                },
                PageList =
                {
                    new Page { StartTick = 0, EndTick = 480, ScanLineDirection = 1 },
                },
                NoteList =
                {
                    new Note { ID = 1, Tick = 60, PageIndex = 0 },
                    new Note { ID = 2, Tick = 180, PageIndex = 0 },
                    new Note { ID = 3, Tick = 220, PageIndex = 0 },
                    new Note { ID = 4, Tick = 300, PageIndex = 0 },
                }
            };

            _ = new NoteTimingCalculator(chart);

            Assert.AreEqual(0.125, chart.NoteList[0].Time, 1e-6);
            Assert.AreEqual(0.3125, chart.NoteList[1].Time, 1e-6);
            Assert.AreEqual(0.41666666666666, chart.NoteList[2].Time, 1e-6);
            Assert.AreEqual(0.625, chart.NoteList[3].Time, 1e-6);
        }

        [Test]
        public void CalculatesHoldNoteTimesWithTempoChanges()
        {
            var chart = new Chart
            {
                TimeBase = 480,
                TempoList =
                {
                    new Tempo { Tick = 0, Value = Tempo.ComputeValueFromBpm(120) },
                    new Tempo { Tick = 120, Value = Tempo.ComputeValueFromBpm(240) },
                    new Tempo { Tick = 200, Value = Tempo.ComputeValueFromBpm(60) },
                    new Tempo { Tick = 240, Value = Tempo.ComputeValueFromBpm(120) },
                },
                PageList =
                {
                    new Page { StartTick = 0, EndTick = 480, ScanLineDirection = 1 },
                },
                NoteList =
                {
                    new Note { ID = 1, Type = 1, Tick = 60, PageIndex = 0, HoldTick = 140 },
                    new Note { ID = 2, Type = 2, Tick = 60, PageIndex = 0, HoldTick = 420 },
                    new Note { ID = 3, Type = 1, Tick = 240, PageIndex = 0, HoldTick = 240 },
                }
            };

            _ = new NoteTimingCalculator(chart);

            Assert.AreEqual(0.125, chart.NoteList[0].Time, 1e-6);
            Assert.AreEqual(0.20833333333, chart.NoteList[0].HoldTime, 1e-6);
            Assert.AreEqual(0.125, chart.NoteList[1].Time, 1e-6);
            Assert.AreEqual(0.875, chart.NoteList[1].HoldTime, 1e-6);
            Assert.AreEqual(0.5, chart.NoteList[2].Time, 1e-6);
            Assert.AreEqual(0.5, chart.NoteList[2].HoldTime, 1e-6);
        }
    }
}