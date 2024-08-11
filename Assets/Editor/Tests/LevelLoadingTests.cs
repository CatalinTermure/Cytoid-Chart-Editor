using System.Collections;
using System.IO;
using CCE.Core;
using CCE.LevelLoading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

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
            if (!Directory.Exists(Path.Combine(levelsPath, "chovvy.test")))
            {
                ImportSampleLevel(levelsPath);
                Assert.Fail("Sample level was not found and it was loaded. Please re-run the test.");
            }
        }

        private static void ImportSampleLevel(string levelsPath)
        {
            var sourcePath = Path.Combine(Application.dataPath, "Editor", "Resources", "chovvy.test");
            if (!Directory.Exists(sourcePath))
            {
                throw new DirectoryNotFoundException("Sample level not found at: " + sourcePath);
            }

            var destinationPath = Path.Combine(levelsPath, "chovvy.test");
            if (Directory.Exists(destinationPath))
            {
                Directory.Delete(destinationPath, true);
            }

            Directory.CreateDirectory(destinationPath);
            foreach (var file in Directory.GetFiles(sourcePath))
            {
                if (Path.GetExtension(file) == ".meta") continue;
                File.Copy(file, Path.Combine(destinationPath, Path.GetFileName(file)));
            }
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

        [UnityTest]
        public IEnumerator SearchShowsSampleLevel()
        {
            var levelList = GameObject.Find("Level List").GetComponent<LevelList>();
            levelList.Query("chovvy.test");

            yield return null;

            var levelNameObject = GameObject.Find("Level Name");
            Assert.IsNotNull(levelNameObject);
            var levelName = levelNameObject.GetComponent<Text>().text;
            Assert.AreEqual("Night Detective", levelName);
        }

        [UnityTest]
        public IEnumerator SearchSetsCurrentLevelID()
        {
            var levelList = GameObject.Find("Level List").GetComponent<LevelList>();
            levelList.Query("chovvy.test");

            yield return null;

            var levelIDObject = GameObject.Find("Current Level ID");
            Assert.IsNotNull(levelIDObject);
            var levelID = levelIDObject.GetComponent<Text>().text;
            Assert.AreEqual("chovvy.test", levelID);
        }

        [UnityTest]
        public IEnumerator SampleLevelLoads()
        {
            var levelList = GameObject.Find("Level List").GetComponent<LevelList>();
            levelList.Query("chovvy.test");

            yield return new WaitForSeconds(1);

            var extremeChartCard = GameObject.Find("Extreme Chart Card");
            Assert.IsNotNull(extremeChartCard);
            var actualChartCard = extremeChartCard.transform.Find("Actual Card");
            var chartCardButton = actualChartCard.GetComponent<Button>();
            chartCardButton.onClick.Invoke();

            yield return null;

            Assert.AreEqual("MainScene", SceneManager.GetActiveScene().name);
        }
    }
}