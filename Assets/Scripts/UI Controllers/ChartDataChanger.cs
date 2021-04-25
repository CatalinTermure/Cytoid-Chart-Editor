using Newtonsoft.Json;
using System.IO;
using CCE;
using CCE.Core;
using UnityEngine;
using UnityEngine.UI;

public class ChartDataChanger : MonoBehaviour
{
    private static readonly Color NormalColor = new Color(1, 1, 1, 0.8f), HighlightColor = new Color(0.35f, 0.4f, 1f, 0.8f);

    private GameObject HighlightedButton;

    public void ChangeType(GameObject btn)
    {
        HighlightedButton.GetComponent<Image>().color = NormalColor;
        GlobalState.CurrentChart.Data.Type = btn.GetComponentInChildren<Text>().text;
        HighlightedButton = btn;
        HighlightedButton.GetComponent<Image>().color = HighlightColor;
    }

    private void Start()
    {
        GameObject.Find("NameInputField").GetComponent<InputField>().text = GlobalState.CurrentChart.Data.Name;

        GameObject.Find("DifficultyInputField").GetComponent<InputField>().text = GlobalState.CurrentChart.Data.Difficulty.ToString();

        HighlightedButton = GameObject.Find(GlobalState.CurrentChart.Data.Type + "Button");
        HighlightedButton.GetComponent<Image>().color = HighlightColor;

        GameObject.Find("FileNameInputField").GetComponent<InputField>().text = Path.GetFileName(GlobalState.CurrentChart.Data.Path);
    }

    public void SaveData()
    {
        string name = GameObject.Find("NameInputField").GetComponent<InputField>().text;
        if (name.Length > 0)
        {
            GlobalState.CurrentChart.Data.Name = name;
        }
        else
        {
            GlobalState.CurrentChart.Data.Name = null;
        }
        GlobalState.CurrentChart.Data.Difficulty = int.Parse(GameObject.Find("DifficultyInputField").GetComponent<InputField>().text);

        if (File.Exists(Path.Combine(GlobalState.CurrentLevelPath, GlobalState.CurrentChart.Data.Path)))
        {
            File.Move(Path.Combine(GlobalState.CurrentLevelPath, GlobalState.CurrentChart.Data.Path), Path.Combine(GlobalState.CurrentLevelPath, GameObject.Find("FileNameInputField").GetComponent<InputField>().text));
            GlobalState.CurrentChart.Data.Path = GameObject.Find("FileNameInputField").GetComponent<InputField>().text;
        }

        if (!File.Exists(Path.Combine(GlobalState.CurrentLevelPath, GlobalState.CurrentChart.Data.Path)))
        {
            return;
        }

        File.WriteAllText(Path.Combine(GlobalState.CurrentLevelPath, "level.json"), JsonConvert.SerializeObject(GlobalState.CurrentLevel, new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        }));
    }
}
