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



        void Awake()
        {
            _chart = CurrentChartProvider.CurrentChart;
            _noteTimingCalculator = new NoteTimingCalculator(_chart);
            _noteVisualsCalculator = new NoteVisualsCalculator(_chart);
            _chartObjectPool = new ChartObjectPool(_notePrefabs);
            _noteSpawner = new NoteSpawner(_chartObjectPool, _chart);
            _playbackRenderer = new PlaybackRenderer(_noteSpawner);
        }

        void Start()
        {
            AudioManagerProvider.AudioManager.Play();
        }

        void Update()
        {
            double time = AudioManagerProvider.AudioManager.Time;
            _noteSpawner.UpdateTime(time);
            _playbackRenderer.Render(time);
        }
    }
}
