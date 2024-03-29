﻿using System;
using System.IO;
using CCE.Core;
using UnityEngine;
using CCE.Utils;

namespace CCE.LevelLoading
{
    public class AndroidIntentHandler : MonoBehaviour
    {
        [SerializeField] private LevelListBehaviour LevelListBehaviour;
        private static bool _isIntentHandled;

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
            var intent = currentActivity.Call<AndroidJavaObject>("getIntent");
            string filePath = intent.Call<string>("getDataString");

            if (String.IsNullOrEmpty(filePath)) return;
            
            filePath = new Uri(filePath).LocalPath;

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