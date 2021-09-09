using System.Collections.Generic;
using CCE.Data;
using ManagedBass;
using UnityEngine;
using UnityEngine.UI;

namespace CCE.LevelLoading
{
    public class LevelListView : MonoBehaviour
    {
        [SerializeField] private GameObject LevelListCenter;
        [SerializeField] private GameObject LevelItemPrefab;
        [SerializeField] private Text CurrentIDText;
        
        private const int _poolSize = 24;
        private const float _arcStep = Mathf.PI / 80;
        private const float _circleRadius = 3000;

        private LevelAssetsManager _levelAssetsManager;

        private List<LevelData> _filteredLevels = new List<LevelData>();

        private int _currentLevelIndex = -1;

        private GameObject _helpText;
        private int _lastRenderedOffset = _poolSize / 2;
        private List<LevelCardInfo> _levelCardInfos = new List<LevelCardInfo>();
        private List<GameObject> _levelCards = new List<GameObject>();
        private LevelList _levelList;

        private float _offset;

        /// <summary>
        ///     Position of the level in the level list that the center level card represents.
        ///     <para>Integer part of the float represents the index of the level.</para>
        ///     <para>
        ///         Fractional part represents how much the middle level card is displaced
        ///         upwards from the middle of the screen, in respect to one <see cref="_arcStep" />.
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

            for (int i = 0; i < _poolSize && i < _filteredLevels.Count; i++)
            {
                GameObject levelItem = Instantiate(LevelItemPrefab, listCenterTransform);
                _levelCards.Add(levelItem);
                var levelCardInfo = levelItem.GetComponent<LevelCardInfo>();

                _levelCardInfos.Add(levelCardInfo);

                levelCardInfo.RectTransform.anchoredPosition =
                    new Vector2(-Mathf.Cos(_arcStep * i) * _circleRadius,
                        -Mathf.Sin(_arcStep * i) * _circleRadius);

                levelCardInfo.LevelIndex = i;

                FillLevelCard(_levelCardInfos[i], _filteredLevels[i]);
            }

            _currentLevelIndex = -1;
            Render();
        }

        public void RemoveLevel(LevelData level)
        {
            int index = _filteredLevels.IndexOf(level);
            if (index == -1) return;

            _filteredLevels.RemoveAt(index);
            
            if (index == _filteredLevels.Count) _offset--;

            int cardIndex = index - _levelCardInfos[0].LevelIndex;
            FreeLevelCardResources(_levelCardInfos[cardIndex]);

            if (_levelCardInfos[_levelCardInfos.Count - 1].LevelIndex >= _filteredLevels.Count)
            {
                if (_filteredLevels.Count >= _poolSize)
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

            foreach(var levelCardInfo in _levelCardInfos)
                FillLevelCard(levelCardInfo, _filteredLevels[levelCardInfo.LevelIndex]);

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

            int wholeOffset = (int) _offset;
            float fractionalOffset = wholeOffset - _offset;

            int currentLevelCard = GetCurrentLevelCard();

            while (_levelCardInfos[_levelCardInfos.Count - 1].LevelIndex + 1 < _filteredLevels.Count
                   && wholeOffset > _poolSize / 2
                   && _lastRenderedOffset < wholeOffset)
            {
                MoveTopCardToBottom();
            }

            while (_levelCardInfos[0].LevelIndex > 0
                   && wholeOffset + _poolSize / 2 < _filteredLevels.Count
                   && _lastRenderedOffset > wholeOffset)
            {
                MoveBottomCardToTop();
            }

            for (int i = 0; i < _poolSize && i < _levelCardInfos.Count; i++)
            {
                _levelCardInfos[i].RectTransform.anchoredPosition =
                    new Vector2(
                        -Mathf.Cos(_arcStep * (i - currentLevelCard + fractionalOffset)) * _circleRadius,
                        -Mathf.Sin(_arcStep * (i - currentLevelCard + fractionalOffset)) * _circleRadius
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
            if (_offset < _poolSize / 2)
            {
                currentLevelCard = (int)_offset;
            }
            else if (_offset < _filteredLevels.Count - _poolSize / 2)
            {
                currentLevelCard = _poolSize / 2;
            }
            else
            {
                currentLevelCard = (int)_offset - _filteredLevels.Count + Mathf.Min(_poolSize, _filteredLevels.Count);
            }

            return currentLevelCard;
        }

        public void FreeResources()
        {
            foreach (LevelCardInfo levelCardInfo in _levelCardInfos)
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