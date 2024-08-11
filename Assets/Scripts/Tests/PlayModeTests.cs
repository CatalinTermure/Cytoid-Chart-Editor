using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace CCE.Tests
{
    public class PlayModeTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void PlayModeTestsSimplePasses()
        {
            // Use the Assert class to test conditions
        }

        [Test]
        public void LevelListLoads()
        {
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator PlayModeTestsWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}