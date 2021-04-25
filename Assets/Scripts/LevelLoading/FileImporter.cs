using System.IO;
using System.IO.Compression;
using CCE.Core;
using CCE.Data;
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
                case ".cytoidlevel.zip":
                    ImportCytoidLevel();
                    break;
                
                case ".cytoidpack":
                case ".cytoidpack.zip":
                    ImportCytoidPack();
                    break;
                
                case ".png":
                case ".jpg":
                case ".jpeg":
                    ImportResource(GlobalState.Config.BackgroundStoragePath);
                    break;
                
                case ".wav":
                case ".ogg":
                case ".mp3":
                    ImportResource(GlobalState.Config.MusicStoragePath);
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
            string finalFolderPath = Path.Combine(GlobalState.Config.LevelStoragePath, Path.GetFileName(FilePath));

            try
            {
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

            if (File.Exists(finalPath))
            {
                int i = 1;
                string newFileName = 
                    Path.GetFileNameWithoutExtension(FilePath) + $" ({i})" + Path.GetExtension(FilePath);

                while (File.Exists(Path.Combine(resourceFolderPath, newFileName)))
                {
                    i++;
                    newFileName = 
                        Path.GetFileNameWithoutExtension(FilePath) + $" ({i})" + Path.GetExtension(FilePath);
                }

                finalPath = Path.Combine(resourceFolderPath, newFileName);
            }
            
            File.Copy(FilePath, finalPath); 
        }
    }
}