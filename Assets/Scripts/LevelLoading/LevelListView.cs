using System.Collections.Generic;
using CCE.Data;
using ManagedBass;
using UnityEngine;
using UnityEngine.UI;

namespace CCE.LevelLoading
{
    public class LevelListView : MonoBehaviour
    {
        private const int PoolSize = 24;
        private const float ArcStep = Mathf.PI / 80;
        private const float CircleRadius = 3000;
        [SerializeField] private GameObject LevelListCenter;
        [SerializeField] private GameObject LevelItemPrefab;
        [SerializeField] private Text CurrentIDText;
        private readonly List<LevelCardInfo> _levelCardInfos = new();
        private readonly List<GameObject> _levelCards = new();

        private int _currentLevelIndex = -1;

        private List<LevelData> _filteredLevels = new();

        private GameObject _helpText;
        private int _lastRenderedOffset = PoolSize / 2;

        private LevelAssetsManager _levelAssetsManager;
        private LevelList _levelList;

        private float _offset;

        /// <summary>
        ///     Position of the level in the level list that the center level card represents.
        ///     <para>Integer part of the float represents the index of the level.</para>
        ///     <para>
        ///         Fractional part represents how much the middle level card is displaced
        ///         upwards from the middle of the screen, in respect to one <see cref="ArcStep" />.
        ///     </para>
        /// </summary>
        public float Offset
        {
            get => _offset;
            set => _offset = Mathf.Clamp(value, 0, _filteredLevels.Count - 0.5f);
        }

        private void Awake()
        {
            _levelList = gameObject.GetComponent<LevelList>();
            if (_levelList == null)
            {
                Debug.LogError("You must have a LevelList component attached to an object that has a LevelListView");
            }

            _helpText = GameObject.Find("Help Text");

            _levelAssetsManager = new LevelAssetsManager();
        }

        public void Initialize(List<LevelData> levels)
        {
            _lastRenderedOffset = 0;
            _offset = 0;

            _filteredLevels = levels;

            _levelCards.ForEach(obj => Destroy(obj));
            _levelCards.Clear();
            _levelCardInfos.Clear();

            _helpText.SetActive(false);

            var listCenterTransform = LevelListCenter.GetComponent<RectTransform>();

            for (var i = 0; i < PoolSize && i < _filteredLevels.Count; i++)
            {
                var levelItem = Instantiate(LevelItemPrefab, listCenterTransform);
                _levelCards.Add(levelItem);
                var levelCardInfo = levelItem.GetComponent<LevelCardInfo>();

                _levelCardInfos.Add(levelCardInfo);

                levelCardInfo.RectTransform.anchoredPosition =
                    new Vector2(-Mathf.Cos(ArcStep * i) * CircleRadius,
                        -Mathf.Sin(ArcStep * i) * CircleRadius);

                levelCardInfo.LevelIndex = i;

                FillLevelCard(_levelCardInfos[i], _filteredLevels[i]);
            }

            _currentLevelIndex = -1;
            Render();
        }

        public void RemoveLevel(LevelData level)
        {
            var index = _filteredLevels.IndexOf(level);
            if (index == -1) return;

            _filteredLevels.RemoveAt(index);

            if (index == _filteredLevels.Count) _offset--;

            var cardIndex = index - _levelCardInfos[0].LevelIndex;
            FreeLevelCardResources(_levelCardInfos[cardIndex]);

            if (_levelCardInfos[_levelCardInfos.Count - 1].LevelIndex >= _filteredLevels.Count)
            {
                if (_filteredLevels.Count >= PoolSize)
                {
                    MoveBottomCardToTop();
                }
                else
                {
                    Destroy(_levelCards[_levelCards.Count - 1]);
                    _levelCardInfos.RemoveAt(_levelCardInfos.Count - 1);
                    _levelCards.RemoveAt(_levelCards.Count - 1);
                }
            }

            foreach (var levelCardInfo in _levelCardInfos)
            {
                FillLevelCard(levelCardInfo, _filteredLevels[levelCardInfo.LevelIndex]);
            }

            _currentLevelIndex = -1;
            Render();
        }


        private void FillLevelCard(LevelCardInfo levelCardInfo, LevelData levelData)
        {
            levelCardInfo.ArtistName.text = $"by: {levelData.DisplayArtist}";
            levelCardInfo.Title.text = levelData.DisplayTitle;
            levelCardInfo.CharterName.text = levelData.Charter;

            _levelAssetsManager.ScheduleLevelLoad(levelCardInfo, levelData);
        }

