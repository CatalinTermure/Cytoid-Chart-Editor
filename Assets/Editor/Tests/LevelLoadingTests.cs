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
            if (!Directory.Exists(Path.Combine(levelsPath, "chovvy.test")))
            {
                LoadSampleLevel(levelsPath);
                Assert.Fail("Sample level was not found and it was loaded. Please re-run the test.");
            }
        }

        private static void LoadSampleLevel(string levelsPath)
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
    }
}