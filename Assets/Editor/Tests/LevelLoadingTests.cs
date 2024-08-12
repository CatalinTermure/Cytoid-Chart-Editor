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
        private static readonly string _sampleLevelPath =
            Path.Combine(Application.dataPath, "Editor", "Resources", "chovvy.test");

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
            if (!Directory.Exists(_sampleLevelPath))
            {
                throw new DirectoryNotFoundException("Sample level not found at: " + _sampleLevelPath);
            }

            var destinationPath = Path.Combine(levelsPath, "chovvy.test");
            if (Directory.Exists(destinationPath))
            {
                Directory.Delete(destinationPath, true);
            }

            Directory.CreateDirectory(destinationPath);
            foreach (var file in Directory.GetFiles(_sampleLevelPath))
            {
                if (Path.GetExtension(file) == ".meta") continue;
                File.Copy(file, Path.Combine(destinationPath, Path.GetFileName(file)));
            }
        }

        private static IEnumerator SearchForSampleLevel()
        {
            var levelList = GameObject.Find("Level List").GetComponent<LevelList>();
            levelList.Query("chovvy.test");

            yield return new WaitForSeconds(1);
        }

        private static void AssureSampleLevelIntegrity()
        {
            if (!Directory.Exists(_sampleLevelPath))
            {
                throw new DirectoryNotFoundException("Sample level not found at: " + _sampleLevelPath);
            }

            var levelsPath = GlobalState.Config.LevelStoragePath;
            if (!Directory.Exists(Path.Combine(levelsPath, "chovvy.test")))
            {
                ImportSampleLevel(levelsPath);
                return;
            }

            var levelPath = Path.Combine(levelsPath, "chovvy.test");
            foreach (var file in Directory.GetFiles(_sampleLevelPath))
            {
                if (Path.GetExtension(file) == ".meta") continue;
                var fileName = Path.GetFileName(file);
                var sourceFile = Path.Combine(_sampleLevelPath, fileName);
                var targetFile = Path.Combine(levelPath, fileName);
                if (File.ReadAllText(sourceFile) != File.ReadAllText(targetFile))
                {
                    File.Delete(targetFile);
                    File.Copy(sourceFile, targetFile);
                }
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
        public IEnumerator SampleLevelChartCardTextCorrect()
        {
            var levelList = GameObject.Find("Level List").GetComponent<LevelList>();
            levelList.Query("chovvy.test");

            yield return new WaitForSeconds(1);

            var extremeChartCard = GameObject.Find("Extreme Chart Card");
            Assert.IsNotNull(extremeChartCard);
            var chartCardText = extremeChartCard.GetComponentInChildren<Text>().text;
            Assert.AreEqual("testing Lvl. 0", chartCardText);

            var hardChartCard = GameObject.Find("Hard Chart Card");
            Assert.IsNotNull(hardChartCard);
            chartCardText = hardChartCard.GetComponentInChildren<Text>().text;
            Assert.AreEqual("Add hard", chartCardText);

            var easyChartCard = GameObject.Find("Easy Chart Card");
            Assert.IsNotNull(easyChartCard);
            chartCardText = easyChartCard.GetComponentInChildren<Text>().text;
            Assert.AreEqual("Add easy", chartCardText);
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

        [UnityTest]
        public IEnumerator SampleLevelAddEasy()
        {
            try
            {
                // Add the easy chart
                yield return SearchForSampleLevel();
                var easyChartCard = GameObject.Find("Easy Chart Card");
                Assert.IsNotNull(easyChartCard);
                var chartCardText = easyChartCard.GetComponentInChildren<Text>().text;
                Assert.AreEqual("Add easy", chartCardText);
                var addButton = easyChartCard.transform.Find("Actual Card").GetComponent<Button>();
                addButton.onClick.Invoke();

                // Check if the easy chart was loaded
                yield return null;
                Assert.AreEqual("MainScene", SceneManager.GetActiveScene().name);

                // Save the easy chart
                var saveButton = GameObject.Find("SaveButton").GetComponent<Button>();
                saveButton.onClick.Invoke();
                yield return null;

                // Go back to the level select scene
                var chartSelectButton = GameObject.Find("ChartSelectButton").GetComponent<Button>();
                chartSelectButton.onClick.Invoke();
                yield return null;
                Assert.AreEqual("LevelSelectScene", SceneManager.GetActiveScene().name);

                // Check if the easy chart was added
                yield return SearchForSampleLevel();
                easyChartCard = GameObject.Find("Easy Chart Card");
                Assert.IsNotNull(easyChartCard);
                chartCardText = easyChartCard.GetComponentInChildren<Text>().text;
                Assert.AreEqual("easy Lvl. 0", chartCardText);
            }
            finally
            {
                AssureSampleLevelIntegrity();
            }
        }
    }
}