using System;
using Newtonsoft.Json;
using System.IO;
using CCE;
using CCE.Core;
using UnityEngine;
using UnityEngine.UI;

public class ChartDataChanger : MonoBehaviour
{
    private static readonly Color _normalColor = new Color(1f, 1f, 1f, 0.8f);
    private static readonly Color _highlightColor = new Color(0.35f, 0.4f, 1f, 0.8f);

    private GameObject _highlightedButton;
    [SerializeField] private InputField ChartNameInputField;
    [SerializeField] private InputField DifficultyInputField;

    public void ChangeType(GameObject btn)
    {
        _highlightedButton.GetComponent<Image>().color = _normalColor;
        GlobalState.CurrentChart.Data.Type = btn.GetComponentInChildren<Text>().text;
        _highlightedButton = btn;
        _highlightedButton.GetComponent<Image>().color = _highlightColor;
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
        string difficultyName = GameObject.Find("NameInputField").GetComponent<InputField>().text;
        GlobalState.CurrentChart.Data.Name = difficultyName.Length > 0 ? difficultyName : null;
        
        GlobalState.CurrentChart.Data.Difficulty =
            Int32.Parse(GameObject.Find("DifficultyInputField").GetComponent<InputField>().text);

        File.WriteAllText(Path.Combine(GlobalState.CurrentLevelPath, "level.json"), 
            JsonConvert.SerializeObject(GlobalState.CurrentLevel, new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        }));
    }
}
