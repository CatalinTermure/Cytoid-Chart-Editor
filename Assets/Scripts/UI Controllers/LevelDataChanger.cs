using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

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
    }

    public static void SaveLevel()
    {
        File.WriteAllText(Path.Combine(GlobalState.CurrentLevelPath, "level.json"), JsonConvert.SerializeObject(GlobalState.CurrentLevel, new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        }));
    }

    public void PackageLevel()
    {
        try
        {
            System.IO.Compression.ZipFile.CreateFromDirectory(GlobalState.CurrentLevelPath, Path.Combine(Application.persistentDataPath, GlobalState.CurrentLevel.id + ".cytoidlevel"));
            File.Move(Path.Combine(Application.persistentDataPath, GlobalState.CurrentLevel.id + ".cytoidlevel"), Path.Combine(GlobalState.CurrentLevelPath, GlobalState.CurrentLevel.id + ".cytoidlevel"));
            GameObject.Find("ToastText").GetComponent<ToastMessageManager>().CreateToast("Created the file " + GlobalState.CurrentLevel.id + ".cytoidlevel");
        } catch(System.Exception)
        {
            GameObject.Find("ToastText").GetComponent<ToastMessageManager>().CreateToast("Could not create .cytoidlevel");
        }
    }
}
