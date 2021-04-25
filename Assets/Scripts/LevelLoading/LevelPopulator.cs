using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CCE.Core;
using CCE.Data;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace CCE.LevelLoading
{
    /// <summary>
    ///     Class for populating a <see cref="LevelListView" />.
    /// </summary>
    public class LevelPopulator
    {
        /// <summary>
        ///     List of currently active <see cref="FileImporter" /> tasks.
        /// </summary>
        private readonly List<FileImporter> _fileImporters = new List<FileImporter>();

        private readonly LevelListView _levelListView;

        public LevelPopulator(LevelListView levelListView)
        {
            _levelListView = levelListView;
#if UNITY_ANDROID
            if(Application.platform == RuntimePlatform.Android)
                AndroidHandleImportIntent();
#endif
        }

        private void ImportLevel(string levelFilePath)
        {
            _fileImporters.Add(new FileImporter(levelFilePath));

            var t = new Thread(_fileImporters[_fileImporters.Count - 1].ImportFile);

            t.Start();
        }

        private void AddLevelToPool(string path)
        {
            var level = JsonConvert.DeserializeObject<LevelData>(File.ReadAllText(path));

            _levelListView.AddLevel(level);
        }

#if UNITY_ANDROID
        private static void AndroidHandleImportIntent()
        {
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");

            var currentActivity =
                unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            var intent = currentActivity.Call<AndroidJavaObject>("getIntent");

            string filePath = intent.Call<string>("getDataString");

            if (String.IsNullOrEmpty(filePath)) return;
            filePath = new Uri(filePath).LocalPath;
            
            if (File.Exists(filePath))
            {
                File.Copy(filePath,
                    Path.Combine(
                        GlobalState.Config.LevelStoragePath,
                        Path.GetFileName(filePath)
                    )
                );
            }
        }
#endif

        /// <summary>
        ///     Coroutine that adds and initializes the levels from the
        ///     file system into the list level pool, while displaying a loading message.
        /// </summary>
        /// <param name="levelItemTemplate"> Template level card for initializing the pool. </param>
        public IEnumerator PopulateLevelsCoroutine(GameObject levelItemTemplate)
        {
            foreach (string filePath in
                Directory.EnumerateFiles(GlobalState.Config.LevelStoragePath))
            {
                string extension = Path.GetExtension(filePath);
                if (extension == ".cytoidpack" || extension == ".cytoidlevel")
                {
                    ImportLevel(filePath);
                }
            }

            string dots = ".";

            while (_fileImporters.Count > 0)
            {
                while (_fileImporters[0].IsRunning)
                {
                    GameObject.Find("ToastText").GetComponent<Text>().text =
                        $"Unpacking {Path.GetFileName(_fileImporters[0].FilePath)}{dots}";

                    dots += ".";
                    if (dots == "....")
                    {
                        dots = ".";
                    }

                    yield return new WaitForSeconds(0.5f);
                }

                _fileImporters.RemoveAt(0);
            }

            GameObject.Find("ToastText").GetComponent<Text>().text = "";

            foreach (string levelDir in
                Directory.EnumerateDirectories(GlobalState.Config.LevelStoragePath))
            {
                foreach (string file in Directory.EnumerateFiles(levelDir))
                {
                    if (Path.GetFileName(file) == "level.json")
                    {
                        AddLevelToPool(file);
                    }
                }
            }

            _levelListView.Initialize(levelItemTemplate, GameObject.Find("Level List"));
        }
    }
}