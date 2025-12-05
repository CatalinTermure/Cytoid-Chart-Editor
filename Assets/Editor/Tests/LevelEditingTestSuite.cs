using System.Collections;
using UnityEngine.TestTools;

namespace CCE.Tests
{
    public class LevelEditingTestSuite : TestUsingSampleLevel
    {
        [UnitySetUp]
        public new IEnumerator SetUp()
        {
            yield return base.SetUp();
            yield return TestUtils.LoadSampleLevel();
        }
    }
}