using CCE.Audio;
using CCE.Data;
using CCE.Rendering;
using CCE.Rendering.Notes;
using UnityEngine;

namespace CCE.Storyboard
{
    public class StoryboardSceneController : MonoBehaviour
    {
        [SerializeField] private NotePrefabs _notePrefabs;
        private ChartObjectPool _chartObjectPool;
        private NoteSpawner _noteSpawner;
        private PlaybackRenderer _playbackRenderer;
        private Chart _chart;
        private NoteTimingCalculator _noteTimingCalculator;
        private NoteVisualsCalculator _noteVisualsCalculator;
        private IAudioManager _audioManager;

        public void TogglePlayPause()
        {
            if (_audioManager.IsPlaying)
            {
                _audioManager.Pause();
            }
            else
            {
                _audioManager.Play();
            }
        }

        public void SetTime(float timePercentage)
        {
            _audioManager.Time = _audioManager.MaxTime * timePercentage;
        }

        private Rect GetScreenRect(Vector2 center, float aspectRatio, float height)
        {
            float width = aspectRatio * height;
            float x = center.x - width / 2.0f;
            float y = center.y - height / 2.0f;
            return new Rect(x, y, width, height);
        }

        void Awake()
        {
            var chartToScreenCoordinatesConverter = new ChartToScreenCoordinatesConverter(GetScreenRect(new Vector2(2.5f, 0), (float)Screen.width / Screen.height, 5.0f));
            _chart = CurrentChartProvider.CurrentChart;
            _noteTimingCalculator = new NoteTimingCalculator(_chart);
            _noteVisualsCalculator = new NoteVisualsCalculator(_chart);
            _chartObjectPool = new ChartObjectPool(_notePrefabs);
            _noteSpawner = new NoteSpawner(_chartObjectPool, _chart, chartToScreenCoordinatesConverter);
            _playbackRenderer = new PlaybackRenderer(_noteSpawner, chartToScreenCoordinatesConverter);
            _audioManager = AudioManagerProvider.AudioManager;
        }

        void Update()
        {
            double time = AudioManagerProvider.AudioManager.Time;
            _noteSpawner.UpdateTime(time);
            _playbackRenderer.Render(time);
        }
    }
}
