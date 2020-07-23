using System;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;

public class GlobalState : MonoBehaviour
{
    /// <summary>
    /// Distance, in Unity units, from the center of the screen to the top/bottom edge of the screen.
    /// </summary>
    public const float Height = 10;

    /// <summary>
    /// Distance, in Unity units, from the center of the screen to the left/right edge of the screen.
    /// </summary>
    public static readonly float Width = Screen.width / (Screen.height / Height);

    /// <summary>
    /// The dimensions of the play area, in Unity units.
    /// </summary>
    public const float PlayAreaWidth = 24, PlayAreaHeight = 12;

    public static float HitsoundVolume = 0.25f;

    public const float DefaultNoteSize = 2;

    public static Sprite BackgroundSprite = null;

    public static string DefaultRingColor = "#FFFFFF";
    public static string[] DefaultFillColors = new string[12] { "#35A7FF", "#FF5964", "#39E59E", "#39E59E", "#35A7FF", "#FF5964", "#F2C85A", "#F2C85A", "#35A7FF", "#FF5964", "#39E59E", "#39E59E" };

    /// <summary>
    /// Path to the directory in which levels are stored.
    /// </summary>
    public static string DirPath = "";

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
        get => -CurrentChart.music_offset;
    }

    public static bool IsGameRunning = false;

    private void Awake()
    {
        // Initialize DirPath
        if(DirPath == "")
        {
            if(File.Exists(Path.Combine(Application.persistentDataPath, "data.txt")))
            {
                DirPath = File.ReadAllLines(Path.Combine(Application.persistentDataPath, "data.txt"))[0];
            }
            else
            {
                Debug.Log("CCELog: Data file not found, creating it...");
                File.WriteAllText(Path.Combine(Application.persistentDataPath, "data.txt"), Application.persistentDataPath);
                DirPath = Application.persistentDataPath;
            }
        }
        if (File.Exists(Path.Combine(Application.persistentDataPath, "data.txt")))
        {
            string[] lines = File.ReadAllLines(Path.Combine(Application.persistentDataPath, "data.txt"));
            if(lines.Length > 1)
            {
                HitsoundVolume = float.Parse(lines[1]);
            }
        }
        else
        {
            HitsoundVolume = 0.25f;
        }
    }

    /// <summary>
    /// Sets the default directory in which levels are stored and stores it into the config file.
    /// </summary>
    /// <param name="path"> Path to the directory to be set as default </param>
    public static void SetDefaultFolder(string path)
    {
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "data.txt"), path + "\n" + HitsoundVolume.ToString());
    }

    public static void SaveHitsoundVolume()
    {
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "data.txt"), DirPath + "\n" + HitsoundVolume.ToString());
    }

    /// <summary>
    /// Loads a level file and sets the current path to search for files related to the level.
    /// </summary>
    /// <param name="level"> The <see cref="LevelData"/> to load the level from. </param>
    /// <param name="path"> The path of the level file. </param>
    public static void LoadLevel(LevelData level, string path)
    {
        CurrentLevel = level;
        CurrentLevelPath = Path.GetDirectoryName(path);
        LoadBackground();
    }

    public static void LoadAudio()
    {
        MusicManager = new AudioManager(Path.Combine(CurrentLevelPath, CurrentChart.Data.music_override?.path ?? CurrentLevel.music.path));
    }

    /// <summary>
    /// Creates level file and loads it from a music file.
    /// </summary>
    /// <param name="musicPath"> Path to the music file. </param>
    public static void CreateLevel(string musicPath)
    {
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
        LoadBackground();
    }

    public static void LoadChart(LevelData.ChartData chart)
    {
        CurrentChart = new Chart(JsonUtility.FromJson<ChartJSON>(File.ReadAllText(Path.Combine(CurrentLevelPath, chart.path))), chart);
    }

    public static void CreateChart()
    {
        LevelData.ChartData chart;
        CurrentLevel.charts.Add(chart = new LevelData.ChartData
        {
            type = "easy",
            difficulty = 0,
            path = "PLACEHOLDER.json"
        });
        File.WriteAllText(Path.Combine(CurrentLevelPath, "PLACEHOLDER.json"), "{\"format_version\":0,\"time_base\":480,\"start_offset_time\":0,\"page_list\":[{\"start_tick\":0,\"end_tick\":480,\"scan_line_direction\":-1}],\"tempo_list\":[{\"tick\":0,\"value\":1000000}],\"event_order_list\":[],\"note_list\":[]}");
        CurrentChart = new Chart(JsonUtility.FromJson<ChartJSON>(File.ReadAllText(Path.Combine(CurrentLevelPath, chart.path))), chart);
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
