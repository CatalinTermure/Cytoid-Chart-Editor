using System;
using System.IO;
using CCE.Core;
using CCE.Utils;
using UnityEngine;

namespace CCE.LevelLoading
{
    public class AndroidIntentHandler : MonoBehaviour
    {
        private static bool _isIntentHandled;
        [SerializeField] private LevelListBehaviour LevelListBehaviour;

        private void Awake()
        {
            if (Application.platform != RuntimePlatform.Android) return;
            if (_isIntentHandled) return;

            HandleImportIntent();
        }

        private void HandleImportIntent()
        {
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            using var plugin = new AndroidJavaClass(GlobalState.AndroidPluginPackageName);
            var filePath = plugin.CallStatic<string>("HandleIntent", currentActivity);

            if (filePath == "No uri") return;

            try
            {
                filePath = new Uri(filePath).LocalPath;
            }
            catch (Exception)
            {
                Debug.LogError($"Invalid file path: {filePath}");
                throw;
            }

            _isIntentHandled = true;

            if (!File.Exists(filePath)) return;

            if (FileUtils.IsAudioFile(filePath))
            {
                LevelListBehaviour.ShowLevelMetadataPopup(filePath);
            }
            else if (FileUtils.IsLevelFile(filePath))
            {
                File.Copy(filePath,
                    Path.Combine(GlobalState.Config.LevelStoragePath, Path.GetFileName(filePath)));
            }
        }
    }
}