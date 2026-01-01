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

        void Awake()
        {
            var chartToScreenCoordinatesConverter = new ChartToScreenCoordinatesConverter();
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
