using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using CCE.Core;
using CCE.Data;
using CCE.UI;
using CCE.Utils;
using ManagedBass;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace CCE.LevelLoading
{
    public class ChartCardController : MonoBehaviour
    {
        private const float _holdDeleteTimeThreshold = 1.0f;
        public List<Button> ChartCardButtons;
        public List<Text> ChartCardTexts;
        public List<Image> ChartDeleteProgressCircles;
        public List<Collider2D> ChartDeleteButtonColliders;

        private readonly string[] _chartTypes = { "easy", "hard", "extreme" };
        private bool _isDeletingChart;
        private LevelData _levelData;

        private LevelList _levelList;

        private IEnumerator _updateChartCardsCoroutine;

        private void Awake()
        {
            _levelList = gameObject.GetComponent<LevelList>();
            if (_levelList == null)
            {
                Debug.LogError(
                    "You must have a LevelList component attached to an object that has a ChartCardController");
            }
        }

        private void Update()
        {
            if (_isDeletingChart) return;
            if (!Input.GetMouseButton(0)) return;
            if (_levelData == null) return;

            for (int i = 0; i < _chartTypes.Length; i++)
            {
                if (_levelData.Charts.Count(chart => chart.Type == _chartTypes[i]) == 0) continue;

                if (ChartDeleteButtonColliders[i].OverlapPoint(Camera.main!.ScreenToWorldPoint(Input.mousePosition)))
                {
                    StartCoroutine(DeleteChartCoroutine(ChartDeleteButtonColliders[i], ChartDeleteProgressCircles[i],
                        _chartTypes[i]));
                }
            }
        }

        private IEnumerator UpdateChartCardsCoroutine(LevelData levelData)
        {
            yield return new WaitForSeconds(LevelListBehaviour.UpdateBackgroundDelay);

            _levelData = levelData;

            for (int i = 0; i < _chartTypes.Length; i++)
            {
                LevelData.ChartFileData chartData = levelData.Charts.Find(chart => chart.Type == _chartTypes[i]);

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

        public void UpdateChartCards(LevelData levelData)
        {
            if (_updateChartCardsCoroutine != null) StopCoroutine(_updateChartCardsCoroutine);
            _updateChartCardsCoroutine = UpdateChartCardsCoroutine(levelData);
            StartCoroutine(_updateChartCardsCoroutine);
        }

        public static async void LoadNewChart(LevelData levelData, string type)
        {
            string levelDirPath = Path.Combine(GlobalState.Config.LevelStoragePath, levelData.ID);
            string audioFilePath = Path.Combine(levelDirPath, levelData.Music.Path);
            string chartFilePath = FileUtils.GetUniqueFilePath(Path.Combine(levelDirPath, $"chart-{type}.json"));

            File.WriteAllText(chartFilePath, GlobalState.NewChartString);
            var chartData = new LevelData.ChartFileData
            {
                Type = type,
                Path = Path.GetFileName(chartFilePath)
            };

            levelData.Charts.Add(chartData);
            File.WriteAllText(Path.Combine(levelDirPath, "level.json"),
                JsonConvert.SerializeObject(levelData, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                }));

            byte[] data = await LevelAssetsManager.LoadFileAsync(audioFilePath);
            SceneNavigation.NavigateToChartEdit(levelData, chartData,
                Bass.CreateStream(data, 0, data.Length, BassFlags.Decode));
        }

        private IEnumerator DeleteChartCoroutine(Collider2D targetCollider, Image progressGraphic, string type)
        {
            _isDeletingChart = true;

            Camera mainCamera = Camera.main;
            float startTime = Time.time;
            float currentTime = Time.time;
            progressGraphic.fillAmount = 0.0f;
            while (Input.GetMouseButton(0) && startTime + _holdDeleteTimeThreshold > currentTime)
            {
                currentTime = Time.time;
                progressGraphic.fillAmount = (currentTime - startTime) / _holdDeleteTimeThreshold;
                yield return null;
            }

            if (startTime + _holdDeleteTimeThreshold <= currentTime)
            {
                ShowDeleteMessagePopup(type);
            }
            else
            {
                _isDeletingChart = false;
            }

            progressGraphic.fillAmount = 1.0f;
        }

        private void ShowDeleteMessagePopup(string type)
        {
            if (_levelData.Charts.Count() == 1)
            {
                MessagePopupController.ShowPopup(
                    "Deleting the only chart of a level will also delete the level. Are you sure you want to delete the level?",
                    () =>
                    {
                        Directory.Delete(Path.Combine(GlobalState.Config.LevelStoragePath, _levelData.ID), true);
                        _levelList.View.RemoveLevel(_levelData);
                        _isDeletingChart = false;
                    }, 
                    () => _isDeletingChart = false);
            }
            else
            {
                MessagePopupController.ShowPopup($"Are you sure you want to delete the {type} chart for this level?",
                    () =>
                    {
                        _levelData.Charts.RemoveAll(chartData => chartData.Type == type);
                        DeleteDeadAssets(_levelData);
                        SaveLevel();
                        UpdateChartCards(_levelData);
                        _isDeletingChart = false;
                    },
                    () => _isDeletingChart = false);
            }
        }

        private void SaveLevel()
        {
            string levelDirPath = Path.Combine(GlobalState.Config.LevelStoragePath, _levelData.ID);
            File.WriteAllText(Path.Combine(levelDirPath, "level.json"),
                JsonConvert.SerializeObject(_levelData, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                }));
        }

        private void DeleteDeadAssets(LevelData levelData)
        {
            foreach (var chart in _levelData.Charts)
            {
                if (!String.IsNullOrEmpty(chart.Storyboard?.Path))
                {
                    // this is a workaround so that we don't delete files linked to a storyboard
                    // will need to change this when storyboard parsing is implemented
                    return;
                }
            }
            
            string levelDir = Path.Combine(GlobalState.Config.LevelStoragePath, _levelData.ID);
            
            // The huge amount of Path.GetFullPath() comes from the need to use a consistent path scheme
            // so that the string comparisons don't return false negatives
            
            var validFiles = new List<string>();
            validFiles.Add(Path.GetFullPath(Path.Combine(levelDir, "level.json")));
            validFiles.Add(Path.GetFullPath(Path.Combine(levelDir, ".bg")));
            if (_levelData.Background?.Path != null)
                validFiles.Add(Path.GetFullPath(Path.Combine(levelDir, _levelData.Background?.Path)));
            if (_levelData.Music?.Path != null)
                validFiles.Add(Path.GetFullPath(Path.Combine(levelDir, _levelData.Music?.Path)));
            if (_levelData.MusicPreview?.Path != null)
                validFiles.Add(Path.GetFullPath(Path.Combine(levelDir, _levelData.MusicPreview?.Path)));

            foreach (var chart in _levelData.Charts)
            {
                validFiles.Add(Path.GetFullPath(Path.Combine(levelDir, chart.Path)));
                if (chart.MusicOverride?.Path != null)
                    validFiles.Add(Path.GetFullPath(Path.Combine(levelDir, chart.MusicOverride?.Path)));
            }

            List<string> filesToRemove = GetFilesInDirectory(levelDir).Except(validFiles).ToList();

            foreach (string file in filesToRemove)
            {
                File.Delete(file);
            }
        }

        private List<string> GetFilesInDirectory(string dirPath)
        {
            List<string> result = Directory.EnumerateFiles(Path.GetFullPath(dirPath)).ToList();

            foreach (string folder in Directory.EnumerateDirectories(Path.GetFullPath(dirPath)))
            {
                result.AddRange(GetFilesInDirectory(folder));
            }

            return result;
        }
    }
}