using System;
using System.Collections;
using System.IO;
using System.Linq;
using CCE.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CCE.Tests
{
    public class LevelLoadingTests
    {
        [UnitySetUp]
        public IEnumerator SetUpScene()
        {
            SceneNavigator.NavigateToFileSelect();
            yield return null;

            var levelsPath = GlobalState.Config.LevelStoragePath;
            if (!Directory.EnumerateFileSystemEntries(levelsPath).Any())
            {
                LoadSampleLevels();
            }
        }

        private static void LoadSampleLevels()
        {
            throw new NotImplementedException(
                "You have no levels in your level storage path. Please import some levels. " +
                "Automatic sample level loading is not implemented yet.");
        }

        [Test]
        public void LevelListExists()
        {
            var levelList = GameObject.Find("Level List");
            Assert.IsNotNull(levelList);
        }


        [Test]
        public void LevelListNotEmpty()
        {
            var levelList = GameObject.Find("Level List");
            Assert.Greater(levelList.transform.childCount, 0);
        }
    }
}