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

        private Level _level;

        private void Awake()
        {
            _level = new Level();
            gameObject.GetComponent<ClassInfoDisplay>().DrawGui(_level, 0);
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
            if (!IsLevelIDValid(_level.ID, ErrorToaster)) return false;

            if (_level.Background?.Path == null)
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

            _level.Music = new Level.MusicData { Path = Path.GetFileName(_audioAbsolutePath) };
            _level.MusicPreview = new Level.MusicData { Path = Path.GetFileName(_audioAbsolutePath) };

            var levelFolderPath = Path.Combine(GlobalState.Config.LevelStoragePath, _level.ID);
            var finalBackgroundPath = Path.Combine(levelFolderPath,
                "background" + Path.GetExtension(_level.Background.Path));

            Directory.CreateDirectory(levelFolderPath);
            File.Copy(_audioAbsolutePath!, Path.Combine(levelFolderPath, Path.GetFileName(_audioAbsolutePath)));
            File.Copy(_level.Background.Path!, finalBackgroundPath);
            _level.Background.Path = Path.GetFileName(finalBackgroundPath);

            ChartCardController.LoadNewChart(_level, "easy");
        }

        public void Cancel()
        {
            Destroy(gameObject);
        }
    }
}