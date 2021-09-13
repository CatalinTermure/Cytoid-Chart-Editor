using CCE.Core;
using CCE.Data;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    public void NavigateToFileSelect()
    {
        NavigateToScene("LevelSelectScene");
    }

    public void NavigatoToMainScreen()
    {
        if (GlobalState.CurrentChart != null)
        {
            NavigateToScene("MainScene");
        }
    }

    public void NavigateToMainScreenUnsafe()
    {
        NavigateToScene("MainScene");
    }

    public void NavigateToLevelOptions()
    {
        if (GlobalState.CurrentChart != null)
        {
            NavigateToScene("LevelOptionsScene");
        }
    }

    public void NavigateToChartOptions()
    {
        NavigateToScene("ChartOptionsScene");
    }

    public void NavigateToEditorOptions()
    {
        NavigateToScene("EditorOptionsScene");
    }

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
        AudioManager.Pause();
    }
}
