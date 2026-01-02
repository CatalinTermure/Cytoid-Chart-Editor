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
        private ChartObjectPool _chartObjectPool16x9;
        private ChartObjectPool _chartObjectPool4x3;
        private NoteSpawner _noteSpawner16x9;
        private NoteSpawner _noteSpawner4x3;
        private PlaybackRenderer _playbackRenderer16x9;
        private PlaybackRenderer _playbackRenderer4x3;
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
            var chartToScreenCoordinatesConverter16x9 = new ChartToScreenCoordinatesConverter(GetScreenRect(new Vector2(3.5f, 0), (float)16.0f / 9.0f, 5.0f));
            var chartToScreenCoordinatesConverter4x3 = new ChartToScreenCoordinatesConverter(GetScreenRect(new Vector2(-5f, 0), (float)4.0f / 3.0f, 5.0f));
            _chart = CurrentChartProvider.CurrentChart;
            _noteTimingCalculator = new NoteTimingCalculator(_chart);
            _noteVisualsCalculator = new NoteVisualsCalculator(_chart);
            _chartObjectPool16x9 = new ChartObjectPool(_notePrefabs);
            _chartObjectPool4x3 = new ChartObjectPool(_notePrefabs);
            _noteSpawner16x9 = new NoteSpawner(_chartObjectPool16x9, _chart, chartToScreenCoordinatesConverter16x9);
            _noteSpawner4x3 = new NoteSpawner(_chartObjectPool4x3, _chart, chartToScreenCoordinatesConverter4x3);
            _playbackRenderer16x9 = new PlaybackRenderer(_noteSpawner16x9, chartToScreenCoordinatesConverter16x9);
            _playbackRenderer4x3 = new PlaybackRenderer(_noteSpawner4x3, chartToScreenCoordinatesConverter4x3);
            _audioManager = AudioManagerProvider.AudioManager;
        }

        void Update()
        {
            double time = AudioManagerProvider.AudioManager.Time + _chart.MusicOffset;
            _noteSpawner16x9.UpdateTime(time);
            _playbackRenderer16x9.Render(time);
            _noteSpawner4x3.UpdateTime(time);
            _playbackRenderer4x3.Render(time);
        }
    }
}
