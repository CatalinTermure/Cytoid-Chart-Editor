using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CCE.Core;
using CCE.Data;
using CCE.GameUtils;
using CCE.Popups;
using CCE.Utils;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace CCE.LevelLoading
{
    public class ChartCardController : MonoBehaviour
    {
        private const float HoldDeleteTimeThreshold = 1.0f;

        // References to the chart cards
        [SerializeField] private List<GameObject> ChartCards;
        private readonly List<Button> _chartCardButtons = new();
        private readonly List<Text> _chartCardTexts = new();
        private readonly List<Collider2D> _chartDeleteButtonColliders = new();
        private readonly List<Image> _chartDeleteProgressCircles = new();
        private readonly List<string> _chartTypes = new();

        private bool _isDeletingChart;
        private Level _level;

        private LevelList _levelList;
        private Camera _mainCamera;

        private IEnumerator _updateChartCardsCoroutine;

        private void Awake()
        {
            foreach (var chartCardInfo in
                     ChartCards.Select(chartCard => chartCard.GetComponentInChildren<ChartCardInfo>()))
            {
                _chartCardButtons.Add(chartCardInfo.CardButton);
                _chartCardTexts.Add(chartCardInfo.CardText);
                _chartDeleteProgressCircles.Add(chartCardInfo.DeleteProgressImage);
                _chartDeleteButtonColliders.Add(chartCardInfo.DeleteCollider);
                _chartTypes.Add(chartCardInfo.ChartType);
            }

            _mainCamera = Camera.main;

            _levelList = gameObject.GetComponent<LevelList>();
            if (_levelList == null)
            {
                Debug.LogError(
                    "You must have a LevelList component attached to an object that has a ChartCardController");
            }
        }

        private void Update()
        {
            if (_level == null) return;
            if (_isDeletingChart) return;
            if (!Input.GetMouseButton(0)) return;

            HandleChartDeleteButton();
        }

        private void HandleChartDeleteButton()
        {
            for (var i = 0; i < _chartTypes.Count; i++)
            {
                if (_level.Charts.All(chart => chart.Type != _chartTypes[i])) continue;

                if (_chartDeleteButtonColliders[i].OverlapPoint(_mainCamera.ScreenToWorldPoint(Input.mousePosition)))
                {
                    StartCoroutine(DeleteChartCoroutine(_chartDeleteProgressCircles[i], _chartTypes[i]));
                }
            }
        }

        private IEnumerator DeleteChartCoroutine(Image progressGraphic, string type)
        {
            _isDeletingChart = true;

            var startTime = Time.time;
            var currentTime = Time.time;
            progressGraphic.fillAmount = 0.0f;
            while (Input.GetMouseButton(0) && startTime + HoldDeleteTimeThreshold > currentTime)
            {
                currentTime = Time.time;
                progressGraphic.fillAmount = (currentTime - startTime) / HoldDeleteTimeThreshold;
                yield return null;
            }

            if (startTime + HoldDeleteTimeThreshold <= currentTime)
            {
                ShowDeleteMessagePopup(type);
            }
            else
            {
                _isDeletingChart = false;
            }

            progressGraphic.fillAmount = 1.0f;
        }

        private IEnumerator UpdateChartCardsCoroutine(Level level)
        {
            yield return new WaitForSeconds(LevelListBehaviour.UpdateBackgroundDelay);

            _level = level;

            for (var i = 0; i < _chartTypes.Count; i++)
            {
                var chartData = level.Charts.Find(chart => chart.Type == _chartTypes[i]);

                if (chartData == null)
                {
                    _chartCardTexts[i].text = $"Add {_chartTypes[i]}";
                    _chartCardButtons[i].onClick.RemoveAllListeners();
                    var iCapture = i;
                    _chartCardButtons[i].onClick.AddListener(() => LoadNewChart(level, _chartTypes[iCapture]));
                    continue;
                }

                _chartCardButtons[i].onClick.RemoveAllListeners();
                _chartCardButtons[i].onClick.AddListener(() => LoadChart(level, chartData));

                _chartCardTexts[i].text = $"{chartData.DisplayName} Lvl. {chartData.Difficulty}";
            }
        }

        private void LoadChart(Level level, ChartMetadata chartData)
        {
            var audioFilePath = Path.Combine(GlobalState.Config.LevelStoragePath, level.ID,
                chartData.MusicOverride?.Path ?? level.Music.Path);

            _levelList.View.FreeResources();

            SceneNavigator.NavigateToChartEdit(level, chartData,
                GlobalState.AudioManager.CreateStream(File.ReadAllBytes(audioFilePath)));
        }

        public void UpdateChartCards(Level level)
        {
            if (_updateChartCardsCoroutine != null) StopCoroutine(_updateChartCardsCoroutine);
            _updateChartCardsCoroutine = UpdateChartCardsCoroutine(level);
            StartCoroutine(_updateChartCardsCoroutine);
        }

        public static void LoadNewChart(Level level, string type)
        {
            var levelDirPath = Path.Combine(GlobalState.Config.LevelStoragePath, level.ID);
            var audioFilePath = Path.Combine(levelDirPath, level.Music.Path);
            var chartFilePath = FileUtils.GetUniqueFilePath(Path.Combine(levelDirPath, $"chart-{type}.json"));

            var chartWriteTask = File.WriteAllTextAsync(chartFilePath, GlobalState.NewChartString);
            var levelDataWriteTask = File.WriteAllTextAsync(Path.Combine(levelDirPath, "level.json"),
                JsonConvert.SerializeObject(level, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                }));

            var chartData = new ChartMetadata
            {
                Type = type,
                Path = Path.GetFileName(chartFilePath)
            };

            level.Charts.Add(chartData);

            SceneNavigator.NavigateToChartEdit(level, chartData,
                GlobalState.AudioManager.CreateStream(File.ReadAllBytes(audioFilePath)));
            Task.WaitAll(chartWriteTask, levelDataWriteTask);
        }

        private void ShowDeleteMessagePopup(string type)
        {
            if (_level.Charts.Count() == 1)
            {
                MessagePopupController.ShowPopup(
                    "Deleting the only chart of a level will also delete the level. Are you sure you want to delete the level?",
                    () =>
                    {
                        Directory.Delete(Path.Combine(GlobalState.Config.LevelStoragePath, _level.ID), true);
                        _levelList.RemoveLevel(_level);
                        _isDeletingChart = false;
                    },
                    () => _isDeletingChart = false);
            }
            else
            {
                MessagePopupController.ShowPopup($"Are you sure you want to delete the {type} chart for this level?",
                    () =>
                    {
                        _level.Charts.RemoveAll(chartData => chartData.Type == type);
                        LevelUtils.DeleteDeadAssets(GlobalState.Config.LevelStoragePath, _level);
                        SaveLevel();
                        UpdateChartCards(_level);
                        _isDeletingChart = false;
                    },
                    () => _isDeletingChart = false);
            }
        }

        private void SaveLevel()
        {
            var levelDirPath = Path.Combine(GlobalState.Config.LevelStoragePath, _level.ID);
            File.WriteAllText(Path.Combine(levelDirPath, "level.json"),
                JsonConvert.SerializeObject(_level, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                }));
        }
    }
}