using CCE.Data;
using CCE.Rendering;
using CCE.Rendering.Notes;
using NUnit.Framework;
using UnityEngine;

namespace CCE.Tests.Rendering
{
    public class NoteSpawnerTests
    {
        private class ChartObjectPoolWithTracking : ChartObjectPool
        {
            public int GetNoteCallCount = 0;
            public int ReturnToPoolCallCount = 0;
            public NoteType LastRequestedType;

            public ChartObjectPoolWithTracking() : base(new NotePrefabs
            {
                ClickNote = Resources.Load<GameObject>("Click Note new"),
                FlickNote = Resources.Load<GameObject>("Flick Note New"),
                DragHeadNote = Resources.Load<GameObject>("Click Note New"),
                DragChildNote = Resources.Load<GameObject>("Drag Child New"),
                CDragHeadNote = Resources.Load<GameObject>("Click Note New"),
                HoldNote = Resources.Load<GameObject>("Hold Note New"),
                LongHoldNote = Resources.Load<GameObject>("Click Note New"),
            })
            { }

            public override GameObject GetNote(NoteType type)
            {
                GetNoteCallCount++;
                LastRequestedType = type;
                return base.GetNote(type);
            }

            public override void ReturnToPool(GameObject obj, NoteType type)
            {
                ReturnToPoolCallCount++;
                base.ReturnToPool(obj, type);
            }
        }

        [Test]
        public void UpdateTime_SpawnsClickNote_WhenInRange()
        {
            var chart = new Chart();
            var note = new Note
            {
                Type = (int)NoteType.Click,
                Time = 2.0,
                ApproachTime = 1.0,
                ActualSize = 1.5,
                ActualOpacity = 0.8,
                X = 0.6,
                Y = 0.7,
                PageIndex = 0
            };
            chart.NoteList.Add(note);
            chart.PageList.Add(new Page { ScanLineDirection = 1 });
            var pool = new ChartObjectPoolWithTracking();
            var spawner = new NoteSpawner(pool, chart);

            spawner.UpdateTime(1.5);

            var activeNotes = spawner.GetClickNotes();
            Assert.AreEqual(1, activeNotes.Count);
            Assert.AreEqual(1, pool.GetNoteCallCount);
            var spawnedInfo = activeNotes[0];
            Assert.AreEqual(1.0f, spawnedInfo.IntroTime, 1e-6f);
            Assert.AreEqual(2.0f, spawnedInfo.Time, 1e-6f);
            Assert.AreEqual(1.5f, spawnedInfo.Size, 1e-6f);
            Assert.AreEqual(0.8f, spawnedInfo.Opacity, 1e-6f);
            Assert.AreEqual(1.0f, spawnedInfo.X, 1e-6f);
            Assert.AreEqual(2.0f, spawnedInfo.Y, 1e-6f);
        }

        [Test]
        public void UpdateTime_SpawnsFlickNote_WhenInRange()
        {
            var chart = new Chart();
            var note = new Note
            {
                Type = (int)NoteType.Flick,
                Time = 2.0,
                ApproachTime = 1.0,
                ActualSize = 1.5,
                ActualOpacity = 0.8,
                X = 0.6,
                Y = 0.7,
                PageIndex = 0
            };
            chart.NoteList.Add(note);
            chart.PageList.Add(new Page { ScanLineDirection = 1 });
            var pool = new ChartObjectPoolWithTracking();
            var spawner = new NoteSpawner(pool, chart);

            spawner.UpdateTime(1.5);

            var activeNotes = spawner.GetFlickNotes();
            Assert.AreEqual(1, activeNotes.Count);
            Assert.AreEqual(1, pool.GetNoteCallCount);
            var spawnedInfo = activeNotes[0];
            Assert.AreEqual(1.0f, spawnedInfo.IntroTime, 1e-6f);
            Assert.AreEqual(2.0f, spawnedInfo.Time, 1e-6f);
            Assert.AreEqual(1.5f, spawnedInfo.Size, 1e-6f);
            Assert.AreEqual(0.8f, spawnedInfo.Opacity, 1e-6f);
            Assert.AreEqual(1.0f, spawnedInfo.X, 1e-6f);
            Assert.AreEqual(2.0f, spawnedInfo.Y, 1e-6f);
        }

        [Test]
        public void UpdateTime_SpawnsDragChildNote_WhenInRange()
        {
            var chart = new Chart();
            var note = new Note
            {
                Type = (int)NoteType.DragChild,
                Time = 2.0,
                ApproachTime = 1.0,
                ActualSize = 1.5,
                ActualOpacity = 0.8,
                X = 0.6,
                Y = 0.7
            };
            chart.NoteList.Add(note);
            var pool = new ChartObjectPoolWithTracking();
            var spawner = new NoteSpawner(pool, chart);

            spawner.UpdateTime(1.5);

            var activeNotes = spawner.GetDragChildNotes();
            Assert.AreEqual(1, activeNotes.Count);
            Assert.AreEqual(1, pool.GetNoteCallCount);
            var spawnedInfo = activeNotes[0];
            Assert.AreEqual(1.0f, spawnedInfo.IntroTime, 1e-6f);
            Assert.AreEqual(2.0f, spawnedInfo.Time, 1e-6f);
            Assert.AreEqual(0.975f, spawnedInfo.Size, 1e-6f);
            Assert.AreEqual(0.8f, spawnedInfo.Opacity, 1e-6f);
            Assert.AreEqual(1.0f, spawnedInfo.X, 1e-6f);
            Assert.AreEqual(2.0f, spawnedInfo.Y, 1e-6f);
        }

