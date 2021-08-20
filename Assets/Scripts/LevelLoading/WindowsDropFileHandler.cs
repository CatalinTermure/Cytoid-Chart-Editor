using System.Collections.Generic;
using B83.Win32;
using CCE.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CCE.LevelLoading
{
    public class WindowsDropFileHandler : MonoBehaviour
    {
        [SerializeField] private LevelListBehaviour LevelListBehaviour;
        
        private void OnEnable()
        {
            if (Application.platform != RuntimePlatform.WindowsPlayer) return; 
            UnityDragAndDropHook.InstallHook();
            UnityDragAndDropHook.OnDroppedFiles += OnFiles;
        }

        private void OnDisable()
        {
            if (Application.platform != RuntimePlatform.WindowsPlayer) return;
            UnityDragAndDropHook.UninstallHook();
        }

        private void OnFiles(List<string> filePaths, POINT positions)
        {
            foreach (string filePath in filePaths)
            {
                if (FileUtils.IsAudioFile(filePath) 
                    && SceneManager.GetActiveScene().name == SceneUtils.LevelSelectSceneName)
                {
                    LevelListBehaviour.ShowLevelMetadataPopup(filePath);
                    return;
                }
                if (FileUtils.IsImageFile(filePath) 
                         && SceneManager.GetActiveScene().name == SceneUtils.MainSceneName)
                {
                    Debug.LogError("TODO: Add functionality for changing the background during chart edit.");
                    return;
                }
            }
        }
    }
}