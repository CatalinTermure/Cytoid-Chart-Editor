using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace CCE.Tests
{
    public class LevelOptionsTestSuite : LevelEditingTestSuite
    {
        [UnitySetUp]
        public new IEnumerator SetUp()
        {
            yield return base.SetUp();
            var levelOptionsButton = GameObject.Find("Level Options Button");
            levelOptionsButton.GetComponent<Button>().onClick.Invoke();

            yield return null;
        }
    }
}