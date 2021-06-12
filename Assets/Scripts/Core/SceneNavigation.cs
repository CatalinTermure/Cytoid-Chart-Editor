using System.IO;
using CCE.Data;
using UnityEngine.SceneManagement;

namespace CCE.Core
{
    public static class SceneNavigation
    {
        public static void NavigateToChartEdit(LevelData levelData, LevelData.ChartFileData chartFileData, int audioHandle)
        {
            AudioManager.LoadAudio(audioHandle, true);
            GlobalState.LoadLevel(levelData, Path.Combine(GlobalState.Config.LevelStoragePath, levelData.ID, "level.json"));
            GlobalState.LoadChart(chartFileData);
            NavigateToScene("MainScene");
        }

        private static void NavigateToScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
