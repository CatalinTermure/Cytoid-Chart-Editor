using CCE.Core;
using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace CCE.LevelLoading
{
    public class LevelExporter : MonoBehaviour
    {
        public void ExportLevel()
        {
            ChartCardController.DeleteDeadAssets(GlobalState.CurrentLevel);
            string srcDirPath = GlobalState.CurrentLevelPath;
            string tempArchivePath = Path.Combine(
                GlobalState.Config.TempStoragePath,
                GlobalState.CurrentLevel.ID + ".cytoidlevel");

            try
            {
                ZipFile.CreateFromDirectory(srcDirPath, tempArchivePath);

                if (Application.platform == RuntimePlatform.Android)
                {
                    ExportArchiveAndroid(tempArchivePath);
                }
                else
                {
                    ExportArchiveDesktop(tempArchivePath);
                }
            } finally
            {
                File.Delete(tempArchivePath);
            }
        }

        private void ExportArchiveDesktop(string tempArchivePath)
        {
            //
        }

        private void ExportArchiveAndroid(string tempArchivePath)
        {
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            using var plugin = new AndroidJavaClass(GlobalState.AndroidPluginPackageName);
            using var fileUtils = plugin.CallStatic<AndroidJavaObject>("getInstance");

            fileUtils.Call("ExportCytoidLevel", currentActivity, tempArchivePath);
        }
    }
}
