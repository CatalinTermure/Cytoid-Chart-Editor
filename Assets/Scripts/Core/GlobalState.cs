using System.IO;
using CCE.Audio;
using CCE.Data;
using CCE.GameUtils;
using UnityEngine;

namespace CCE.Core
{
    public class GlobalState : MonoBehaviour
    {
        /// <summary>
        /// The width of the play area in Unity units.
        /// </summary>
        public static float PlayAreaWidth => ScreenDimensionsProvider.PlayAreaWidth;

        /// <summary>
        /// The height of the play area in Unity units.
        /// </summary>
        public static float PlayAreaHeight => ScreenDimensionsProvider.PlayAreaHeight;

        public static EditorConfig Config => EditorConfigProvider.Config;

        public static IAudioManager AudioManager => AudioManagerProvider.AudioManager;

        private static Sprite _backgroundSprite;

        public static Level CurrentLevel => CurrentChartProvider.CurrentLevel;
        public static Chart CurrentChart => CurrentChartProvider.CurrentChart;

        public static bool IsGameRunning = false;

#if UNITY_STANDALONE
        private static bool _loadedHotkeys;
#endif

        /// <summary>
        ///     The current path to use for relative paths referenced in the level.json
        /// </summary>
        public static string CurrentLevelPath => Path.Combine(Config.LevelStoragePath, CurrentLevel.ID);

        public static double Offset => CurrentChart.MusicOffset - Config.UserOffset / 1000.0;

        private void Awake()
        {
            Application.targetFrameRate = 60;

#if UNITY_STANDALONE
            if (!_loadedHotkeys)
            {
                HotkeyManager.LoadCustomHotkeys();
                _loadedHotkeys = true;
            }
#endif
        }

        public static void LoadLevel(Level level)
        {
            CurrentChartProvider.CurrentLevel = level;

            LoadBackground();
        }

        public static void LoadChart(ChartMetadata chartMetadata)
        {
            CurrentChartProvider.CurrentChart = LevelLoader.LoadChart(Path.Combine(CurrentLevelPath, chartMetadata.Path), chartMetadata);
        }

        public static void LoadBackground()
        {
            if (File.Exists(Path.Combine(CurrentLevelPath, CurrentLevel.Background.Path)))
            {
                var tex = new Texture2D(1, 1);
                tex.LoadImage(File.ReadAllBytes(Path.Combine(CurrentLevelPath, CurrentLevel.Background.Path)));
                _backgroundSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                BackgroundManager.BackgroundOverride = _backgroundSprite;
            }
            else
            {
                _backgroundSprite = null;
            }
        }
    }
}