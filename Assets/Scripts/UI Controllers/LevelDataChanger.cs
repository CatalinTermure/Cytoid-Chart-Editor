using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System;
using CCE;
using CCE.Core;
using CCE.Data;

public class LevelDataChanger : MonoBehaviour
{
    private void Start()
    {
        GameObject.Find("IDInputField").GetComponent<InputField>().text = GlobalState.CurrentLevel.ID;

        GameObject.Find("TitleInputField").GetComponent<InputField>().text = GlobalState.CurrentLevel.Title;
        GameObject.Find("TitleLocInputField").GetComponent<InputField>().text = GlobalState.CurrentLevel.TitleLocalized;

        GameObject.Find("ArtistInputField").GetComponent<InputField>().text = GlobalState.CurrentLevel.Artist;
        GameObject.Find("ArtistLocInputField").GetComponent<InputField>().text = GlobalState.CurrentLevel.ArtistLocalized;
        GameObject.Find("ArtistSrcInputField").GetComponent<InputField>().text = GlobalState.CurrentLevel.ArtistSource;

        GameObject.Find("IllustratorInputField").GetComponent<InputField>().text = GlobalState.CurrentLevel.Illustrator;
        GameObject.Find("IllustratorSrcInputField").GetComponent<InputField>().text = GlobalState.CurrentLevel.IllustratorSource;

        GameObject.Find("CharterInputField").GetComponent<InputField>().text = GlobalState.CurrentLevel.Charter;
        GameObject.Find("StoryboarderInputField").GetComponent<InputField>().text = GlobalState.CurrentLevel.Storyboarder;

        GameObject.Find("BackgroundInputField").GetComponent<InputField>().text = GlobalState.CurrentLevel.Background.Path;
        GameObject.Find("MusicPreviewInputField").GetComponent<InputField>().text = GlobalState.CurrentLevel.MusicPreview.Path;
    }

    public void SaveData()
    {
        GlobalState.CurrentLevel.ID = GameObject.Find("IDInputField").GetComponent<InputField>().text;

        GlobalState.CurrentLevel.Title = GameObject.Find("TitleInputField").GetComponent<InputField>().text;
        GlobalState.CurrentLevel.TitleLocalized = GameObject.Find("TitleLocInputField").GetComponent<InputField>().text;

        GlobalState.CurrentLevel.Artist = GameObject.Find("ArtistInputField").GetComponent<InputField>().text;
        GlobalState.CurrentLevel.ArtistLocalized = GameObject.Find("ArtistLocInputField").GetComponent<InputField>().text;
        GlobalState.CurrentLevel.ArtistSource = GameObject.Find("ArtistSrcInputField").GetComponent<InputField>().text;

        GlobalState.CurrentLevel.Illustrator = GameObject.Find("IllustratorInputField").GetComponent<InputField>().text;
        GlobalState.CurrentLevel.IllustratorSource = GameObject.Find("IllustratorSrcInputField").GetComponent<InputField>().text;

        GlobalState.CurrentLevel.Charter = GameObject.Find("CharterInputField").GetComponent<InputField>().text;
        GlobalState.CurrentLevel.Storyboarder = GameObject.Find("StoryboarderInputField").GetComponent<InputField>().text;

        GlobalState.CurrentLevel.Background.Path = GameObject.Find("BackgroundInputField").GetComponent<InputField>().text;
        GlobalState.CurrentLevel.MusicPreview.Path = GameObject.Find("MusicPreviewInputField").GetComponent<InputField>().text;

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
        File.Delete(Path.Combine(Application.persistentDataPath, GlobalState.CurrentLevel.ID + ".cytoidlevel"));
    }

    public void PackageLevel()
    {
        string intermediatepath = Path.Combine(Application.persistentDataPath, GlobalState.CurrentLevel.ID + ".cytoidlevel"), finalpath = Path.Combine(GlobalState.CurrentLevelPath, GlobalState.CurrentLevel.ID + ".cytoidlevel");
        if (File.Exists(intermediatepath))
        {
            File.Delete(intermediatepath);
        }
        if (!File.Exists(Path.Combine(GlobalState.CurrentLevelPath, GlobalState.CurrentLevel.Background.Path)))
        {
            GameObject.Find("ToastText").GetComponent<ToastMessageManager>().CreateToast("Please make sure you have a background, otherwise the chart cannot be played in Cytoid.");
        }
        if (File.Exists(Path.Combine(GlobalState.CurrentLevelPath, GlobalState.CurrentChart.Data.Storyboard?.Path ?? "storyboard.json")))
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

                File.Copy(Path.Combine(GlobalState.CurrentLevelPath, GlobalState.CurrentLevel.Background.Path),
                    Path.Combine(GlobalState.CurrentLevelPath, "tmp", GlobalState.CurrentLevel.Background.Path));

                File.Copy(Path.Combine(GlobalState.CurrentLevelPath, GlobalState.CurrentLevel.Music.Path),
                    Path.Combine(GlobalState.CurrentLevelPath, "tmp", GlobalState.CurrentLevel.Music.Path));

                if (File.Exists(Path.Combine(GlobalState.CurrentLevelPath, GlobalState.CurrentLevel.MusicPreview.Path)))
                {
                    File.Copy(Path.Combine(GlobalState.CurrentLevelPath, GlobalState.CurrentLevel.MusicPreview.Path),
                        Path.Combine(GlobalState.CurrentLevelPath, "tmp", GlobalState.CurrentLevel.MusicPreview.Path));
                }

                foreach (LevelData.ChartFileData chart in GlobalState.CurrentLevel.Charts)
                {
                    File.Copy(Path.Combine(GlobalState.CurrentLevelPath, chart.Path),
                        Path.Combine(GlobalState.CurrentLevelPath, "tmp", chart.Path));

                    if (File.Exists(Path.Combine(GlobalState.CurrentLevelPath, chart.MusicOverride.Path)))
                    {
                        File.Copy(Path.Combine(GlobalState.CurrentLevelPath, chart.MusicOverride.Path),
                            Path.Combine(GlobalState.CurrentLevelPath, "tmp", chart.MusicOverride.Path));
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

        GameObject.Find("ToastText").GetComponent<ToastMessageManager>().CreateToast("Created the file " + GlobalState.CurrentLevel.ID + ".cytoidlevel");
    }
}
