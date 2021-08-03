using System;
using System.IO;
using CCE.Core;
using UnityEngine;

namespace CCE.LevelLoading
{
    public class AndroidIntentHandler : MonoBehaviour
    {
        [SerializeField] private LevelListBehaviour LevelListBehaviour;
        private bool _isIntentHandled;

        private void Awake()
        {
            if (_isIntentHandled) return;

            HandleImportIntent();
        }
        
        private void HandleImportIntent()
        {
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            var intent = currentActivity.Call<AndroidJavaObject>("getIntent");
            string filePath = intent.Call<string>("getDataString");

            if (String.IsNullOrEmpty(filePath)) return;
            
            filePath = new Uri(filePath).LocalPath;
            
            if (!File.Exists(filePath)) return;

            if (IsAudioFile(filePath))
            {
                LevelListBehaviour.ShowLevelMetadataPopup(filePath);
            }
            else if (IsLevelFile(filePath))
            {
                File.Copy(filePath,
                    Path.Combine(
                        GlobalState.Config.LevelStoragePath,
                        Path.GetFileName(filePath)
                    )
                );
            }
        }

        private static bool IsAudioFile(string file)
        {
            return Path.GetExtension(file) switch
            {
                ".wav" => true,
                ".ogg" => true,
                ".mp3" => true,
                _ => false
            };
        }

        private static bool IsLevelFile(string file)
        {
            return Path.GetExtension(file) switch
            {
                ".cytoidlevel" => true,
                ".cytoidpack" => true,
                _ => false
            };
        }
    }
}