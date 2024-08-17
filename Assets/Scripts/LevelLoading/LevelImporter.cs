using System;
using System.IO;
using System.IO.Compression;
using CCE.Core;
using CCE.Data;
using CCE.Utils;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace CCE.LevelLoading
{
    public class LevelImporter
    {
        [NotNull] public string FilePath;
        public bool IsRunning;

        public LevelImporter(string path)
        {
            FilePath = path;
            IsRunning = true;
        }

        public void ImportFile()
        {
            if (Directory.Exists(FilePath))
            {
                ImportUnpackedCytoidLevel(FilePath);
                IsRunning = false;
                return;
            }

            if (!File.Exists(FilePath)) return;

            switch (Path.GetExtension(FilePath))
            {
                case ".cytoidlevel":
                    ImportCytoidLevel(FilePath);
                    break;

                case ".cytoidpack":
                    ImportCytoidPack(FilePath);
                    break;

                default:
                    Debug.LogError($"CCELog: File extension of {FilePath} " +
                                   "does not correspond to any known file extension.");
                    break;
            }

            IsRunning = false;
        }

        private void ImportUnpackedCytoidLevel(string folderPath)
        {
            if (!File.Exists(Path.Combine(folderPath, "level.json")))
            {
                return;
            }

            var levelData =
                JsonConvert.DeserializeObject<Level>(File.ReadAllText(Path.Combine(folderPath, "level.json")));

            var finalFolderPath = Path.Combine(GlobalState.Config.LevelStoragePath, levelData.ID);

            if (Directory.Exists(finalFolderPath))
            {
                Debug.LogError(
                    $"CCELog: Level {folderPath} " +
                    "has not been loaded.\nA level with the same ID has already been " +
                    "loaded, please delete it first before trying again.");
            }
            else
            {
                FileUtils.CopyDirectory(folderPath, finalFolderPath);
                if (folderPath.Contains(GlobalState.Config.TempStoragePath))
                {
                    Directory.Delete(folderPath, true);
                }
            }

            FilePath = finalFolderPath;
        }

        private void ImportCytoidLevel(string filePath)
        {
            var tempFolderPath = Path.Combine(GlobalState.Config.TempStoragePath,
                Path.GetFileNameWithoutExtension(filePath));

            var finalFolderPath = "";

            try
            {
                Directory.CreateDirectory(tempFolderPath);

                try
                {
                    ZipFile.ExtractToDirectory(filePath, tempFolderPath);
                }
                catch (Exception)
                {
                    File.Delete(filePath);
                    return;
                }

                if (!File.Exists(Path.Combine(tempFolderPath, "level.json")))
                {
                    Debug.LogError("Could not find level.json file in the .cytoidlevel. " +
                                   "Did you zip the folder rather than the files?");
                }

                var levelData = JsonConvert.DeserializeObject<Level>(
                    File.ReadAllText(Path.Combine(tempFolderPath, "level.json")));

                finalFolderPath = Path.Combine(GlobalState.Config.LevelStoragePath, levelData.ID);

                if (Directory.Exists(finalFolderPath))
                {
                    Debug.LogError(
                        $"CCELog: Level {levelData.ID} from {Path.GetFileName(filePath)} " +
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
                if (filePath.Contains(GlobalState.Config.LevelStoragePath))
                {
                    File.Delete(filePath);
                }

                FilePath = finalFolderPath;

                if (Directory.Exists(tempFolderPath)) Directory.Delete(tempFolderPath, true);
            }
        }

        private void ImportCytoidPack(string filePath)
        {
            try
            {
                try
                {
                    ZipFile.ExtractToDirectory(filePath, GlobalState.Config.TempStoragePath);
                }
                catch (Exception)
                {
                    File.Delete(filePath);
                    return;
                }


                if (FilePath.Contains(GlobalState.Config.LevelStoragePath))
                {
                    File.Delete(filePath);
                }

                foreach (var levelPath in
                         Directory.EnumerateFiles(GlobalState.Config.TempStoragePath, "*.cytoidlevel"))
                {
                    FilePath = levelPath;
                    ImportCytoidLevel(levelPath);
                }

                foreach (var unpackedLevelPath in
                         Directory.EnumerateDirectories(GlobalState.Config.TempStoragePath))
                {
                    FilePath = unpackedLevelPath;
                    ImportUnpackedCytoidLevel(unpackedLevelPath);
                }
            }
            finally
            {
                foreach (var dir in
                         Directory.EnumerateDirectories(GlobalState.Config.TempStoragePath))
                {
                    Directory.Delete(dir, true);
                }

                foreach (var file in
                         Directory.EnumerateFiles(GlobalState.Config.TempStoragePath))
                {
                    File.Delete(file);
                }
            }
        }
    }
}