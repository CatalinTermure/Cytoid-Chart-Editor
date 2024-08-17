using System;
using System.IO;
using CCE.Core;
using CCE.GameUtils;
using CCE.Utils;
using Newtonsoft.Json;
using SFB;
using UnityEngine;
using UnityEngine.UI;

namespace CCE.UI
{
    public class ChartDataChanger : MonoBehaviour
    {
        private static readonly Color _normalColor = new(1f, 1f, 1f, 0.8f);
        private static readonly Color _highlightColor = new(0.35f, 0.4f, 1f, 0.8f);
        [SerializeField] private InputField ChartNameInputField;
        [SerializeField] private InputField DifficultyInputField;
        [SerializeField] private ToastMessageManager MessageToaster;

        private GameObject _highlightedButton;

        private void Start()
        {
            ChartNameInputField.text = GlobalState.CurrentChart.Metadata.Name;

            DifficultyInputField.text = GlobalState.CurrentChart.Metadata.Difficulty.ToString();

            _highlightedButton = GameObject.Find(GlobalState.CurrentChart.Metadata.Type + "Button");
            if (_highlightedButton == null)
            {
                Debug.LogError($"CCELog: Could not find button {GlobalState.CurrentChart.Metadata.Type}Button");
            }

            _highlightedButton.GetComponent<Image>().color = _highlightColor;
        }

        public void ChangeType(GameObject btn)
        {
            _highlightedButton.GetComponent<Image>().color = _normalColor;
            GlobalState.CurrentChart.Metadata.Type = btn.GetComponentInChildren<Text>().text;
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
            NativeFilePicker.PickFile(pickedFile =>
            {
                try
                {
                    GlobalState.CurrentChart = LevelLoader.LoadChart(pickedFile, GlobalState.CurrentChart.Metadata);
                    MessageToaster.CreateToast(
                        "Chart file imported. It will not be saved unless you save the chart in the chart editing screen.");
                }
                catch (JsonException)
                {
                    MessageToaster.CreateToast("Selected file is not a valid chart file");
                }
            }, new[] { "*/*" });
        }

        private void ImportChartDesktop()
        {
            var extensions = new[] { new ExtensionFilter("Chart files", "json", "txt") };
            StandaloneFileBrowser.OpenFilePanelAsync("Open a chart file", "", extensions, false,
                paths =>
                {
                    if (paths.Length == 0) return;
                    try
                    {
                        GlobalState.CurrentChart = LevelLoader.LoadChart(paths[0], GlobalState.CurrentChart.Metadata);
                        MessageToaster.CreateToast(
                            "Chart file imported. It will not be saved unless you save the chart in the chart editing screen.");
                    }
                    catch (JsonException)
                    {
                        MessageToaster.CreateToast("Selected file is not a valid chart file");
                    }
                });
        }

        public void SaveData()
        {
            var difficultyName = ChartNameInputField.text;
            GlobalState.CurrentChart.Metadata.Name = difficultyName.Length > 0 ? difficultyName : null;

            GlobalState.CurrentChart.Metadata.Difficulty = Int32.Parse(DifficultyInputField.text);

            File.WriteAllText(Path.Combine(GlobalState.CurrentLevelPath, "level.json"),
                JsonConvert.SerializeObject(GlobalState.CurrentLevel, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                }));
        }
    }
}