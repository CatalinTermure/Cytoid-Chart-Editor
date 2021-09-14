using System;
using System.IO;
using System.IO.Compression;
using CCE.Core;
using CCE.Data;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace CCE.LevelLoading
{
    public class LevelImporter
    {
        public bool IsRunning;
        [NotNull] public string FilePath;

        public LevelImporter(string path)
        {
            FilePath = path;
            IsRunning = true;
        }

        public void ImportFile()
        {
            if (Directory.Exists(FilePath))
            {
                ImportUnpackedCytoidLevel();
                IsRunning = false;
                return;
            }

            if (!File.Exists(FilePath)) return;

            switch (Path.GetExtension(FilePath))
            {
                case ".cytoidlevel":
                    ImportCytoidLevel();
                    break;

                case ".cytoidpack":
                    ImportCytoidPack();
                    break;

                default:
                    Debug.LogError($"CCELog: File extension of {FilePath} " +
                                   "does not correspond to any known file extension.");
                    break;
            }

            IsRunning = false;
        }

        private void ImportUnpackedCytoidLevel()
        {
            if (!File.Exists(Path.Combine(FilePath, "level.json"))) return;

            var levelData =
                JsonConvert.DeserializeObject<LevelData>(File.ReadAllText(Path.Combine(FilePath, "level.json")));

            string finalFolderPath = Path.Combine(GlobalState.Config.LevelStoragePath, levelData.ID);

            if (Directory.Exists(finalFolderPath))
            {
                Debug.LogError(
                    $"CCELog: Level {Path.GetDirectoryName(FilePath)} " +
                    "has not been loaded.\nA level with the same ID has already been " +
                    "loaded, please delete it first before trying again.");
            }
            else
            {
                Directory.Move(FilePath, finalFolderPath);
            }
        }

        private void ImportCytoidLevel()
        {
            string tempFolderPath = Path.Combine(GlobalState.Config.TempStoragePath,
                Path.GetFileNameWithoutExtension(FilePath));

            try
            {
                Directory.CreateDirectory(tempFolderPath);

                try
                {
                    ZipFile.ExtractToDirectory(FilePath, tempFolderPath);
                }
                catch (Exception)
                {
                    File.Delete(FilePath);
                    return;
                }

                if (!File.Exists(Path.Combine(tempFolderPath, "level.json")))
                {
                    Debug.LogError("Could not find level.json file in the .cytoidlevel. " +
                        "Did you zip the folder rather than the files?");
                }

                var levelData = JsonConvert.DeserializeObject<LevelData>(
                    File.ReadAllText(Path.Combine(tempFolderPath, "level.json")));

                string finalFolderPath = Path.Combine(GlobalState.Config.LevelStoragePath, levelData.ID);

                if (Directory.Exists(finalFolderPath))
                {
                    Debug.LogError(
                        $"CCELog: Level {levelData.ID} from {Path.GetFileName(FilePath)} " +
                        "has not been loaded.\nA level with the same ID has already been " +
                        "loaded, please delete it first before trying again.");
                }
                else
                {
                    Directory.Move(tempFolderPath, finalFolderPath);
                }
            }
            finally
            {
                if (FilePath.Contains(GlobalState.Config.LevelStoragePath))
                {
                    File.Delete(FilePath);
                }

                if (Directory.Exists(tempFolderPath)) Directory.Delete(tempFolderPath, true);
            }
        }

        private void ImportCytoidPack()
        {
            try
            {
                try
                {
                    ZipFile.ExtractToDirectory(FilePath, GlobalState.Config.TempStoragePath);
                }
                catch (Exception)
                {
                    File.Delete(FilePath);
                    return;
                }


                if (FilePath.Contains(GlobalState.Config.LevelStoragePath))
                {
                    File.Delete(FilePath);
                }

                foreach (string levelPath in
                    Directory.EnumerateFiles(GlobalState.Config.TempStoragePath, "*.cytoidlevel"))
                {
                    FilePath = levelPath;
                    ImportCytoidLevel();
                }

                foreach (string unpackedLevelPath in
                    Directory.EnumerateDirectories(GlobalState.Config.TempStoragePath))
                {
                    FilePath = unpackedLevelPath;
                    ImportUnpackedCytoidLevel();
                }
            }
            finally
            {
                foreach (string dirPath in
                    Directory.EnumerateDirectories(GlobalState.Config.TempStoragePath))
                {
                    Directory.Delete(dirPath, true);
                }

                foreach (string filePath in
                    Directory.EnumerateFiles(GlobalState.Config.TempStoragePath))
                {
                    File.Delete(filePath);
                }
            }
        }
    }
}