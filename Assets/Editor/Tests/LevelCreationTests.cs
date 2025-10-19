using System.IO;
using CCE.LevelLoading;
using NUnit.Framework;
using UnityEngine;

namespace CCE.Tests
{
    public class LevelCreationTests : TestUsingSampleLevel
    {
        [Test]
        public void ImportButtonExists()
        {
            var importButton = GameObject.Find("Import Button");
            Assert.IsNotNull(importButton);
        }

        [Test]
        public void LevelListBehaviourExists()
        {
            var levelListBehaviours =
                Object.FindObjectsByType<LevelListBehaviour>(FindObjectsSortMode.None);
            Assert.AreEqual(1, levelListBehaviours.Length);
        }

        [Test]
        public void ShowLevelMetadataPopupCreatesObject()
        {
            var levelListBehaviour =
                Object.FindAnyObjectByType<LevelListBehaviour>();
            var testAudioPath = Path.Combine(TestUtils.SampleLevelPath, "test-audio.mp3");
            levelListBehaviour.ShowLevelMetadataPopup(testAudioPath);
            var levelMetadataPopup = Object.FindAnyObjectByType<LevelMetadataPopupController>();
            Assert.IsNotNull(levelMetadataPopup);
        }
    }
}