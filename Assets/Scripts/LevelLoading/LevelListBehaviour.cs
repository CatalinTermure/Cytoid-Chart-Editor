using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using CCE.Core;
using CCE.Data;
using ManagedBass;
using Newtonsoft.Json;
using UnityEngine.UI;

namespace CCE.LevelLoading
{
    public class LevelListBehaviour : MonoBehaviour
    {
        [SerializeField]
        private GameObject LevelCardTemplate;

        public Image ScreenBackground;
        
        private LevelListView _levelListView;

        private LevelPopulator _levelPopulator;
        
        private Text _easyCardText, _hardCardText, _extremeCardText;
        private List<GameObject> _chartCards;
        private List<Text> _chartCardTexts;
        private readonly string[] _cardObjectNames = {"Easy Chart Card", "Hard Chart Card", "Extreme Chart Card"};
        private readonly string[] _chartTypes = {"easy", "hard", "extreme"};

        private void Awake()
        {
            GameObject.Find("Screen Background").GetComponent<BackgroundManager>();
            _chartCards = new List<GameObject>(3);
            _chartCardTexts = new List<Text>(3);

            for (int i = 0; i < _cardObjectNames.Length; i++)
            {
                _chartCards.Add(GameObject.Find(_cardObjectNames[i]));
                _chartCardTexts.Add(_chartCards[i].GetComponentInChildren<Text>());
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

        public void UpdateBackground(Sprite background)
        {
            if(_updateBackgroundCoroutine != null) StopCoroutine(_updateBackgroundCoroutine);
            _updateBackgroundCoroutine = UpdateBackgroundCoroutine(background);
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

        private static IEnumerator UpdateMusicCoroutine(int handle)
        {
            AudioManager.Stop();

            yield return new WaitForSeconds(0.25f);

            AudioManager.LoadAudio(handle);
            AudioManager.Play();
        }

        private IEnumerator UpdateBackgroundCoroutine(Sprite background)
        {
            yield return new WaitForSeconds(0.25f);
            ScreenBackground.sprite = background;
        }

        private IEnumerator UpdateDifficultyCardsCoroutine(LevelData levelData)
        {
            yield return new WaitForSeconds(0.25f);
            
            for (int i = 0; i < _chartTypes.Length; i++)
            {
                var chartData = levelData.Charts.Find(chart => chart.Type == _chartTypes[i]);

                if (chartData == null)
                {
                    _chartCardTexts[i].text = $"Add {_chartTypes[i]}";
                    continue;
                }
                
                _chartCards[i].GetComponent<Button>().onClick.RemoveAllListeners();
                _chartCards[i].GetComponent<Button>().onClick
                    .AddListener(() =>
                    {
                        string chartFilePath = Path.Combine(GlobalState.Config.LevelStoragePath, levelData.ID,
                            chartData.Path);
                        string audioFilePath = Path.Combine(GlobalState.Config.LevelStoragePath, levelData.ID,
                            chartData.MusicOverride?.Path ?? levelData.Music.Path);
                        var chart = new Chart(JsonConvert.DeserializeObject<ChartData>(File.ReadAllText(chartFilePath)),
                            chartData);
                        
                        SceneNavigation.NavigateToChartEdit(chart, Bass.CreateStream(audioFilePath));
                        // TODO: read this fully into memory first to reduce audio delay (?)
                    });
                
                _chartCardTexts[i].text = $"{chartData.DisplayName} Lvl. {chartData.Difficulty}";
            }
        }

        private void OnEnable()
        {
            if (!AudioManager.IsInitialized)
            {
                AudioManager.Initialize();
            }
        }

        private void OnApplicationQuit()
        {
            Bass.Free();
        }
    }
}