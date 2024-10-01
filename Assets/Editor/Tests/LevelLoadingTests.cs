using System.Collections;
using CCE.Core;
using CCE.LevelLoading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace CCE.Tests
{
    public class LevelLoadingTests : TestUsingSampleLevel
    {
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
            yield return TestUtils.LoadSampleLevel();
            
            Assert.AreEqual("MainScene", SceneManager.GetActiveScene().name);
            Assert.AreEqual(GlobalState.CurrentLevel.ID, "chovvy.test");
        }

        [UnityTest]
        public IEnumerator SampleLevelAddEasy()
        {
            // Add the easy chart
            yield return TestUtils.SearchForSampleLevel();
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
            yield return TestUtils.SearchForSampleLevel();
            easyChartCard = GameObject.Find("Easy Chart Card");
            Assert.IsNotNull(easyChartCard);
            chartCardText = easyChartCard.GetComponentInChildren<Text>().text;
            Assert.AreEqual("easy Lvl. 0", chartCardText);
        }
    }
}