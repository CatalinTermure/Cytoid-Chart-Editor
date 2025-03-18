using CCE.Audio.Abstract;
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

        public static void NavigateToChartEdit(Level level, ChartMetadata chartFileData,
            IAudioStream audio)
        {
            GlobalState.AudioManager.LoadAudio(audio, true);
            GlobalState.LoadLevel(level);
            GlobalState.LoadChart(chartFileData);
            NavigateToScene("MainScene");
        }

        private static void NavigateToScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
            GlobalState.AudioManager.Pause();
        }
    }
}