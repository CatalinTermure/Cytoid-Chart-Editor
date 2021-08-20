using System.Collections;
using System.Collections.Generic;
using System.IO;
using CCE.Core;
using CCE.Data;
using CCE.Utils;
using ManagedBass;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace CCE.LevelLoading
{
    public class ChartCardController : MonoBehaviour
    {
        public List<Button> ChartCardButtons;
        public List<Text> ChartCardTexts;
        
        private readonly string[] _chartTypes = { "easy", "hard", "extreme" };

        private IEnumerator _updateChartCardsCoroutine;

        private LevelList _levelList;
        
        private void Awake()
        {
            _levelList = gameObject.GetComponent<LevelList>();
            if (_levelList == null)
            {
                Debug.LogError(
                    "You must have a LevelList component attached to an object that has a ChartCardController");
            }
        }

        private IEnumerator UpdateChartCardsCoroutine(LevelData levelData)
        {
            yield return new WaitForSeconds(LevelListBehaviour.UpdateBackgroundDelay);
            
            for (int i = 0; i < _chartTypes.Length; i++)
            {
                var chartData = levelData.Charts.Find(chart => chart.Type == _chartTypes[i]);

                if (chartData == null)
                {
                    ChartCardTexts[i].text = $"Add {_chartTypes[i]}";
                    ChartCardButtons[i].onClick.RemoveAllListeners();
                    int iCapture = i;
                    ChartCardButtons[i].onClick.AddListener(() => LoadNewChart(levelData, _chartTypes[iCapture]));
                    continue;
                }
                
                ChartCardButtons[i].onClick.RemoveAllListeners();
                ChartCardButtons[i].onClick.AddListener(() => LoadChart(levelData, chartData));
                
                ChartCardTexts[i].text = $"{chartData.DisplayName} Lvl. {chartData.Difficulty}";
            }
        }
        
        private async void LoadChart(LevelData levelData, LevelData.ChartFileData chartData)
        {
            string audioFilePath = Path.Combine(GlobalState.Config.LevelStoragePath, levelData.ID,
                chartData.MusicOverride?.Path ?? levelData.Music.Path);

            _levelList.View.FreeResources();

            byte[] data = await LevelAssetsManager.LoadFileAsync(audioFilePath);
            SceneNavigation.NavigateToChartEdit(levelData, chartData, 
                Bass.CreateStream(data, 0, data.Length, BassFlags.Decode));
        }

        public void UpdateDifficultyCards(LevelData levelData)
        {
            if(_updateChartCardsCoroutine != null) StopCoroutine(_updateChartCardsCoroutine);
            _updateChartCardsCoroutine = UpdateChartCardsCoroutine(levelData);
            StartCoroutine(_updateChartCardsCoroutine);
        }
        
        public static async void LoadNewChart(LevelData levelData, string type)
        {
            string levelDirPath = Path.Combine(GlobalState.Config.LevelStoragePath, levelData.ID);
            string audioFilePath = Path.Combine(levelDirPath, levelData.Music.Path);
            string chartFilePath = FileUtils.GetUniqueFilePath(Path.Combine(levelDirPath, $"chart-{type}.json"));
            
            File.WriteAllText(chartFilePath, GlobalState.NewChartString);
            var chartData = new LevelData.ChartFileData()
            {
                Type = type,
                Path = Path.GetFileName(chartFilePath)
            };
            
            levelData.Charts.Add(chartData);
            File.WriteAllText(Path.Combine(levelDirPath, "level.json"), 
                JsonConvert.SerializeObject(levelData, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                }));

            byte[] data = await LevelAssetsManager.LoadFileAsync(audioFilePath);
            SceneNavigation.NavigateToChartEdit(levelData, chartData, 
                Bass.CreateStream(data, 0, data.Length, BassFlags.Decode));
        }
    }
}