using System;
using System.IO;
using System.Text.RegularExpressions;
using CCE.Core;
using CCE.Data;
using CCE.GameUtils;
using CCE.Utils;
using UnityEngine;

namespace CCE.LevelLoading
{
    public class LevelMetadataPopupController : MonoBehaviour
    {
        private const string LevelIdRegex = "^[a-z0-9_]+([-_.][a-z0-9_]+)+$";
        [SerializeField] private ToastMessageManager ErrorToaster;
        private string _audioAbsolutePath;

        private LevelData _levelData;

        private void Awake()
        {
            _levelData = new LevelData();
            gameObject.GetComponent<ClassInfoDisplay>().DrawGui(_levelData, 0);
        }

        public void SetAudioFile(string filePath)
        {
            _audioAbsolutePath = filePath;
        }

        public static bool IsLevelIDValid(string id, ToastMessageManager errorToaster)
        {
            if (String.IsNullOrEmpty(id))
            {
                errorToaster.CreateToast("Having a level ID is required.");
                return false;
            }

            if (Directory.Exists(Path.Combine(GlobalState.Config.LevelStoragePath, id)))
            {
                errorToaster.CreateToast("A level with this ID has already been loaded.\n" +
                                         "Choose a new, unique ID or delete the existing level.", 5);
                return false;
            }

            if (!Regex.IsMatch(id, LevelIdRegex))
            {
                errorToaster.CreateToast(
                    "Level ID must contain only lowercase letters, numbers and separators(_, -, or .).\n" +
                    "It also must contain at least one separator(_, - or .).", 8);
                return false;
            }

            return true;
        }

        private bool IsLevelDataValid()
        {
            if (!IsLevelIDValid(_levelData.ID, ErrorToaster)) return false;

            if (_levelData.Background?.Path == null)
            {
                ErrorToaster.CreateToast("You must choose a background picture.");
                return false;
            }

            return true;
        }

        public void SaveMetadata()
        {
            if (_audioAbsolutePath == null) return;
            if (!IsLevelDataValid()) return;

            _levelData.Music = new LevelData.MusicData { Path = Path.GetFileName(_audioAbsolutePath) };
            _levelData.MusicPreview = new LevelData.MusicData { Path = Path.GetFileName(_audioAbsolutePath) };

            var levelFolderPath = Path.Combine(GlobalState.Config.LevelStoragePath, _levelData.ID);
            var finalBackgroundPath = Path.Combine(levelFolderPath,
                "background" + Path.GetExtension(_levelData.Background.Path));

            Directory.CreateDirectory(levelFolderPath);
            File.Copy(_audioAbsolutePath!, Path.Combine(levelFolderPath, Path.GetFileName(_audioAbsolutePath)));
            File.Copy(_levelData.Background.Path!, finalBackgroundPath);
            _levelData.Background.Path = Path.GetFileName(finalBackgroundPath);

            ChartCardController.LoadNewChart(_levelData, "easy");
        }

        public void Cancel()
        {
            Destroy(gameObject);
        }
    }
}