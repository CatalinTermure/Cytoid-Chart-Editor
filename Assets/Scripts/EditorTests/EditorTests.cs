using System.Collections;
using CCE.Core;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace EditorTests
{
    public class EditorTests
    {
        [Test]
        public void AudioManagerInitializeSetsIsInitialized()
        {
            AudioManager.Initialize();
            Assert.IsTrue(AudioManager.IsInitialized);
            AudioManager.Cleanup();
        }

        [Test]
        public void AudioManagerCleanupSetsIsInitialized()
        {
            AudioManager.Initialize();
            AudioManager.Cleanup();
            Assert.IsFalse(AudioManager.IsInitialized);
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator EditorTestsWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}