using CCE.Core;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CCE
{
    public class LevelExporter : MonoBehaviour
    {
        public void SaveLevel()
        {
            string srcDirPath = GlobalState.CurrentLevelPath;
            string tempDirPath = Path.Combine(GlobalState.Config.TempStoragePath, GlobalState.CurrentLevel.ID);
        }
    }
}
