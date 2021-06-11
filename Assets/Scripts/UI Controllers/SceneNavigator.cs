using CCE;
using CCE.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    public void NavigateToFileSelect()
    {
        SceneManager.LoadScene("LevelSelectScene");
    }

    public void NavigatoToMainScreen()
    {
        if (GlobalState.CurrentChart != null)
        {
            SceneManager.LoadScene("MainScene");
        }
    }

    public void NavigateToMainScreenUnsafe()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void NavigateToLevelOptions()
    {
        if (GlobalState.CurrentChart != null)
        {
            SceneManager.LoadScene("LevelOptionsScene");
        }
    }

    public void NavigateToChartOptions()
    {
        SceneManager.LoadScene("ChartOptionsScene");
    }

    public void NavigateToEditorOptions()
    {
        SceneManager.LoadScene("EditorOptionsScene");
    }
}
