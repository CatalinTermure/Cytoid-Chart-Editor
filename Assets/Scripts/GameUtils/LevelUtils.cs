using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CCE.Data;
using CCE.Utils;

namespace CCE.GameUtils
{
    public static class LevelUtils
    {
        public static void DeleteDeadAssets(string levelStoragePath, LevelData levelData)
        {
            foreach (var chart in levelData.Charts)
            {
                if (!String.IsNullOrEmpty(chart.Storyboard?.Path))
                {
                    // this is a workaround so that we don't delete files linked to a storyboard
                    // will need to change this when storyboard parsing is implemented
                    return;
                }
            }

            var levelDir = Path.Combine(levelStoragePath, levelData.ID);

            // The huge amount of Path.GetFullPath() comes from the need to use a consistent path scheme
            // so that the string comparisons don't return false negatives

            var validFiles = new List<string>
            {
                Path.GetFullPath(Path.Combine(levelDir, "level.json")),
                Path.GetFullPath(Path.Combine(levelDir, ".bg"))
            };
            if (levelData.Background?.Path != null)
            {
                validFiles.Add(Path.GetFullPath(Path.Combine(levelDir, levelData.Background.Path)));
            }

            if (levelData.Music?.Path != null)
            {
                validFiles.Add(Path.GetFullPath(Path.Combine(levelDir, levelData.Music.Path)));
            }

            if (levelData.MusicPreview?.Path != null)
            {
                validFiles.Add(Path.GetFullPath(Path.Combine(levelDir, levelData.MusicPreview.Path)));
            }

            foreach (var chart in levelData.Charts)
            {
                validFiles.Add(Path.GetFullPath(Path.Combine(levelDir, chart.Path)));
                if (chart.MusicOverride?.Path != null)
                {
                    validFiles.Add(Path.GetFullPath(Path.Combine(levelDir, chart.MusicOverride.Path)));
                }
            }

            var filesToRemove = FileUtils.GetFilesInDirectory(levelDir).Except(validFiles).ToList();

            foreach (var file in filesToRemove)
            {
                File.Delete(file);
            }
        }
    }
}