        [Test]
        public void UpdateTime_DoesNotSpawn_WhenTooEarly()
        {
            var chart = new Chart();
            chart.NoteList.Add(new Note { Type = (int)NoteType.Click, Time = 2.0, ApproachTime = 1.0 });
            var pool = new ChartObjectPoolWithTracking();
            var spawner = new NoteSpawner(pool, chart);

            spawner.UpdateTime(0.9);

            Assert.AreEqual(0, spawner.GetClickNotes().Count);
            Assert.AreEqual(0, pool.GetNoteCallCount);
        }

        [Test]
        public void UpdateTime_Despawns_WhenTooLate()
        {
            var chart = new Chart();
            chart.NoteList.Add(new Note { Type = (int)NoteType.Click, Time = 2.0, ApproachTime = 1.0 });
            var pool = new ChartObjectPoolWithTracking();
            var spawner = new NoteSpawner(pool, chart);

            spawner.UpdateTime(2.1);

            Assert.AreEqual(0, spawner.GetClickNotes().Count);
        }

        [Test]
        public void UpdateTime_HoldNote_StaysActiveUntilHoldEnd()
        {
            var chart = new Chart();
            var holdNote = new Note
            {
                Type = (int)NoteType.Hold,
                Time = 2.0,
                ApproachTime = 1.0,
                HoldTime = 1.5,
                HoldTick = 100,
                PageIndex = 0
            };
            chart.NoteList.Add(holdNote);
            chart.PageList.Add(new Page { ScanLineDirection = 1, ActualStartTick = 0, EndTick = 400 });
            var pool = new ChartObjectPoolWithTracking();
            var spawner = new NoteSpawner(pool, chart);

            spawner.UpdateTime(3.0);

            Assert.AreEqual(1, spawner.GetHoldNotes().Count);
            Assert.AreEqual(NoteType.Hold, pool.LastRequestedType);
        }

        [Test]
        public void UpdateTime_DragChain_StaysActiveUntilChainEnd()
        {
            var chart = new Chart();
            var head = new Note { ID = 0, NextID = 1, Type = (int)NoteType.DragHead, Time = 2.0, ApproachTime = 1.0, PageIndex = 0 };
            var child1 = new Note { ID = 1, NextID = 2, Type = (int)NoteType.DragChild, Time = 2.5, PageIndex = 0 };
            var child2 = new Note { ID = 2, NextID = -1, Type = (int)NoteType.DragChild, Time = 3.0, PageIndex = 0 };
            chart.NoteList.Add(head);
            chart.NoteList.Add(child1);
            chart.NoteList.Add(child2);
            chart.PageList.Add(new Page { ScanLineDirection = 1 });
            var pool = new ChartObjectPoolWithTracking();
            var spawner = new NoteSpawner(pool, chart);

            spawner.UpdateTime(2.8);

            Assert.IsTrue(spawner.GetClickNotes().Count >= 1, "Drag Head should be active at 2.8s");
        }

        [Test]
        public void UpdateTime_SetsRingColor_FromNote()
        {
            var chart = new Chart();
            var note = new Note
            {
                Type = (int)NoteType.Click,
                Time = 2.0,
                ApproachTime = 1.0,
                RingColor = "#FF0000",
                PageIndex = 0
            };
            chart.NoteList.Add(note);
            chart.PageList.Add(new Page { ScanLineDirection = 1 });
            var pool = new ChartObjectPoolWithTracking();
            var spawner = new NoteSpawner(pool, chart);

            spawner.UpdateTime(1.5);

            var activeNotes = spawner.GetClickNotes();
            Assert.AreEqual(1, activeNotes.Count);
            var color = activeNotes[0].NoteRing.color;
            Assert.AreEqual(Color.red.r, color.r, 1e-6f);
            Assert.AreEqual(Color.red.g, color.g, 1e-6f);
            Assert.AreEqual(Color.red.b, color.b, 1e-6f);
        }

