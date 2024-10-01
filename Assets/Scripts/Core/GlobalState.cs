using System;
using System.IO;
using CCE.Data;
using CCE.GameUtils;
using Newtonsoft.Json;
using UnityEngine;

namespace CCE.Core
{
    public class GlobalState : MonoBehaviour
    {
        public const float NormalAspectRatio = 16f / 9f;

        public const string AndroidPluginPackageName = "com.chovvy.unityfileutils.FileUtils";

        public const string NewChartString =
            "{\"format_version\":0,\"time_base\":480,\"start_offset_time\":0,\"page_list\":[{\"start_tick\":0,\"end_tick\":480,\"scan_line_direction\":-1}],\"tempo_list\":[{\"tick\":0,\"value\":1000000}],\"event_order_list\":[],\"note_list\":[]}";

        /// <summary>
        ///     Distance, in Unity units, from the center of the screen to the top/bottom edge of the screen.
        /// </summary>
        public static float Height;

        /// <summary>
        ///     The dimension of the play area, in Unity units.
        /// </summary>
        public static float PlayAreaWidth, PlayAreaHeight;

        public static readonly float AspectRatio = (float)Screen.width / Screen.height;

        public static EditorConfig Config;

        private static Sprite _backgroundSprite;

        public static readonly string[] DefaultFillColors =
        {
            "#35A7FF", "#FF5964", "#39E59E", "#39E59E", "#35A7FF", "#FF5964", "#F2C85A", "#F2C85A", "#35A7FF",
            "#FF5964", "#39E59E", "#39E59E"
        };

        public static readonly int[] ColorIndexes = { 0, 4, 6, 2, 2, 8, 10, 10 };

        public static Level CurrentLevel;
        public static Chart CurrentChart;

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

            Height = Camera.main!.orthographicSize;
            PlayAreaWidth = 24 * AspectRatio / NormalAspectRatio;
            PlayAreaHeight = 12;

            if (File.Exists(Path.Combine(Application.persistentDataPath, "data.txt")))
            {
                try
                {
                    Config = JsonConvert.DeserializeObject<EditorConfig>(
                        File.ReadAllText(Path.Combine(Application.persistentDataPath, "data.txt")));
                }
                catch (Exception)
                {
                    Config = new EditorConfig();
                }
            }
            else
            {
                Config = new EditorConfig();
            }

            if (!Directory.Exists(Config.DirPath)) Config.DirPath = Application.persistentDataPath;

            if (!Directory.Exists(Config.LevelStoragePath)) Directory.CreateDirectory(Config.LevelStoragePath);
            if (!Directory.Exists(Config.TempStoragePath)) Directory.CreateDirectory(Config.TempStoragePath);

#if UNITY_STANDALONE
            if (!_loadedHotkeys)
            {
                HotkeyManager.LoadCustomHotkeys();
                _loadedHotkeys = true;
            }
#endif
        }

        private void OnEnable()
        {
            if (AudioManager.IsInitialized) return;
            AudioManager.Initialize();
            AudioManager.SetMusicVolume(Config.MusicVolume);
            AudioManager.SetHitsoundVolume(Config.HitsoundVolume);
        }

        private void OnApplicationQuit()
        {
            AudioManager.Stop();
#if !UNITY_EDITOR
            AudioManager.Cleanup();
#endif
        }

        public static void SaveConfig()
        {
            File.WriteAllText(Path.Combine(Application.persistentDataPath, "data.txt"),
                JsonConvert.SerializeObject(Config));
        }

        public static void LoadLevel(Level level)
        {
            CurrentLevel = level;

            LoadBackground();
        }

        public static void LoadChart(ChartMetadata chartMetadata)
        {
            CurrentChart = LevelLoader.LoadChart(Path.Combine(CurrentLevelPath, chartMetadata.Path), chartMetadata);
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