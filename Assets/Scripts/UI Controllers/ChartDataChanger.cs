using Newtonsoft.Json;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ChartDataChanger : MonoBehaviour
{
    private static readonly Color NormalColor = new Color(1, 1, 1, 0.8f), HighlightColor = new Color(0.35f, 0.4f, 1f, 0.8f);

    private GameObject HighlightedButton;

    public void ChangeType(GameObject btn)
    {
        HighlightedButton.GetComponent<Image>().color = NormalColor;
        GlobalState.CurrentChart.Data.type = btn.GetComponentInChildren<Text>().text;
        HighlightedButton = btn;
        HighlightedButton.GetComponent<Image>().color = HighlightColor;
    }

    private void Start()
    {
        GameObject.Find("NameInputField").GetComponent<InputField>().text = GlobalState.CurrentChart.Data.name;

        GameObject.Find("DifficultyInputField").GetComponent<InputField>().text = GlobalState.CurrentChart.Data.difficulty.ToString();

        HighlightedButton = GameObject.Find(GlobalState.CurrentChart.Data.type + "Button");
        HighlightedButton.GetComponent<Image>().color = HighlightColor;

        GameObject.Find("FileNameInputField").GetComponent<InputField>().text = Path.GetFileName(GlobalState.CurrentChart.Data.path);
    }

    public void SaveData()
    {
        string name = GameObject.Find("NameInputField").GetComponent<InputField>().text;
        if(name.Length > 0)
        {
            GlobalState.CurrentChart.Data.name = name;
        }
        else
        {
            GlobalState.CurrentChart.Data.name = null;
        }
        GlobalState.CurrentChart.Data.difficulty = int.Parse(GameObject.Find("DifficultyInputField").GetComponent<InputField>().text);

        File.Move(Path.Combine(GlobalState.CurrentLevelPath, GlobalState.CurrentChart.Data.path), Path.Combine(GlobalState.CurrentLevelPath, GameObject.Find("FileNameInputField").GetComponent<InputField>().text));
        GlobalState.CurrentChart.Data.path = GameObject.Find("FileNameInputField").GetComponent<InputField>().text;

        File.WriteAllText(Path.Combine(GlobalState.CurrentLevelPath, "level.json"), JsonConvert.SerializeObject(GlobalState.CurrentLevel, new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        }));
    }
}
