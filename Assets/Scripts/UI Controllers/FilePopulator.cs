using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class FilePopulator : MonoBehaviour
{
    private int listItemCount = 0;

    public GameObject FolderListItem, FileListItem;
    public GameObject DirectoryText;

    private void Start()
    {
        PopulateList(GlobalState.Config.DirPath);
    }

    private void Awake()
    {
        // Handle SetDefaultFolderButton's click event
        GameObject.Find("SetDefaultFolderButton").GetComponent<Button>().onClick.AddListener(
            () =>
            {
                GlobalState.Config.DirPath = GameObject.Find("FolderPathText").GetComponent<Text>().text;
                GlobalState.SaveConfig();
                GameObject.Find("LevelListView").GetComponentInChildren<LevelPopulator>().PopulateLevels(GlobalState.Config.DirPath);
            });
    }

    public void AddFolder(string name)
    {
        GameObject obj = Instantiate(FolderListItem);
        (obj.transform as RectTransform).SetParent(gameObject.transform);
        (obj.transform as RectTransform).localPosition = new Vector3(0, -100 * listItemCount);
        (obj.transform as RectTransform).sizeDelta = new Vector2(800, 100);
        obj.transform.localScale = Vector3.one;
        obj.GetComponentInChildren<Text>().text = name;

        listItemCount++;
        (gameObject.transform as RectTransform).sizeDelta = new Vector2(0, 100 * listItemCount);

        obj.GetComponent<Button>().onClick.AddListener(() =>
        {
            GlobalState.Config.DirPath = Path.Combine(GlobalState.Config.DirPath, name);
            PopulateList(GlobalState.Config.DirPath);
        });
    }

    public void PopulateList(string dirName)
    {
        DirectoryText.GetComponent<Text>().text = dirName;

        foreach (Transform child in gameObject.transform)
        {
            Destroy(child.gameObject);
        }
        listItemCount = 0;
        (gameObject.transform as RectTransform).sizeDelta = new Vector2(0, 0);

        // Add parent directory option

        try
        {
            if (Directory.GetDirectories(Directory.GetParent(GlobalState.Config.DirPath).FullName).Length > 0)
            {
                GameObject obj = Instantiate(FolderListItem);
                (obj.transform as RectTransform).SetParent(gameObject.transform);
                (obj.transform as RectTransform).localPosition = new Vector3(0, -100 * listItemCount);
                (obj.transform as RectTransform).sizeDelta = new Vector2(800, 100);
                obj.transform.localScale = Vector3.one;
                obj.GetComponentInChildren<Text>().text = "...";

                listItemCount++;
                (gameObject.transform as RectTransform).sizeDelta = new Vector2(0, 100 * listItemCount);

                obj.GetComponent<Button>().onClick.AddListener(() =>
                {
                    GlobalState.Config.DirPath = Directory.GetParent(GlobalState.Config.DirPath).FullName;
                    PopulateList(GlobalState.Config.DirPath);
                });
            }
        } catch(UnauthorizedAccessException)
        {
            //
        }
        

        // Add child folders and files
        foreach (string folder in Directory.EnumerateDirectories(dirName))
        {
            AddFolder(Path.GetFileName(folder));
        }
    }
}
