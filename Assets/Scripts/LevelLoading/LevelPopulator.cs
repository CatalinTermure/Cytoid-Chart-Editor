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
    public class LevelPopulator
    {
        private const int CacheImageSize = 256;
        private readonly List<LevelImporter> _fileImporters = new();

        private readonly LevelList _levelList;

        public LevelPopulator(LevelList levelList)
        {
            _levelList = levelList;
        }

        private void ImportLevel(string levelFilePath)
        {
            _fileImporters.Add(new LevelImporter(levelFilePath));

            var t = new Thread(_fileImporters[^1].ImportFile);

            t.Start();
        }

        private void AddLevelToPool(string path)
        {
            var level = JsonConvert.DeserializeObject<Level>(File.ReadAllText(path));

            _levelList.AddLevel(level);
        }

        public IEnumerator PopulateLevelsCoroutine()
        {
            foreach (var filePath in
                     Directory.EnumerateFiles(GlobalState.Config.LevelStoragePath))
            {
                if (FileUtils.IsLevelFile(filePath))
                {
                    ImportLevel(filePath);
                }
            }

            // Importing levels from old directory
            if (Directory.Exists(GlobalState.Config.DirPath) &&
                !Directory.Exists(Path.Combine(GlobalState.Config.DirPath, "charts")))
            {
                foreach (var dirPath in Directory.EnumerateDirectories(GlobalState.Config.DirPath))
                {
                    ImportLevel(dirPath);
                }

                GlobalState.Config.DirPath = "DEPRECATED";
            }
            else
            {
                GlobalState.Config.DirPath = "DEPRECATED";
            }

            var dots = ".";

            while (_fileImporters.Count > 0)
            {
                while (_fileImporters[0].IsRunning)
                {
                    GameObject.Find("ToastText").GetComponent<Text>().text =
                        $"Unpacking {Path.GetFileName(_fileImporters[0].FilePath)}{dots}";

                    dots += ".";
                    if (dots == "....") dots = ".";

                    yield return new WaitForSeconds(0.5f);
                }

                _fileImporters.RemoveAt(0);
            }

            GameObject.Find("ToastText").GetComponent<Text>().text = "";

            foreach (var levelDir in
                     Directory.EnumerateDirectories(GlobalState.Config.LevelStoragePath))
            {
                if (!File.Exists(Path.Combine(levelDir, ".bg")))
                {
                    var levelData =
                        JsonConvert.DeserializeObject<Level>(
                            File.ReadAllText(Path.Combine(levelDir, "level.json")));

                    if (!File.Exists(Path.Combine(levelDir, levelData.Background.Path))) continue;
                    CacheBackground(Path.Combine(levelDir, levelData.Background.Path), Path.Combine(levelDir, ".bg"));
                }

                foreach (var file in Directory.EnumerateFiles(levelDir))
                {
                    if (Path.GetFileName(file) == "level.json")
                    {
                        AddLevelToPool(file);
                    }
                }
            }

            _levelList.Query("");
        }

        public static void CacheBackground(string originalBackgroundPath, string cacheFilePath)
        {
            if (!SystemInfo.SupportsTextureFormat(TextureFormat.ARGB32))
            {
                Debug.LogError("Texture format not supported");
                return;
            }

            var tex = new Texture2D(1, 1);
            tex.LoadImage(File.ReadAllBytes(originalBackgroundPath));

            var finalSize = Math.Min(tex.width, tex.height);

            var finalTex = new Texture2D(finalSize, finalSize, TextureFormat.ARGB32, false);
            finalTex.SetPixels(0, 0,
                finalSize, finalSize,
                tex.GetPixels(
                    (tex.width - finalSize) / 2,
                    (tex.height - finalSize) / 2,
                    finalSize,
                    finalSize));

            if (tex.width < CacheImageSize || tex.height < CacheImageSize)
            {
                TextureScale.Point(finalTex, CacheImageSize, CacheImageSize);
            }
            else
            {
                TextureScale.Bilinear(finalTex, CacheImageSize, CacheImageSize);
            }

            File.WriteAllBytes(cacheFilePath, finalTex.GetRawTextureData());
        }
    }
}