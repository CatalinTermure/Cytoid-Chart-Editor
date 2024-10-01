using System.Collections;
using System.IO;
using CCE.Core;
using CCE.Data;
using CCE.Game;
using CCE.LevelLoading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace CCE.Tests
{
    public class LevelEditingTests : TestUsingSampleLevel
    {
        [UnityTest]
        public IEnumerator CanAddClickNote()
        {
            yield return TestUtils.LoadSampleLevel();
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