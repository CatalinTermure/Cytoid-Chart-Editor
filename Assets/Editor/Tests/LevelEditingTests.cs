using CCE.Core;
using CCE.Data;
using CCE.Game;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace CCE.Tests
{
    public class LevelEditingTests : LevelEditingTestSuite
    {
        [Test]
        public void CanAddClickNote()
        {
            GameLogic.AddNote(new Note
            {
                ApproachRate = 1,
                FillColor = null,
                Opacity = 1,
                PageIndex = 0,
                RingColor = null,
                Tick = 0,
                Type = (int)NoteType.Click,
                X = 0.5
            });

            Assert.AreEqual(1, GlobalState.CurrentChart.NoteList.Count);
            Assert.AreEqual(0, GlobalState.CurrentChart.NoteList[0].ID);
            Assert.AreEqual(0, GlobalState.CurrentChart.NoteList[0].PageIndex);
            Assert.AreEqual(0, GlobalState.CurrentChart.NoteList[0].Tick);
            Assert.AreEqual(0.5, GlobalState.CurrentChart.NoteList[0].X, 0.0001);
        }
    }
}