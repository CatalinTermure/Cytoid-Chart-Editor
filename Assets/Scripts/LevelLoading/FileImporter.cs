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
    public class FileImporter
    {
        public bool IsRunning;
        [NotNull] public string FilePath;

        public FileImporter(string path)
        {
            FilePath = path;
            IsRunning = true;
        }

        public void ImportFile()
        {
            if (!File.Exists(FilePath)) return;
            
            switch (Path.GetExtension(FilePath))
            {
                case ".cytoidlevel":
                    ImportCytoidLevel();
                    break;
                
                case ".cytoidpack":
                    ImportCytoidPack();
                    break;
                
                case ".png":
                case ".jpg":
                case ".jpeg":
                    ImportResource(GlobalState.Config.BackgroundStoragePath);
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
            try
            {
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
            finally
            {
                if(Directory.Exists(FilePath)) Directory.Delete(FilePath, true);
            }
        }

        private void ImportCytoidLevel()
        {
            string tempFolderPath = Path.Combine(GlobalState.Config.TempStoragePath,
                Path.GetFileNameWithoutExtension(FilePath));
            
            try
            {
                Directory.CreateDirectory(tempFolderPath);
                
                ZipFile.ExtractToDirectory(FilePath, tempFolderPath);

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
                
                if (FilePath.Contains(GlobalState.Config.LevelStoragePath))
                {
                    File.Delete(FilePath);
                }
            }
            finally
            {
                if(Directory.Exists(tempFolderPath)) Directory.Delete(tempFolderPath, true);
            }
        }

        private void ImportCytoidPack()
        {
            try
            {
                ZipFile.ExtractToDirectory(FilePath, GlobalState.Config.TempStoragePath);
                
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

        private void ImportResource(string resourceFolderPath)
        {
            string finalPath = Path.Combine(resourceFolderPath, Path.GetFileName(FilePath));

            finalPath = FileUtils.GetUniqueFilePath(finalPath);

            File.Copy(FilePath, finalPath); 
        }
    }
}