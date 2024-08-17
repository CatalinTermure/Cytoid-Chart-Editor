using System.IO;
using CCE.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CCE.Core
{
    public class SceneNavigator : MonoBehaviour
    {
        public static void NavigateToFileSelect()
        {
            NavigateToScene("LevelSelectScene");
        }

        public static void NavigateToMainScreen()
        {
            if (GlobalState.CurrentChart != null)
            {
                NavigateToScene("MainScene");
            }
        }

        public static void NavigateToMainScreenUnsafe()
        {
            NavigateToScene("MainScene");
        }

        public static void NavigateToLevelOptions()
        {
            if (GlobalState.CurrentChart != null)
            {
                NavigateToScene("LevelOptionsScene");
            }
        }

        public static void NavigateToChartOptions()
        {
            NavigateToScene("ChartOptionsScene");
        }

        public static void NavigateToEditorOptions()
        {
            NavigateToScene("EditorOptionsScene");
        }

        public static void NavigateToChartEdit(LevelData levelData, ChartMetadata chartFileData,
            AudioStream audio)
        {
            AudioManager.LoadAudio(audio, true);
            GlobalState.LoadLevel(levelData,
                Path.Combine(GlobalState.Config.LevelStoragePath, levelData.ID, "level.json"));
            GlobalState.LoadChart(chartFileData);
            NavigateToScene("MainScene");
        }

        private static void NavigateToScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
            AudioManager.Pause();
        }
    }
}