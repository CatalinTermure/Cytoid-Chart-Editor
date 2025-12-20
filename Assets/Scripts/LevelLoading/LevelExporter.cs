using System;
using System.IO;
using System.IO.Compression;
using CCE.Core;
using CCE.GameUtils;
using CCE.Utils;
using SFB;
using UnityEngine;

namespace CCE.LevelLoading
{
    public class LevelExporter : MonoBehaviour
    {
        public ToastMessageManager ExportedToast;

        private void Start()
        {
            // TODO: Implement this sometime in the future
            GameObject.Find("Export Button").SetActive(false);
        }

        private static void ExportToTempAndThen(ExportDelegate callback)
        {
            LevelUtils.DeleteDeadAssets(GlobalState.Config.LevelStoragePath, GlobalState.CurrentLevel);
            var srcDirPath = GlobalState.CurrentLevelPath;
            var tempDirPath = Path.Combine(GlobalState.Config.TempStoragePath,
                GlobalState.CurrentLevel.ID);
            var tempArchivePath = Path.Combine(
                GlobalState.Config.TempStoragePath,
                GlobalState.CurrentLevel.ID + ".cytoidlevel");

            if (File.Exists(tempArchivePath))
            {
                File.Delete(tempArchivePath);
            }

            FileUtils.CopyDirectory(srcDirPath, tempDirPath);
            File.Delete(Path.Combine(tempDirPath, ".bg"));

            ZipFile.CreateFromDirectory(tempDirPath, tempArchivePath);

            callback(tempArchivePath);
        }

        public void ExportLevel()
        {
            ExportToTempAndThen(tempArchivePath =>
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    var success = ExportToDownloadsAndroid(tempArchivePath);
                    if (success) ExportedToast.CreateToast("Exported to Downloads");
                }
                else
                {
                    ExportArchiveDesktop(tempArchivePath);
                }
            });
        }

        public void ExportToCytoid()
        {
            if (Application.platform != RuntimePlatform.Android) return;

            ExportToTempAndThen(ExportArchiveAndroid);
        }

        private static void ExportArchiveDesktop(string tempArchivePath)
        {
            var destinationPath = StandaloneFileBrowser.SaveFilePanel("Export .cytoidlevel", "",
                GlobalState.CurrentLevel.ID, "cytoidlevel");
            if (String.IsNullOrEmpty(destinationPath))
            {
                Debug.LogWarning("Export cancelled.");
                return;
            }
            File.Move(tempArchivePath, destinationPath);
        }

        private static void ExportArchiveAndroid(string tempArchivePath)
        {
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            using var plugin = new AndroidJavaClass(Constants.AndroidPluginPackageName);
            var message = plugin.CallStatic<string>("ExportToCytoid", currentActivity, tempArchivePath);
            if (message == "") return;
            Debug.LogError(message);
        }

        private static bool ExportToDownloadsAndroid(string tempArchivePath)
        {
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            using var plugin = new AndroidJavaClass(Constants.AndroidPluginPackageName);
            var message = plugin.CallStatic<string>("ExportCytoidLevel", currentActivity, tempArchivePath);
            if (message == "") return true;
            Debug.LogError(message);
            return false;
        }

        private delegate void ExportDelegate(string tempArchivePath);
    }
}