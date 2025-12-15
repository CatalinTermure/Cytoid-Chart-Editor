using System.Collections;
using System.IO;
using CCE.LevelLoading;
using CCE.Utils;
using CCE.Popups;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

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

        [UnityTest]
        public IEnumerator LevelMetadataPopupValidatesLevelId()
        {
            var levelListBehaviour =
                Object.FindAnyObjectByType<LevelListBehaviour>();
            var testAudioPath = Path.Combine(TestUtils.SampleLevelPath, "test-audio.mp3");
            levelListBehaviour.ShowLevelMetadataPopup(testAudioPath);

            var idField = GameObject.Find("IDField");
            Assert.IsNotNull(idField);
            var idClassFieldDisplay = idField.GetComponent<ClassFieldDisplay>();
            Assert.IsNotNull(idClassFieldDisplay);
            idClassFieldDisplay.ValueInputField.text = "wrong level ID";
            idClassFieldDisplay.ValueInputField.ReleaseSelection();

            yield return null;

            var idFieldValidationResult = idField.transform.Find("ValidationResult").gameObject;
            Assert.IsTrue(idFieldValidationResult.activeSelf);

            idFieldValidationResult.GetComponent<Button>().onClick.Invoke();

            yield return null;

            var validationErrorsPopup = Object.FindAnyObjectByType<ValidationErrorsPopupController>();
            Assert.IsNotNull(validationErrorsPopup);
        }
    }
}