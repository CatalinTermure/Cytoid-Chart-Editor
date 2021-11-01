using System;
using Newtonsoft.Json;
using System.IO;
using CCE;
using CCE.Core;
using UnityEngine;
using UnityEngine.UI;
using SFB;
using CCE.Data;

public class ChartDataChanger : MonoBehaviour
{
    private static readonly Color _normalColor = new Color(1f, 1f, 1f, 0.8f);
    private static readonly Color _highlightColor = new Color(0.35f, 0.4f, 1f, 0.8f);

    private GameObject _highlightedButton;
    [SerializeField] private InputField ChartNameInputField;
    [SerializeField] private InputField DifficultyInputField;
    [SerializeField] private ToastMessageManager MessageToaster;

    public void ChangeType(GameObject btn)
    {
        _highlightedButton.GetComponent<Image>().color = _normalColor;
        GlobalState.CurrentChart.Data.Type = btn.GetComponentInChildren<Text>().text;
        _highlightedButton = btn;
        _highlightedButton.GetComponent<Image>().color = _highlightColor;
    }

    public void ImportChart()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            ImportChartAndroid();
        }
        else
        {
            ImportChartDesktop();
        }
    }

    private void ImportChartAndroid()
    {
        NativeFilePicker.PickFile(pickedFile => {
            try
            {
                ChartData chartData = JsonConvert.DeserializeObject<ChartData>(File.ReadAllText(pickedFile));
                GlobalState.CurrentChart = new Chart(chartData, GlobalState.CurrentChart.Data);
                MessageToaster.CreateToast("Chart file imported. It will not be saved unless you save the chart in the chart editing screen.");
            } catch(JsonException)
            {
                MessageToaster.CreateToast("Selected file is not a valid chart file");
            }
        }, new[] { "*/*" });
    }

    private void ImportChartDesktop()
    {
        var extensions = new[] { new ExtensionFilter("Chart files", "json", "txt") };
        StandaloneFileBrowser.OpenFilePanelAsync("Open a chart file", "", extensions, false,
            paths => {
                if (paths.Length == 0) return;
                try
                {
                    ChartData chartData = JsonConvert.DeserializeObject<ChartData>(File.ReadAllText(paths[0]));
                    GlobalState.CurrentChart = new Chart(chartData, GlobalState.CurrentChart.Data);
                    MessageToaster.CreateToast("Chart file imported. It will not be saved unless you save the chart in the chart editing screen.");
                }
                catch (JsonException)
                {
                    MessageToaster.CreateToast("Selected file is not a valid chart file");
                }
            });
    }

    private void Start()
    {
        ChartNameInputField.text = GlobalState.CurrentChart.Data.Name;

        DifficultyInputField.text = GlobalState.CurrentChart.Data.Difficulty.ToString();

        _highlightedButton = GameObject.Find(GlobalState.CurrentChart.Data.Type + "Button");
        if (_highlightedButton == null)
            Debug.LogError($"CCELog: Could not find button {GlobalState.CurrentChart.Data.Type}Button");
        
        _highlightedButton.GetComponent<Image>().color = _highlightColor;
    }

    public void SaveData()
    {
        string difficultyName = ChartNameInputField.text;
        GlobalState.CurrentChart.Data.Name = difficultyName.Length > 0 ? difficultyName : null;
        
        GlobalState.CurrentChart.Data.Difficulty = Int32.Parse(DifficultyInputField.text);

        File.WriteAllText(Path.Combine(GlobalState.CurrentLevelPath, "level.json"), 
            JsonConvert.SerializeObject(GlobalState.CurrentLevel, new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        }));
    }
}
