using System;
using System.IO;
using System.Text.RegularExpressions;
using CCE.Core;
using CCE.Data;
using CCE.UI;
using UnityEngine;

namespace CCE.LevelLoading
{
    public class LevelMetadataPopupController : MonoBehaviour
    {
        [SerializeField] private ToastMessageManager ErrorToaster;
        
        private LevelData _levelData;
        private string _audioAbsolutePath;

        private const string _levelIdRegex = "^[a-z0-9_]+([-_.][a-z0-9_]+)+$";
        
        private void Awake()
        {
            _levelData = new LevelData();
            gameObject.GetComponent<ClassInfoDisplay>().DrawGui(_levelData);
        }

        public void SetAudioFile(string filePath)
        {
            _audioAbsolutePath = filePath;
        }

        private bool IsLevelDataValid()
        {
            if (String.IsNullOrEmpty(_levelData.ID))
            {
                ErrorToaster.CreateToast("Having a level ID is required.");
                return false;
            }

            if (Directory.Exists(Path.Combine(GlobalState.Config.LevelStoragePath, _levelData.ID)))
            {
                ErrorToaster.CreateToast("A level with this ID has already been loaded.\n" +
                                         "Choose a new, unique ID or delete the existing level.");
                return false;
            }
            
            if (!Regex.IsMatch(_levelData.ID, _levelIdRegex))
            {
                ErrorToaster.CreateToast("Level ID must contain only lowercase letters, numbers and separators(_, -, or .).\n" +
                                         "It also must contain at least one separator(_, - or .).");
                return false;
            }

            return true;
        }
        
        public void SaveMetadata()
        {
            if (_audioAbsolutePath == null) return;
            if (!IsLevelDataValid()) return;

            _levelData.Music = new LevelData.MusicData() { Path = Path.GetFileName(_audioAbsolutePath) };

            string levelFolderPath = Path.Combine(GlobalState.Config.LevelStoragePath, _levelData.ID);

            Directory.CreateDirectory(levelFolderPath);
            File.Copy(_audioAbsolutePath!, Path.Combine(levelFolderPath, Path.GetFileName(_audioAbsolutePath)));

            LevelListBehaviour.LoadNewChart(_levelData, "easy");
        }
    }
}