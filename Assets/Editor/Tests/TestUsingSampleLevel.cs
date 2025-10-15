using System.Collections;
using System.IO;
using CCE.Core;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace CCE.Tests
{
    public class TestUsingSampleLevel
    {
        protected string TestLevelPath;
            
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            SceneNavigator.NavigateToFileSelect();
            yield return null;

            var levelsPath = GlobalState.Config.LevelStoragePath;
            TestLevelPath = Path.Combine(levelsPath, "chovvy.test");
            if (Directory.Exists(TestLevelPath)) yield break;

            TestUtils.ImportSampleLevel(levelsPath);
            Assert.Fail("Sample level was not found and it was loaded. Please re-run the test.");
        }

        [TearDown]
        public void TearDown()
        {
            TestUtils.AssureSampleLevelIntegrity();
        }
    }
}