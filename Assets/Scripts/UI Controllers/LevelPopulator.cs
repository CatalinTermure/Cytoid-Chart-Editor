﻿using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class LevelPopulator : MonoBehaviour
{
    public GameObject LevelListItem, MusicListItem;
    public GameObject ChartList;

    private static readonly Color NormalColor = new Color(1, 1, 1, 0.8f), HighlightColor = new Color(0.35f, 0.4f, 1f, 0.8f);
    private GameObject _currentLevelItem = null;
    public GameObject CurrentLevelItem
    {
        get => _currentLevelItem;
        set
        {
            if (_currentLevelItem != null)
            {
                _currentLevelItem.GetComponent<Image>().color = NormalColor;
            }
            _currentLevelItem = value;
            _currentLevelItem.GetComponent<Image>().color = HighlightColor;
        }
    }

    private int listItemCount = 0;
    private readonly string LogPath = Path.Combine(GlobalState.Config.DirPath, "LevelPopulatorLog.txt");

    private void Start()
    {
        #if CCE_DEBUG
        File.WriteAllText(LogPath, "Starting the log...\n");
        #endif
        PopulateLevels(GlobalState.Config.DirPath);
    }

    /// <summary>
    /// Adds a list item that allows loading a level file.
    /// </summary>
    /// <param name="level"> <see cref="LevelData"/> of the level to load. </param>
    /// <param name="path"> Path of the level.json file. </param>
    private void AddLevel(LevelData level, string path)
    {
        GameObject obj = Instantiate(LevelListItem);
        (obj.transform as RectTransform).SetParent(gameObject.transform);
        (obj.transform as RectTransform).localPosition = new Vector3(0, -100 * listItemCount);
        (obj.transform as RectTransform).sizeDelta = new Vector2(800, 100);
        obj.transform.localScale = Vector3.one;
        obj.GetComponentInChildren<Text>().text = level.id;

        listItemCount++;
        (gameObject.transform as RectTransform).sizeDelta = new Vector2(0, 100 * listItemCount);

        obj.GetComponent<Button>().onClick.AddListener(() =>
        {
            CurrentLevelItem = obj;
            GlobalState.LoadLevel(level, path);
            ChartList.GetComponent<ChartPopulator>().PopulateCharts(level);
        });
    }
    
    /// <summary>
    /// Adds a list item that allows creation of a level from a music file.
    /// </summary>
    /// <param name="musicPath"> Path of the music file. </param>
    private void AddMusicOption(string musicPath)
    {
        GameObject obj = Instantiate(MusicListItem);
        (obj.transform as RectTransform).SetParent(gameObject.transform);
        (obj.transform as RectTransform).localPosition = new Vector3(0, -100 * listItemCount);
        (obj.transform as RectTransform).sizeDelta = new Vector2(800, 100);
        obj.transform.localScale = Vector3.one;
        obj.GetComponentInChildren<Text>().text = GlobalState.Config.DirPath == Path.GetDirectoryName(musicPath) ? Path.GetFileName(musicPath) : Path.Combine(Path.GetFileName(Path.GetDirectoryName(musicPath)), Path.GetFileName(musicPath));

        listItemCount++;
        (gameObject.transform as RectTransform).sizeDelta = new Vector2(0, 100 * listItemCount);

        obj.GetComponent<Button>().onClick.AddListener(() =>
        {
            CurrentLevelItem = obj;
            GlobalState.CreateLevel(musicPath);
            ChartList.GetComponent<ChartPopulator>().PopulateCharts(GlobalState.CurrentLevel);
        });

        #if CCE_DEBUG
        File.AppendAllText(LogPath, "Created music item for file: " + musicPath + "\n");
        #endif
    }

    /// <summary>
    /// Populates the level list with levels and music files to create new levels from. Searches up to one directory level deep.
    /// </summary>
    /// <param name="dirPath"> Path of the directory to search. </param>
    public void PopulateLevels(string dirPath)
    {
        #if CCE_DEBUG
        File.AppendAllText(LogPath, "Starting population of levels...\n");
        #endif

        foreach (Transform child in gameObject.transform)
        {
            Destroy(child.gameObject);
        }

        listItemCount = 0;
        (gameObject.transform as RectTransform).sizeDelta = new Vector2(0, 0);

        #if CCE_DEBUG
        File.AppendAllText(LogPath, "Starting search through root directory...\n");
        #endif

        PopulateLevelsUtil(dirPath);

        foreach(string folder in Directory.EnumerateDirectories(dirPath))
        {
            PopulateLevelsUtil(folder);
        }
    }

    /// <summary>
    /// Utility function for <see cref="PopulateLevels(string)"/>. Searches only files in the directory referenced by <paramref name="path"/>
    /// </summary>
    /// <param name="path"> Path of the directory to search. </param>
    private void PopulateLevelsUtil(string path)
    {
        #if CCE_DEBUG
        File.AppendAllText(LogPath, "Searching directory " + path + "\n");
        #endif

        foreach (string file in Directory.EnumerateFiles(path))
        {
            #if CCE_DEBUG
            File.AppendAllText(LogPath, "Found file " + file + "\n");
            #endif

            if (file.EndsWith("level.json"))
            {
                #if CCE_DEBUG
                File.AppendAllText(LogPath, "Started adding level item for file: " + file + "\n");
                #endif

                LevelData l = JsonUtility.FromJson<LevelData>(File.ReadAllText(file));

                #if CCE_DEBUG
                File.AppendAllText(LogPath, "Level data read.\n");
                #endif

                AddLevel(l, file);
            }
            else if(file.EndsWith(".mp3") || file.EndsWith(".ogg"))
            {
                #if CCE_DEBUG
                File.AppendAllText(LogPath, "Started adding music item for file: " + file + "\n");
                #endif
                AddMusicOption(file);
            }
        }
    }
}
