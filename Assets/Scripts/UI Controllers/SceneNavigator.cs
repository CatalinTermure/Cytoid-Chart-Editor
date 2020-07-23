using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    public void NavigateToFileSelect()
    {
        GlobalState.MusicManager?.UnloadAudio();
        SceneManager.LoadScene("FileSelectScene");
    }

    public void NavigatoToMainScreen()
    {
        if(GlobalState.CurrentChart != null)
        {
            SceneManager.LoadScene("MainScene");
        }
    }

    public void NavigateToLevelOptions()
    {
        if(GlobalState.CurrentChart != null)
        {
            SceneManager.LoadScene("LevelOptionsScene");
        }
    }

    public void NavigateToChartOptions()
    {
        SceneManager.LoadScene("ChartOptionsScene");
    }
}
