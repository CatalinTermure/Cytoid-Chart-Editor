﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using CCE.Core;
using CCE.Data;
using CCE.Utils;
using ManagedBass;
using Newtonsoft.Json;
using UnityEngine.UI;

namespace CCE.LevelLoading
{
    public class LevelListBehaviour : MonoBehaviour
    {
        [SerializeField] private GameObject LevelCardTemplate;
        [SerializeField] private GameObject LevelMetadataPopup;

        public Image ScreenBackground;
        
        private LevelListView _levelListView;

        private LevelPopulator _levelPopulator;
        
        private Text _easyCardText, _hardCardText, _extremeCardText;
        private List<GameObject> _chartCards;
        private List<Button> _chartCardButtons;
        private List<Text> _chartCardTexts;
        private readonly string[] _cardObjectNames = {"Easy Chart Card", "Hard Chart Card", "Extreme Chart Card"};
        private readonly string[] _chartTypes = {"easy", "hard", "extreme"};

        private bool _isPopupActive;

        private void Awake()
        {
            GameObject.Find("Screen Background").GetComponent<BackgroundManager>();
            _chartCards = new List<GameObject>(3);
            _chartCardTexts = new List<Text>(3);
            _chartCardButtons = new List<Button>(3);

            for (int i = 0; i < _cardObjectNames.Length; i++)
            {
                _chartCards.Add(GameObject.Find(_cardObjectNames[i]));
                _chartCardTexts.Add(_chartCards[i].GetComponentInChildren<Text>());
                _chartCardButtons.Add(_chartCards[i].GetComponent<Button>());
            }
        }

        private void Start()
        {
            _levelListView = new LevelListView();

            _levelPopulator = new LevelPopulator(_levelListView);

            StartCoroutine(_levelPopulator.PopulateLevelsCoroutine(LevelCardTemplate));
        }

        private bool _isDragging;
        
        private float _startDragOffset;
        
        private Vector3 _startDragPosition;

        private const float _scrollSpeed = 0.01f;

        private void Update()
        {
            if (_isPopupActive) return;
            
            if (Input.GetMouseButtonDown(0) && Input.mousePosition.x > Screen.width / 2)
            {
                _isDragging = true;
                _startDragOffset = _levelListView.Offset;
                _startDragPosition = Input.mousePosition;
            }
            if (Input.GetMouseButtonUp(0))
            {
                _isDragging = false;
            }

            if (_isDragging)
            {
                _levelListView.Offset = _startDragOffset +
                    (Input.mousePosition.y - _startDragPosition.y) * _scrollSpeed;

                _levelListView.Render();
            }
        }

        public void ShowLevelMetadataPopup(string audioPath)
        {
            _isPopupActive = true;
            Instantiate(LevelMetadataPopup, Vector3.zero, Quaternion.identity, transform)
                .GetComponent<LevelMetadataPopupController>().SetAudioFile(audioPath);
        }

        public void UpdateBackground(string path)
        {
            if(_updateBackgroundCoroutine != null) StopCoroutine(_updateBackgroundCoroutine);
            _updateBackgroundCoroutine = UpdateBackgroundCoroutine(path);
            StartCoroutine(_updateBackgroundCoroutine);
        }

        public void UpdateMusic(int handle)
        {
            if(_updateMusicCoroutine != null) StopCoroutine(_updateMusicCoroutine);
            _updateMusicCoroutine = UpdateMusicCoroutine(handle);
            StartCoroutine(_updateMusicCoroutine);
        }

        public void UpdateDifficultyCards(LevelData levelData)
        {
            if(_updateDifficultyCardsCoroutine != null) StopCoroutine(_updateDifficultyCardsCoroutine);
            _updateDifficultyCardsCoroutine = UpdateDifficultyCardsCoroutine(levelData);
            StartCoroutine(_updateDifficultyCardsCoroutine);
        }

        private IEnumerator _updateMusicCoroutine;
        private IEnumerator _updateBackgroundCoroutine;
        private IEnumerator _updateDifficultyCardsCoroutine;
        
        private const float _updateBackgroundDelay = 0.6f;
        
        private static IEnumerator UpdateMusicCoroutine(int handle)
        {
            AudioManager.Stop();

            yield return new WaitForSeconds(_updateBackgroundDelay);

            AudioManager.LoadAudio(handle);
            AudioManager.Time = 0;
            AudioManager.Play();
        }

        private IEnumerator UpdateBackgroundCoroutine(string path)
        {
            yield return new WaitForSeconds(_updateBackgroundDelay);
            var tex = new Texture2D(1, 1);
            tex.LoadImage(File.ReadAllBytes(path));
            ScreenBackground.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        }

        private IEnumerator UpdateDifficultyCardsCoroutine(LevelData levelData) 
        {
            yield return new WaitForSeconds(_updateBackgroundDelay);
            
            for (int i = 0; i < _chartTypes.Length; i++)
            {
                var chartData = levelData.Charts.Find(chart => chart.Type == _chartTypes[i]);

                if (chartData == null)
                {
                    _chartCardTexts[i].text = $"Add {_chartTypes[i]}";
                    _chartCardButtons[i].onClick.RemoveAllListeners();
                    int iCapture = i;
                    _chartCardButtons[i].onClick.AddListener(() => LoadNewChart(levelData, _chartTypes[iCapture]));
                    continue;
                }
                
                _chartCardButtons[i].onClick.RemoveAllListeners();
                _chartCardButtons[i].onClick.AddListener(() => LoadChart(levelData, chartData));
                
                _chartCardTexts[i].text = $"{chartData.DisplayName} Lvl. {chartData.Difficulty}";
            }
        }

        private async void LoadChart(LevelData levelData, LevelData.ChartFileData chartData)
        {
            string audioFilePath = Path.Combine(GlobalState.Config.LevelStoragePath, levelData.ID,
                chartData.MusicOverride?.Path ?? levelData.Music.Path);

            _levelListView.FreeResources();

            byte[] data = await LevelAssetsManager.LoadFileAsync(audioFilePath);
            SceneNavigation.NavigateToChartEdit(levelData, chartData, 
                Bass.CreateStream(data, 0, data.Length, BassFlags.Decode));
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

        private void OnDisable()
        {
            _levelListView.FreeResources();
        }
    }
}