using CCE.Data;
using UnityEngine.SceneManagement;

namespace CCE.Core
{
    public static class SceneNavigation
    {
        public static void NavigateToChartEdit(Chart chart, int audioHandle)
        {
            AudioManager.LoadAudio(audioHandle);
            GlobalState.CurrentChart = chart;
            NavigateToScene("MainScene");
        }

        private static void NavigateToScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
