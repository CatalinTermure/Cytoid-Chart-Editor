using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System;

public class LevelDataChanger : MonoBehaviour
{
    private void Start()
    {
        GameObject.Find("IDInputField").GetComponent<InputField>().text = GlobalState.CurrentLevel.id;

        GameObject.Find("TitleInputField").GetComponent<InputField>().text = GlobalState.CurrentLevel.title;
        GameObject.Find("TitleLocInputField").GetComponent<InputField>().text = GlobalState.CurrentLevel.title_localized;

        GameObject.Find("ArtistInputField").GetComponent<InputField>().text = GlobalState.CurrentLevel.artist;
        GameObject.Find("ArtistLocInputField").GetComponent<InputField>().text = GlobalState.CurrentLevel.artist_localized;
        GameObject.Find("ArtistSrcInputField").GetComponent<InputField>().text = GlobalState.CurrentLevel.artist_source;

        GameObject.Find("IllustratorInputField").GetComponent<InputField>().text = GlobalState.CurrentLevel.illustrator;
        GameObject.Find("IllustratorSrcInputField").GetComponent<InputField>().text = GlobalState.CurrentLevel.illustrator_source;

        GameObject.Find("CharterInputField").GetComponent<InputField>().text = GlobalState.CurrentLevel.charter;
        GameObject.Find("StoryboarderInputField").GetComponent<InputField>().text = GlobalState.CurrentLevel.storyboarder;

        GameObject.Find("BackgroundInputField").GetComponent<InputField>().text = GlobalState.CurrentLevel.background.path;
        GameObject.Find("MusicPreviewInputField").GetComponent<InputField>().text = GlobalState.CurrentLevel.music_preview.path;
    }

    public void SaveData()
    {
        GlobalState.CurrentLevel.id = GameObject.Find("IDInputField").GetComponent<InputField>().text;

        GlobalState.CurrentLevel.title = GameObject.Find("TitleInputField").GetComponent<InputField>().text;
        GlobalState.CurrentLevel.title_localized = GameObject.Find("TitleLocInputField").GetComponent<InputField>().text;

        GlobalState.CurrentLevel.artist = GameObject.Find("ArtistInputField").GetComponent<InputField>().text;
        GlobalState.CurrentLevel.artist_localized = GameObject.Find("ArtistLocInputField").GetComponent<InputField>().text;
        GlobalState.CurrentLevel.artist_source = GameObject.Find("ArtistSrcInputField").GetComponent<InputField>().text;

        GlobalState.CurrentLevel.illustrator = GameObject.Find("IllustratorInputField").GetComponent<InputField>().text;
        GlobalState.CurrentLevel.illustrator_source = GameObject.Find("IllustratorSrcInputField").GetComponent<InputField>().text;

        GlobalState.CurrentLevel.charter = GameObject.Find("CharterInputField").GetComponent<InputField>().text;
        GlobalState.CurrentLevel.storyboarder = GameObject.Find("StoryboarderInputField").GetComponent<InputField>().text;

        GlobalState.CurrentLevel.background.path = GameObject.Find("BackgroundInputField").GetComponent<InputField>().text;
        GlobalState.CurrentLevel.music_preview.path = GameObject.Find("MusicPreviewInputField").GetComponent<InputField>().text;

        SaveLevel();

        GlobalState.LoadBackground();
    }

