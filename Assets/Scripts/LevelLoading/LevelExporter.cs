using CCE.Core;
using CCE.Utils;
using System.IO;
using System.IO.Compression;
using SFB;
using UnityEngine;

namespace CCE.LevelLoading
{
    public class LevelExporter : MonoBehaviour
    {
        public void ExportLevel()
        {
            LevelUtils.DeleteDeadAssets(GlobalState.Config.LevelStoragePath, GlobalState.CurrentLevel);
            string srcDirPath = GlobalState.CurrentLevelPath;
            string tempDirPath = Path.Combine(GlobalState.Config.TempStoragePath,
                GlobalState.CurrentLevel.ID);
            string tempArchivePath = Path.Combine(
                GlobalState.Config.TempStoragePath,
                GlobalState.CurrentLevel.ID + ".cytoidlevel");

            if (File.Exists(tempArchivePath))
            {
                File.Delete(tempArchivePath);
            }

            try
            {
                FileUtils.CopyDirectory(srcDirPath, tempDirPath);
                File.Delete(Path.Combine(tempDirPath, ".bg"));

                ZipFile.CreateFromDirectory(tempDirPath, tempArchivePath);

                if (Application.platform == RuntimePlatform.Android)
                {
                    ExportArchiveAndroid(tempArchivePath);
                }
                else
                {
                    ExportArchiveDesktop(tempArchivePath);
                }
            }
            finally
            {
                Directory.Delete(tempDirPath, true);
                File.Delete(tempArchivePath);
            }
        }

        private static void ExportArchiveDesktop(string tempArchivePath)
        {
            string destinationPath = StandaloneFileBrowser.SaveFilePanel("Export .cytoidlevel", "",
                GlobalState.CurrentLevel.ID + ".cytoidlevel", "cytoidlevel");
            File.Move(tempArchivePath, destinationPath);
        }

        private static void ExportArchiveAndroid(string tempArchivePath)
        {
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            using var plugin = new AndroidJavaClass(GlobalState.AndroidPluginPackageName);
            using var fileUtils = plugin.CallStatic<AndroidJavaObject>("getInstance");

            fileUtils.Call("ExportCytoidLevel", currentActivity, tempArchivePath);
        }
    }
}