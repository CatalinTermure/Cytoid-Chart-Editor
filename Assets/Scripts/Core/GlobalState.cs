using System;
using System.Collections.Generic;
using System.IO;
using CCE.Data;
using ManagedBass;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace CCE.Core
{
    public class GlobalState : MonoBehaviour
    {
        public enum NoteInfo
        {
            NoteX,
            NoteY,
            NoteID
        }

        public const float NormalAspectRatio = 16f / 9f;

        /// <summary>
        ///     Distance, in Unity units, from the center of the screen to the top/bottom edge of the screen.
        /// </summary>
        public static float Height;

        /// <summary>
        ///     Distance, in Unity units, from the center of the screen to the left/right edge of the screen.
        /// </summary>
        public static float Width;

        /// <summary>
        ///     The dimension of the play area, in Unity units.
        /// </summary>
        public static float PlayAreaWidth, PlayAreaHeight;

        public static readonly float AspectRatio = (float) Screen.width / Screen.height;

        public static EditorConfig Config;

        private static Sprite _backgroundSprite;

        public static string DefaultRingColor = "#FFFFFF";

        public static readonly string[] DefaultFillColors =
        {
            "#35A7FF", "#FF5964", "#39E59E", "#39E59E", "#35A7FF", "#FF5964", "#F2C85A", "#F2C85A", "#35A7FF",
            "#FF5964", "#39E59E", "#39E59E"
        };

        public static readonly int[] ColorIndexes = {0, 4, 6, 2, 2, 8, 10, 10};

        /// <summary>
        ///     The current path to use for relative paths referenced in the level.json
        /// </summary>
        public static string CurrentLevelPath;

        public static LevelData CurrentLevel;
        public static Chart CurrentChart;

        /// <summary>
        ///     AudioClip holding the currently selected hitsound.
        /// </summary>
        public static AudioClip Hitsound;

        public static bool IsGameRunning = false;

        private static string _logPath;

        public const string NewChartString = "{\"format_version\":0,\"time_base\":480,\"start_offset_time\":0,\"page_list\":[{\"start_tick\":0,\"end_tick\":480,\"scan_line_direction\":-1}],\"tempo_list\":[{\"tick\":0,\"value\":1000000}],\"event_order_list\":[],\"note_list\":[]}";

        public static string InAppLogString = "";

#if UNITY_STANDALONE
        private static bool _loadedHotkeys;
#endif
        public static NoteInfo ShownNoteInfo = NoteInfo.NoteX;

        public static double Offset => CurrentChart.MusicOffset - Config.UserOffset / 1000.0;

        private void Awake()
        {
            Application.targetFrameRate = 60;

            Height = Camera.main.orthographicSize;
            Width = AspectRatio * Height;
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

                Config ??= new EditorConfig();
            }
            else
            {
                Config = new EditorConfig();
            }

            if (!Directory.Exists(Config.DirPath))
            {
                Config.DirPath = Application.persistentDataPath;
            }

            if (!Directory.Exists(Config.LevelStoragePath)) Directory.CreateDirectory(Config.LevelStoragePath);
            if (!Directory.Exists(Config.TempStoragePath)) Directory.CreateDirectory(Config.TempStoragePath);

#if UNITY_STANDALONE
            if (!_loadedHotkeys)
            {
                HotkeyManager.LoadCustomHotkeys();
                _loadedHotkeys = true;
            }
#endif

            if (Config.DebugMode)
            {
                _logPath = Path.Combine(Application.persistentDataPath, "LoadChartLog.txt");
                if (CurrentChart == null)
                {
                    Logging.CreateLog(_logPath, "Starting level loading log...\n");
                }
            }
        }

        public static void SaveConfig()
        {
            File.WriteAllText(Path.Combine(Application.persistentDataPath, "data.txt"),
                JsonConvert.SerializeObject(Config));
        }

        /// <summary>
        ///     Loads a level file and sets the current path to search for files related to the level.
        /// </summary>
        /// <param name="level"> The <see cref="LevelData" /> to load the level from. </param>
        /// <param name="path"> The path of the level file. </param>
        public static void LoadLevel(LevelData level, string path)
        {
            Logging.AddToLog(_logPath, $"Starting the load of level at path: {path}\n");

            CurrentLevel = level;
            CurrentLevelPath = Path.GetDirectoryName(path);

            LoadBackground();

            Logging.AddToLog(_logPath, "Loaded background...\n");
        }

        /// <summary>
        ///     Creates level file and loads it from a music file.
        /// </summary>
        /// <param name="musicPath"> Path to the music file. </param>
        /// <returns> Whether the level can be successfully saved in the path of the music file. </returns>
        public static bool CreateLevel(string musicPath)
        {
            Logging.AddToLog(_logPath, $"Starting creation of a level from the music file at {musicPath}\n");

            CurrentLevelPath = Path.GetDirectoryName(musicPath);

            if (File.Exists(Path.Combine(CurrentLevelPath, "level.json")))
            {
                return false;
            }

            CurrentLevel = new LevelData
            {
                Title = "PLACEHOLDER",
                ID = "PLACEHOLDER",
                Artist = "PLACEHOLDER",
                Illustrator = "PLACEHOLDER",
                Background = new LevelData.BackgroundData {Path = "background.jpg"},
                MusicPreview = new LevelData.MusicData {Path = "preview.ogg"},
                Charter = "PLACEHOLDER",
                Storyboarder = "PLACEHOLDER",
                Music = new LevelData.MusicData {Path = Path.GetFileName(musicPath)}
            };


            Logging.AddToLog(_logPath, "Finished creating the level\n");

            LoadBackground();

            Logging.AddToLog(_logPath, "Loaded Background...\n");

            return true;
        }

        public static void LoadChart(LevelData.ChartFileData chart)
        {
            CurrentChart =
                new Chart(JsonConvert.DeserializeObject<ChartData>(File.ReadAllText(Path.Combine(CurrentLevelPath, chart.Path))),
                    chart);
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

        public static bool ApproximatelyEqual(double a, double b)
        {
            return a - b < 1e-5;
        }

        public static float GetDistance(float x1, float y1, float x2, float y2)
        {
            return (float) Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
        }

        public static int GetLastDragChainLink(int index)
        {
            while (CurrentChart.NoteList[index].NextID > 0)
            {
                index = CurrentChart.NoteList[index].NextID;
            }

            return index;
        }

        public static bool WasPressed(KeyValuePair<KeyCode, KeyCode> key)
        {
            if (key.Key == KeyCode.None && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftShift) ||
                                            Input.GetKey(KeyCode.LeftAlt)))
            {
                return false;
            }

            return (key.Key == KeyCode.None || Input.GetKey(key.Key)) &&
                   (key.Value == KeyCode.None || Input.GetKeyDown(key.Value));
        }

        public static bool IsKeyHeld(KeyValuePair<KeyCode, KeyCode> key)
        {
            if (key.Key == KeyCode.None && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftShift) ||
                                            Input.GetKey(KeyCode.LeftAlt)))
            {
                return false;
            }

            return (key.Key == KeyCode.None || Input.GetKeyDown(key.Key)) &&
                   (key.Value == KeyCode.None || Input.GetKeyDown(key.Value));
        }

        /// <summary>
        ///     Gets the page that contains <paramref name="time" />.
        /// </summary>
        /// <param name="time"> Time to snap to the page, in seconds. </param>
        /// <returns> Index of the page containing <paramref name="time" /> </returns>
        public static int SnapTimeToPage(double time)
        {
            List<Page> p = CurrentChart.PageList;
            int l = 0, cnt = p.Count;

            while (cnt > 0)
            {
                int step = cnt / 2;
                int i = l + step;
                if (p[i].ActualStartTime < time)
                {
                    l = i;
                    cnt -= step + 1;
                }
                else
                {
                    cnt = step;
                }
            }

            while (l + 1 < p.Count && p[l + 1].ActualStartTime < time)
            {
                l++;
            }

            // TODO: fix this undershooting the page
            return l;
        }

        public static T Clamp<T>(T val, T l, T r) where T : IComparable
        {
            if (val.CompareTo(l) == -1)
            {
                val = l;
            }
            else if (val.CompareTo(r) == 1)
            {
                val = r;
            }

            return val;
        }

        public static void LoadCustomHitsounds(string path)
        {
            var type = AudioType.UNKNOWN;
            switch (Path.GetExtension(path))
            {
                case ".ogg":
                    type = AudioType.OGGVORBIS;
                    break;
                case ".mp3":
                    type = AudioType.MPEG;
                    break;
                case ".wav":
                    type = AudioType.WAV;
                    break;
                default:
                    Debug.LogError("CCELog: Audio file type is unsupported.");
                    break;
            }

            Logging.AddToLog(Path.Combine(Application.persistentDataPath, "LoadChartLog.txt"),
                $"Loading music file from path: file://{path}\n");

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + path, type))
            {
                UnityWebRequestAsyncOperation req = www.SendWebRequest();

                while (!req.isDone)
                {
                }

                if (www.result == UnityWebRequest.Result.ConnectionError ||
                    www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("CCELog: " + www.error);
                }
                else
                {
                    Hitsound = DownloadHandlerAudioClip.GetContent(www);
                }
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
            AudioManager.Stop();
            Bass.Free();
        }
    }
}