    public static void SaveLevel()
    {
        File.WriteAllText(Path.Combine(GlobalState.CurrentLevelPath, "level.json"), JsonConvert.SerializeObject(GlobalState.CurrentLevel, new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        }));
    }

    private void CleanupTempFiles()
    {
        Directory.Delete(Path.Combine(GlobalState.CurrentLevelPath, "tmp"), true);
        File.Delete(Path.Combine(Application.persistentDataPath, GlobalState.CurrentLevel.id + ".cytoidlevel"));
    }

    public void PackageLevel()
    {
        string intermediatepath = Path.Combine(Application.persistentDataPath, GlobalState.CurrentLevel.id + ".cytoidlevel"), finalpath = Path.Combine(GlobalState.CurrentLevelPath, GlobalState.CurrentLevel.id + ".cytoidlevel");
        if (File.Exists(intermediatepath))
        {
            File.Delete(intermediatepath);
        }
        if (!File.Exists(Path.Combine(GlobalState.CurrentLevelPath, GlobalState.CurrentLevel.background.path)))
        {
            GameObject.Find("ToastText").GetComponent<ToastMessageManager>().CreateToast("Please make sure you have a background, otherwise the chart cannot be played in Cytoid.");
        }
        if (File.Exists(Path.Combine(GlobalState.CurrentLevelPath, GlobalState.CurrentChart.Data.storyboard?.path ?? "storyboard.json")))
        { // TODO: Properly parse storyboards and check what should and what shouldn't be added to the .cytoidlevel
            try
            {
                System.IO.Compression.ZipFile.CreateFromDirectory(GlobalState.CurrentLevelPath, intermediatepath);
            }
            catch (Exception e)
            {
                GameObject.Find("ToastText").GetComponent<ToastMessageManager>().CreateToast("Could not zip the .cytoidlevel.");
                File.WriteAllText(Path.Combine(Application.persistentDataPath, "error.log"), e.StackTrace);
                CleanupTempFiles();
                return;
            }
        }
        else
        {
            Directory.CreateDirectory(Path.Combine(GlobalState.CurrentLevelPath, "tmp"));

            try
            {
                File.Copy(Path.Combine(GlobalState.CurrentLevelPath, "level.json"), Path.Combine(GlobalState.CurrentLevelPath, "tmp", "level.json"));

                File.Copy(Path.Combine(GlobalState.CurrentLevelPath, GlobalState.CurrentLevel.background.path), 
                    Path.Combine(GlobalState.CurrentLevelPath, "tmp", GlobalState.CurrentLevel.background.path));

                File.Copy(Path.Combine(GlobalState.CurrentLevelPath, GlobalState.CurrentLevel.music.path), 
                    Path.Combine(GlobalState.CurrentLevelPath, "tmp", GlobalState.CurrentLevel.music.path));

                if(File.Exists(Path.Combine(GlobalState.CurrentLevelPath, GlobalState.CurrentLevel.music_preview.path)))
                {
                    File.Copy(Path.Combine(GlobalState.CurrentLevelPath, GlobalState.CurrentLevel.music_preview.path), 
                        Path.Combine(GlobalState.CurrentLevelPath, "tmp", GlobalState.CurrentLevel.music_preview.path));
                }

                foreach(LevelData.ChartData chart in GlobalState.CurrentLevel.charts)
                {
                    File.Copy(Path.Combine(GlobalState.CurrentLevelPath, chart.path),
                        Path.Combine(GlobalState.CurrentLevelPath, "tmp", chart.path));

                    if(File.Exists(Path.Combine(GlobalState.CurrentLevelPath, chart.music_override.path)))
                    {
                        File.Copy(Path.Combine(GlobalState.CurrentLevelPath, chart.music_override.path), 
                            Path.Combine(GlobalState.CurrentLevelPath, "tmp", chart.music_override.path));
                    }

                    // TODO: storyboard
                }
            }
            catch (Exception e)
            {
                GameObject.Find("ToastText").GetComponent<ToastMessageManager>().CreateToast("Could not prepare the files for zipping.");
                File.WriteAllText(Path.Combine(Application.persistentDataPath, "error.log"), e.StackTrace);
            }
            

            try
            {
                System.IO.Compression.ZipFile.CreateFromDirectory(Path.Combine(GlobalState.CurrentLevelPath, "tmp"), intermediatepath);
            }
            catch (Exception e)
            {
                GameObject.Find("ToastText").GetComponent<ToastMessageManager>().CreateToast("Could not zip the .cytoidlevel.");
                File.WriteAllText(Path.Combine(Application.persistentDataPath, "error.log"), e.StackTrace);
                CleanupTempFiles();
                return;
            }
        }

        if (File.Exists(finalpath))
        {
            File.Delete(finalpath);
        }

        File.Move(intermediatepath, finalpath);

        CleanupTempFiles();

        GameObject.Find("ToastText").GetComponent<ToastMessageManager>().CreateToast("Created the file " + GlobalState.CurrentLevel.id + ".cytoidlevel");
    }
}
