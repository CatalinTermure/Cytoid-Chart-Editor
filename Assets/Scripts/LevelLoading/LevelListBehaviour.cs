using System.Collections;
using System.IO;
using CCE.Audio.Abstract;
using CCE.Core;
using CCE.Coroutines;
using UnityEngine;
using UnityEngine.UI;

namespace CCE.LevelLoading
{
    public class LevelListBehaviour : MonoBehaviour
    {
        private const float ScrollSpeed = 0.01f;

        private const float SearchUpdateDelay = 3.0f;

        public const float UpdateBackgroundDelay = 0.6f;
        [SerializeField] public InputField SearchInputField;
        [SerializeField] private GameObject LevelCardTemplate;
        [SerializeField] private GameObject LevelMetadataPopup;

        [SerializeField] private BackgroundManager ScreenBackground;
        [SerializeField] private Sprite DefaultBackground;

        private bool _isDragging;

        private bool _isPopupActive;

        private LevelList _levelList;
        private float _startDragOffset;

        private Vector3 _startDragPosition;
        private IEnumerator _triggerSearchCoroutine;
        private IEnumerator _updateBackgroundCoroutine;
        private IEnumerator _updateMusicCoroutine;

        private void Awake()
        {
            _levelList = gameObject.GetComponent<LevelList>();
            if (_levelList == null)
            {
                Debug.LogError(
                    "You must have a LevelList component attached to an object that has a LevelListBehaviour");
            }
        }

        private void Start()
        {
            SearchInputField.onValueChanged
                .AddListener(query => TriggerSearch(query, SearchUpdateDelay));

            SearchInputField.onEndEdit
                .AddListener(query => TriggerSearch(query, UpdateBackgroundDelay));
        }

        private void Update()
        {
            if (_isPopupActive)
            {
                _isDragging = false;
                return;
            }

            if (Input.GetMouseButtonDown(0) && Input.mousePosition.x > Screen.width / 2.0f)
            {
                _isDragging = true;
                _startDragOffset = _levelList.View.Offset;
                _startDragPosition = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(0))
            {
                _isDragging = false;
            }

            if (_isDragging)
            {
                _levelList.View.Offset = _startDragOffset +
                                         (Input.mousePosition.y - _startDragPosition.y) * ScrollSpeed;

                _levelList.View.Render();
            }
        }

        private void OnDisable()
        {
            _levelList.View.FreeResources();
        }

        private void TriggerSearch(string query, float delay)
        {
            if (_triggerSearchCoroutine != null) StopCoroutine(_triggerSearchCoroutine);
            _triggerSearchCoroutine = SearchCoroutine(query, delay);
            StartCoroutine(_triggerSearchCoroutine);
        }

        private IEnumerator SearchCoroutine(string query, float delay)
        {
            yield return new WaitForSeconds(delay);

            _levelList.Query(query);
        }

        public void ShowLevelMetadataPopup(string audioPath)
        {
            _isPopupActive = true;
            var audioPopupController = Instantiate(LevelMetadataPopup, Vector3.zero, Quaternion.identity, transform.parent)
                .GetComponent<LevelMetadataPopupController>();
            audioPopupController.SetAudioFile(audioPath);
            audioPopupController.OnEnd += () =>
            {
                StartCoroutine(CoroutineUtils.WaitForSecondsAndThen(0.5f, () => { _isPopupActive = false; }));
            };
        }

        public void UpdateBackground(string path)
        {
            if (_updateBackgroundCoroutine != null) StopCoroutine(_updateBackgroundCoroutine);
            _updateBackgroundCoroutine =
                File.Exists(path) ? UpdateBackgroundCoroutine(path) : SetToDefaultBackgroundCoroutine();

            StartCoroutine(_updateBackgroundCoroutine);
        }

        public void UpdateMusic(IAudioStream audio)
        {
            if (_updateMusicCoroutine != null) StopCoroutine(_updateMusicCoroutine);
            _updateMusicCoroutine = UpdateMusicCoroutine(audio);
            StartCoroutine(_updateMusicCoroutine);
        }

        private static IEnumerator UpdateMusicCoroutine(IAudioStream audio)
        {
            GlobalState.AudioManager.Stop();

            yield return new WaitForSeconds(UpdateBackgroundDelay);

            GlobalState.AudioManager.LoadAudio(audio);
            GlobalState.AudioManager.Time = 0;
            GlobalState.AudioManager.Play();
        }

        private IEnumerator UpdateBackgroundCoroutine(string path)
        {
            yield return new WaitForSeconds(UpdateBackgroundDelay);
            var tex = new Texture2D(1, 1);
            tex.LoadImage(File.ReadAllBytes(path));
            SetBackground(Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero));
        }

        private IEnumerator SetToDefaultBackgroundCoroutine()
        {
            yield return new WaitForSeconds(UpdateBackgroundDelay);
            SetBackground(DefaultBackground);
        }

        private void SetBackground(Sprite sprite)
        {
            ScreenBackground.ChangeBackground(sprite);
        }
    }
}