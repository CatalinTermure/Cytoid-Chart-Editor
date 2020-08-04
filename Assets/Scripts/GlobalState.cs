using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GlobalState : MonoBehaviour
{
    /// <summary>
    /// Distance, in Unity units, from the center of the screen to the top/bottom edge of the screen.
    /// </summary>
    public static float Height;

    /// <summary>
    /// Distance, in Unity units, from the center of the screen to the left/right edge of the screen.
    /// </summary>
    public static float Width;

    public static readonly float AspectRatio = (float)Screen.width / Screen.height;
    public const float NormalAspectRatio = 16f / 9f;

    /// <summary>
    /// The dimensions of the play area, in Unity units.
    /// </summary>
    public static float PlayAreaWidth, PlayAreaHeight;

    public static EditorConfig Config;

    public static Sprite BackgroundSprite = null;

    public static string DefaultRingColor = "#FFFFFF";
    public static string[] DefaultFillColors = new string[12] { "#35A7FF", "#FF5964", "#39E59E", "#39E59E", "#35A7FF", "#FF5964", "#F2C85A", "#F2C85A", "#35A7FF", "#FF5964", "#39E59E", "#39E59E" };

    /// <summary>
    /// The current path to use for relative paths referenced in the level.json
    /// </summary>
    public static string CurrentLevelPath = null;

    public static LevelData CurrentLevel = null;
    public static Chart CurrentChart = null;

    /// <summary>
    /// The <see cref="AudioManager"/> that is responsible for playing music.
    /// </summary>
    public static AudioManager MusicManager;

    /// <summary>
    /// AudioClip holding the currently selected hitsound.
    /// </summary>
    public static AudioClip Hitsound;
    
    public static double Offset
    {
        get => CurrentChart.music_offset - Config.UserOffset / 1000.0;
    }

    public static bool IsGameRunning = false;

    #if CCE_DEBUG
    private static string LogPath;
    #endif
    private void Awake()
    {
        #if CCE_DEBUG
        LogPath = Path.Combine(Application.persistentDataPath, "LoadChartLog.txt");
        if(CurrentChart == null)
        {
            File.WriteAllText(LogPath, "Starting level loading log...\n");
        }
        #endif

        Height = Camera.main.orthographicSize;
        Width = AspectRatio * Height;
        PlayAreaWidth = 24 * AspectRatio / NormalAspectRatio;
        PlayAreaHeight = 12 * AspectRatio / NormalAspectRatio;
        
        if(File.Exists(Path.Combine(Application.persistentDataPath, "data.txt")))
        {
            try
            {
                Config = JsonConvert.DeserializeObject<EditorConfig>(File.ReadAllText(Path.Combine(Application.persistentDataPath, "data.txt")));
            }
            catch(Exception)
            {
                Config = new EditorConfig();
            }
            if(Config == null)
            {
                Config = new EditorConfig();
            }
        }
        else
        {
            Config = new EditorConfig();
        }
    }

    public static void SaveConfig()
    {
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "data.txt"), JsonConvert.SerializeObject(Config));
    }

    /// <summary>
    /// Loads a level file and sets the current path to search for files related to the level.
    /// </summary>
    /// <param name="level"> The <see cref="LevelData"/> to load the level from. </param>
    /// <param name="path"> The path of the level file. </param>
    public static void LoadLevel(LevelData level, string path)
    {
        #if CCE_DEBUG
        File.AppendAllText(LogPath, $"Starting the load of level at path: {path}\n");
        #endif

        CurrentLevel = level;
        CurrentLevelPath = Path.GetDirectoryName(path);

        LoadBackground();

        #if CCE_DEBUG
        File.AppendAllText(LogPath, "Loaded background...\n");
        #endif
    }

    public static void LoadAudio()
    {
        #if CCE_DEBUG
        File.AppendAllText(LogPath, "Trying to load music...\n");
        #endif

        if (File.Exists(Path.Combine(CurrentLevelPath, CurrentChart.Data.music_override?.path ?? CurrentLevel.music.path)))
        {
            #if CCE_DEBUG
            File.AppendAllText(LogPath, $"Loading music at path: {CurrentChart.Data.music_override?.path ?? CurrentLevel.music.path}\n");
            #endif

            MusicManager = new AudioManager(Path.Combine(CurrentLevelPath, CurrentChart.Data.music_override?.path ?? CurrentLevel.music.path));

            #if CCE_DEBUG
            File.AppendAllText(LogPath, "Loaded music successfully...\n");
            #endif
        }
    }

    /// <summary>
    /// Creates level file and loads it from a music file.
    /// </summary>
    /// <param name="musicPath"> Path to the music file. </param>
    public static void CreateLevel(string musicPath)
    {
        #if CCE_DEBUG
        File.AppendAllText(LogPath, $"Starting creation of a level from the music file at {musicPath}\n");
        #endif

        CurrentLevelPath = Path.GetDirectoryName(musicPath);
        CurrentLevel = new LevelData()
        {
            title = "PLACEHOLDER",
            id = "PLACEHOLDER",
            artist = "PLACEHOLDER",
            illustrator = "PLACEHOLDER",
            background = new LevelData.BackgroundData() { path = "background.jpg" },
            music_preview = new LevelData.MusicData() { path = "preview.ogg" },
            charter = "PLACEHOLDER",
            storyboarder = "PLACEHOLDER",
            music = new LevelData.MusicData() { path = Path.GetFileName(musicPath) }
        };

        #if CCE_DEBUG
        File.AppendAllText(LogPath, "Finished creating the level...\n");
        #endif

        LoadBackground();

        #if CCE_DEBUG
        File.AppendAllText(LogPath, "Loaded Background...\n");
        #endif
    }

    public static void LoadChart(LevelData.ChartData chart)
    {
        CurrentChart = new Chart(JsonUtility.FromJson<ChartJSON>(File.ReadAllText(Path.Combine(CurrentLevelPath, chart.path))), chart);
    }

    public static void CreateChart()
    {
        #if CCE_DEBUG
        File.AppendAllText(LogPath, "Creating a new chart...\n");
        #endif

        LevelData.ChartData chart;
        CurrentLevel.charts.Add(chart = new LevelData.ChartData
        {
            type = "easy",
            difficulty = 0,
            path = "chart.json"
        });
        CurrentChart = new Chart(JsonUtility.FromJson<ChartJSON>("{\"format_version\":0,\"time_base\":480,\"start_offset_time\":0,\"page_list\":[{\"start_tick\":0,\"end_tick\":480,\"scan_line_direction\":-1}],\"tempo_list\":[{\"tick\":0,\"value\":1000000}],\"event_order_list\":[],\"note_list\":[]}"), chart);

        #if CCE_DEBUG
        File.AppendAllText(LogPath, "New chart created...");
        #endif
    }

    public static void LoadBackground()
    {
        if (File.Exists(Path.Combine(CurrentLevelPath, CurrentLevel.background.path)))
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(File.ReadAllBytes(Path.Combine(CurrentLevelPath, CurrentLevel.background.path)));
            BackgroundSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        }
        else
        {
            BackgroundSprite = null;
        }
    }

    public static bool ApproximatelyEqual(double a, double b)
    {
        return a - b < 0.000001;
    }

    public static float GetDistance(float x1, float y1, float x2, float y2)
    {
        return (float)Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
    }

    /// <summary>
    /// Gets the page that contains <paramref name="time"/>.
    /// </summary>
    /// <param name="time"> Time to snap to the page, in seconds. </param>
    /// <returns> Index of the page containing <paramref name="time"/> </returns>
    public static int SnapTimeToPage(double time)
    {
        List<Page> p = CurrentChart.page_list;
        int l = 0, cnt = p.Count, step, i;

        while(cnt > 0)
        {
            step = cnt / 2;
            i = l + step;
            if(p[i].start_time < time)
            {
                l = i;
                cnt -= step + 1;
            }
            else
            {
                cnt = step;
            }
        }

        while(l + 1 < p.Count && p[l + 1].start_time < time)
        {
            l++;
        }
        // change this to be more efficient
        return l;
    }

    public static T Clamp<T>(T val, T l, T r) where T : IComparable
    {
        if(val.CompareTo(l) == -1)
        {
            val = l;
        }
        else if(val.CompareTo(r) == 1)
        {
            val = r;
        }
        return val;
    }
}
