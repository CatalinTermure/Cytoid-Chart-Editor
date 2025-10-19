using System.Collections;
using System.IO;
using CCE.Core;
using CCE.LevelLoading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace CCE.Tests
{
    public static class TestUtils
    {
        public static readonly string SampleLevelPath =
            Path.Combine(Application.dataPath, "Editor", "Resources", "chovvy.test");

        public static void ImportSampleLevel(string levelsPath)
        {
            if (!Directory.Exists(SampleLevelPath))
            {
                throw new DirectoryNotFoundException("Sample level not found at: " + SampleLevelPath);
            }

            var destinationPath = Path.Combine(levelsPath, "chovvy.test");
            if (Directory.Exists(destinationPath))
            {
                Directory.Delete(destinationPath, true);
            }

            Directory.CreateDirectory(destinationPath);
            foreach (var file in Directory.GetFiles(SampleLevelPath))
            {
                if (Path.GetExtension(file) == ".meta") continue;
                File.Copy(file, Path.Combine(destinationPath, Path.GetFileName(file)));
            }
        }

        public static IEnumerator SearchForSampleLevel()
        {
            var levelList = GameObject.Find("Level List").GetComponent<LevelList>();
            levelList.Query("chovvy.test");

            yield return new WaitForSeconds(1);
        }

        public static IEnumerator LoadSampleLevel()
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
        }

        public static void AssureSampleLevelIntegrity()
        {
            if (!Directory.Exists(SampleLevelPath))
            {
                throw new DirectoryNotFoundException("Sample level not found at: " + SampleLevelPath);
            }

            var levelsPath = GlobalState.Config.LevelStoragePath;
            if (!Directory.Exists(Path.Combine(levelsPath, "chovvy.test")))
            {
                ImportSampleLevel(levelsPath);
                return;
            }

            var levelPath = Path.Combine(levelsPath, "chovvy.test");
            foreach (var file in Directory.GetFiles(SampleLevelPath))
            {
                if (Path.GetExtension(file) == ".meta") continue;
                var fileName = Path.GetFileName(file);
                var sourceFile = Path.Combine(SampleLevelPath, fileName);
                var targetFile = Path.Combine(levelPath, fileName);
                if (File.ReadAllText(sourceFile) != File.ReadAllText(targetFile))
                {
                    File.Delete(targetFile);
                    File.Copy(sourceFile, targetFile);
                }
            }
        }
    }
}