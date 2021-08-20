using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CCE.Core;
using CCE.Data;
using CCE.Utils;
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
        ///     List of currently active <see cref="LevelImporter" /> tasks.
        /// </summary>
        private readonly List<LevelImporter> _fileImporters = new List<LevelImporter>();

        private readonly LevelListView _levelListView;
        
        private const int _cacheImageSize = 256;

        public LevelPopulator(LevelListView levelListView)
        {
            _levelListView = levelListView;
        }

        private void ImportLevel(string levelFilePath)
        {
            _fileImporters.Add(new LevelImporter(levelFilePath));

            var t = new Thread(_fileImporters[_fileImporters.Count - 1].ImportFile);

            t.Start();
        }

        private void AddLevelToPool(string path)
        {
            var level = JsonConvert.DeserializeObject<LevelData>(File.ReadAllText(path));

            _levelListView.AddLevel(level);
        }
        
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
                if (!File.Exists(Path.Combine(levelDir, ".bg")))
                {
                    var levelData = 
                        JsonConvert.DeserializeObject<LevelData>(File.ReadAllText(Path.Combine(levelDir, "level.json")));
                    
                    CacheBackground(Path.Combine(levelDir, levelData.Background.Path), Path.Combine(levelDir, ".bg"));
                }
                
                foreach (string file in Directory.EnumerateFiles(levelDir))
                {
                    if (Path.GetFileName(file) == "level.json")
                    {
                        AddLevelToPool(file);
                    }
                }
            }

            _levelListView.Initialize();
        }
        
        private static void CacheBackground(string originalBackgroundPath, string cacheFilePath)
        {
            if (!SystemInfo.SupportsTextureFormat(TextureFormat.ARGB32))
            {
                Debug.LogError("Texture format not supported");
                return;
            }
            
            var tex = new Texture2D(1, 1);
            tex.LoadImage(File.ReadAllBytes(originalBackgroundPath));
            
            int finalSize = Math.Min(tex.width, tex.height);

            var finalTex = new Texture2D(finalSize, finalSize, TextureFormat.ARGB32, false);
            finalTex.SetPixels(0, 0,
                finalSize, finalSize,
                tex.GetPixels(
                    (tex.width - finalSize) / 2,
                    (tex.height - finalSize) / 2,
                    finalSize,
                    finalSize));

            TextureScale.Bilinear(finalTex, _cacheImageSize, _cacheImageSize);
            
            File.WriteAllBytes(cacheFilePath, finalTex.GetRawTextureData());
        }
    }
}