        private void UpdateCurrentLevel(LevelCardInfo levelCardInfo)
        {
            CurrentIDText.text = _filteredLevels[levelCardInfo.LevelIndex].ID;
            _levelList.Behaviour.UpdateBackground(levelCardInfo.OriginalBackgroundPath);
            _levelList.Behaviour.UpdateMusic(levelCardInfo.PreviewAudioHandle);
            _levelList.ChartCardController.UpdateChartCards(_filteredLevels[levelCardInfo.LevelIndex]);
        }

        public void Render()
        {
            if (_filteredLevels.Count == 0)
            {
                RenderEmptyList();
                return;
            }

            if (_helpText.activeSelf) _helpText.SetActive(false);

            var wholeOffset = (int)_offset;
            var fractionalOffset = wholeOffset - _offset;

            var currentLevelCard = GetCurrentLevelCard();

            while (_levelCardInfos[_levelCardInfos.Count - 1].LevelIndex + 1 < _filteredLevels.Count
                   && wholeOffset > PoolSize / 2
                   && _lastRenderedOffset < wholeOffset)
            {
                MoveTopCardToBottom();
            }

            while (_levelCardInfos[0].LevelIndex > 0
                   && wholeOffset + PoolSize / 2 < _filteredLevels.Count
                   && _lastRenderedOffset > wholeOffset)
            {
                MoveBottomCardToTop();
            }

            for (var i = 0; i < PoolSize && i < _levelCardInfos.Count; i++)
            {
                _levelCardInfos[i].RectTransform.anchoredPosition =
                    new Vector2(
                        -Mathf.Cos(ArcStep * (i - currentLevelCard + fractionalOffset)) * CircleRadius,
                        -Mathf.Sin(ArcStep * (i - currentLevelCard + fractionalOffset)) * CircleRadius
                    );
            }

            _lastRenderedOffset = (int)_offset;

            if (_currentLevelIndex != (int)_offset)
            {
                _currentLevelIndex = (int)_offset;
                UpdateCurrentLevel(_levelCardInfos[currentLevelCard]);
            }

            _levelCardInfos[currentLevelCard].RectTransform
                .anchoredPosition += new Vector2(-50, 0);
        }

        private int GetCurrentLevelCard()
        {
            int currentLevelCard;
            if (_offset < PoolSize / 2)
            {
                currentLevelCard = (int)_offset;
            }
            else if (_offset < _filteredLevels.Count - PoolSize / 2)
            {
                currentLevelCard = PoolSize / 2;
            }
            else
            {
                currentLevelCard = (int)_offset - _filteredLevels.Count + Mathf.Min(PoolSize, _filteredLevels.Count);
            }

            return currentLevelCard;
        }

        public void FreeResources()
        {
            foreach (var levelCardInfo in _levelCardInfos)
            {
                FreeLevelCardResources(levelCardInfo);
            }
        }

        private static void FreeLevelCardResources(LevelCardInfo levelCardInfo)
        {
            Bass.StreamFree(levelCardInfo.PreviewAudioHandle);
        }

        private void MoveBottomCardToTop()
        {
            _lastRenderedOffset--;
            _levelCardInfos.Insert(0, _levelCardInfos[_levelCardInfos.Count - 1]);
            _levelCardInfos.RemoveAt(_levelCardInfos.Count - 1);

            _levelCardInfos[0].LevelIndex = _levelCardInfos[1].LevelIndex - 1;

            _levelCardInfos[0].RectTransform.SetAsFirstSibling();

            FillLevelCard(_levelCardInfos[0], _filteredLevels[_levelCardInfos[0].LevelIndex]);
        }

        private void MoveTopCardToBottom()
        {
            _lastRenderedOffset++;
            _levelCardInfos.Insert(_levelCardInfos.Count, _levelCardInfos[0]);
            _levelCardInfos.RemoveAt(0);

            _levelCardInfos[_levelCardInfos.Count - 1].LevelIndex =
                _levelCardInfos[_levelCardInfos.Count - 2].LevelIndex + 1;

            _levelCardInfos[_levelCardInfos.Count - 1].RectTransform.SetAsLastSibling();

            FillLevelCard(_levelCardInfos[_levelCardInfos.Count - 1],
                _filteredLevels[_levelCardInfos[_levelCardInfos.Count - 1].LevelIndex]);
        }

        private void RenderEmptyList()
        {
            _helpText.SetActive(true);
        }
    }
}