        [Test]
        public void UpdateTime_SetsRingColor_FromChart_WhenNoteRingColorMissing()
        {
            var chart = new Chart();
            chart.RingColor = "#00FF00";
            var note = new Note
            {
                Type = (int)NoteType.Click,
                Time = 2.0,
                ApproachTime = 1.0,
                PageIndex = 0
            };
            chart.NoteList.Add(note);
            chart.PageList.Add(new Page { ScanLineDirection = 1 });
            var pool = new ChartObjectPoolWithTracking();
            var spawner = new NoteSpawner(pool, chart);

            spawner.UpdateTime(1.5);

            var activeNotes = spawner.GetClickNotes();
            var color = activeNotes[0].NoteRing.color;
            Assert.AreEqual(Color.green.r, color.r, 1e-6f);
            Assert.AreEqual(Color.green.g, color.g, 1e-6f);
            Assert.AreEqual(Color.green.b, color.b, 1e-6f);
        }

        [Test]
        public void UpdateTime_SetsRingColor_Default_WhenAllMissing()
        {
            var chart = new Chart();
            var note = new Note
            {
                Type = (int)NoteType.Click,
                Time = 2.0,
                ApproachTime = 1.0,
                PageIndex = 0
            };
            chart.NoteList.Add(note);
            chart.PageList.Add(new Page { ScanLineDirection = 1 });
            var pool = new ChartObjectPoolWithTracking();
            var spawner = new NoteSpawner(pool, chart);

            spawner.UpdateTime(1.5);

            var activeNotes = spawner.GetClickNotes();
            var color = activeNotes[0].NoteRing.color;
            Assert.AreEqual(Color.white.r, color.r, 1e-6f);
            Assert.AreEqual(Color.white.g, color.g, 1e-6f);
            Assert.AreEqual(Color.white.b, color.b, 1e-6f);
        }

        [Test]
        public void UpdateTime_SetsFillColor_FromNote()
        {
            var chart = new Chart();
            var note = new Note
            {
                Type = (int)NoteType.Click,
                Time = 2.0,
                ApproachTime = 1.0,
                FillColor = "#0000FF"
            };
            chart.NoteList.Add(note);
            var pool = new ChartObjectPoolWithTracking();
            var spawner = new NoteSpawner(pool, chart);

            spawner.UpdateTime(1.5);

            var activeNotes = spawner.GetClickNotes();
            var color = activeNotes[0].NoteFill.color;
            Assert.AreEqual(Color.blue.r, color.r, 1e-6f);
            Assert.AreEqual(Color.blue.g, color.g, 1e-6f);
            Assert.AreEqual(Color.blue.b, color.b, 1e-6f);
        }

        [Test]
        public void UpdateTime_DragChild_UsesRingColorLogicForFill()
        {
            var chart = new Chart();
            chart.RingColor = "#FFFF00";
            var note = new Note
            {
                Type = (int)NoteType.DragChild,
                Time = 2.0,
                ApproachTime = 1.0,
                FillColor = "#FF0000",
                PageIndex = 0
            };
            chart.NoteList.Add(note);
            chart.PageList.Add(new Page { ScanLineDirection = 1 });
            var pool = new ChartObjectPoolWithTracking();
            var spawner = new NoteSpawner(pool, chart);

            spawner.UpdateTime(1.5);

            var activeNotes = spawner.GetDragChildNotes();
            var color = activeNotes[0].NoteFill.color;
            Assert.AreEqual(1.0f, color.r, 1e-6f);
            Assert.AreEqual(1.0f, color.g, 1e-6f);
        }

        [Test]
        public void UpdateTime_SetsFillColor_FromChart_WhenNoteFillMissing()
        {
            var chart = new Chart();
            chart.PageList.Add(new Page { ScanLineDirection = 1 });

            for (int i = 0; i < 12; i++) chart.FillColors[i] = "#00FFFF";

            var note = new Note
            {
                Type = (int)NoteType.Click,
                Time = 2.0,
                ApproachTime = 1.0,
                PageIndex = 0
            };
            chart.NoteList.Add(note);
            var pool = new ChartObjectPoolWithTracking();
            var spawner = new NoteSpawner(pool, chart);

            spawner.UpdateTime(1.5);

            var activeNotes = spawner.GetClickNotes();
            var color = activeNotes[0].NoteFill.color;
            Assert.AreEqual(0.0f, color.r, 1e-6f);
            Assert.AreEqual(1.0f, color.g, 1e-6f);
            Assert.AreEqual(1.0f, color.b, 1e-6f);
        }

        [Test]
        public void UpdateTime_SetsFillColor_Default_WhenAllMissing()
        {
            var chart = new Chart();
            var note = new Note
            {
                Type = (int)NoteType.Click,
                Time = 2.0,
                ApproachTime = 1.0,
                PageIndex = 0
            };
            chart.NoteList.Add(note);
            chart.PageList.Add(new Page { ScanLineDirection = 1 });
            var pool = new ChartObjectPoolWithTracking();
            var spawner = new NoteSpawner(pool, chart);

            spawner.UpdateTime(1.5);

            var activeNotes = spawner.GetClickNotes();
            var color = activeNotes[0].NoteFill.color;
            Assert.AreEqual(1.0f, color.r, 1e-6f);
            Assert.AreEqual(0.349f, color.g, 0.001f);
            Assert.AreEqual(0.392f, color.b, 0.001f);
        }
    }
}