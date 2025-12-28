using CCE.Data;
using CCE.Rendering;
using CCE.Rendering.Notes;
using NUnit.Framework;
using UnityEngine;

namespace CCE.Tests.Rendering
{
    public class NoteSpawnerTests
    {
        private class FakeChartObjectPool : ChartObjectPool
        {
            public int GetNoteCallCount = 0;
            public int ReturnToPoolCallCount = 0;
            public NoteType LastRequestedType;

            public FakeChartObjectPool() : base() { }

            public override GameObject GetNote(NoteType type)
            {
                GetNoteCallCount++;
                LastRequestedType = type;

                var go = new GameObject($"FakeNote_{type}");
                if (type == NoteType.Flick)
                {
                    go.AddComponent<FlickNoteInfo>();
                }
                else if (type == NoteType.DragChild)
                {
                    go.AddComponent<DragChildNoteInfo>();
                }
                else
                {
                    go.AddComponent<ClickNoteInfo>();
                }
                return go;
            }

            public override void ReturnToPool(GameObject obj, NoteType type)
            {
                ReturnToPoolCallCount++;
                Object.DestroyImmediate(obj);
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
                Y = 0.7
            };
            chart.NoteList.Add(note);
            var pool = new FakeChartObjectPool();
            var spawner = new NoteSpawner(pool, chart);

            spawner.UpdateTime(1.5);

            var activeNotes = spawner.GetClickNotes();
            Assert.AreEqual(1, activeNotes.Count);
            Assert.AreEqual(1, pool.GetNoteCallCount);
            var spawnedInfo = activeNotes[0];
            Assert.AreEqual(1.0f, spawnedInfo.StartTime);
            Assert.AreEqual(2.0f, spawnedInfo.EndTime);
            Assert.AreEqual(1.5f, spawnedInfo.Size);
            Assert.AreEqual(0.8f, spawnedInfo.Opacity);
            Assert.AreEqual(1.0f, spawnedInfo.X);
            Assert.AreEqual(2.0f, spawnedInfo.Y);
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
                Y = 0.7
            };
            chart.NoteList.Add(note);
            var pool = new FakeChartObjectPool();
            var spawner = new NoteSpawner(pool, chart);

            spawner.UpdateTime(1.5);

            var activeNotes = spawner.GetFlickNotes();
            Assert.AreEqual(1, activeNotes.Count);
            Assert.AreEqual(1, pool.GetNoteCallCount);
            var spawnedInfo = activeNotes[0];
            Assert.AreEqual(1.0f, spawnedInfo.StartTime);
            Assert.AreEqual(2.0f, spawnedInfo.EndTime);
            Assert.AreEqual(1.5f, spawnedInfo.Size);
            Assert.AreEqual(0.8f, spawnedInfo.Opacity);
            Assert.AreEqual(1.0f, spawnedInfo.X);
            Assert.AreEqual(2.0f, spawnedInfo.Y);
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
            var pool = new FakeChartObjectPool();
            var spawner = new NoteSpawner(pool, chart);

            spawner.UpdateTime(1.5);

            var activeNotes = spawner.GetDragChildNotes();
            Assert.AreEqual(1, activeNotes.Count);
            Assert.AreEqual(1, pool.GetNoteCallCount);
            var spawnedInfo = activeNotes[0];
            Assert.AreEqual(1.0f, spawnedInfo.StartTime);
            Assert.AreEqual(2.0f, spawnedInfo.EndTime);
            Assert.AreEqual(0.975f, spawnedInfo.Size);
            Assert.AreEqual(0.8f, spawnedInfo.Opacity);
            Assert.AreEqual(1.0f, spawnedInfo.X);
            Assert.AreEqual(2.0f, spawnedInfo.Y);
        }

        [Test]
        public void UpdateTime_DoesNotSpawn_WhenTooEarly()
        {
            var chart = new Chart();
            chart.NoteList.Add(new Note { Type = (int)NoteType.Click, Time = 2.0, ApproachTime = 1.0 });
            var pool = new FakeChartObjectPool();
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
            var pool = new FakeChartObjectPool();
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
                HoldTime = 1.5
            };
            chart.NoteList.Add(holdNote);
            var pool = new FakeChartObjectPool();
            var spawner = new NoteSpawner(pool, chart);

            spawner.UpdateTime(3.0);

            Assert.AreEqual(1, spawner.GetClickNotes().Count);
            Assert.AreEqual(NoteType.Hold, pool.LastRequestedType);
        }

        [Test]
        public void UpdateTime_DragChain_StaysActiveUntilChainEnd()
        {
            var chart = new Chart();
            var head = new Note { ID = 0, NextID = 1, Type = (int)NoteType.DragHead, Time = 2.0, ApproachTime = 1.0 };
            var child1 = new Note { ID = 1, NextID = 2, Type = (int)NoteType.DragChild, Time = 2.5 };
            var child2 = new Note { ID = 2, NextID = -1, Type = (int)NoteType.DragChild, Time = 3.0 };
            chart.NoteList.Add(head);
            chart.NoteList.Add(child1);
            chart.NoteList.Add(child2);
            var pool = new FakeChartObjectPool();
            var spawner = new NoteSpawner(pool, chart);

            spawner.UpdateTime(2.8);

            Assert.IsTrue(spawner.GetClickNotes().Count >= 1, "Drag Head should be active at 2.8s");
        }
    }
}