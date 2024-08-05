using System.Collections;
using System.IO;
using CCE.Data;
using Newtonsoft.Json;
using SFB;
using UnityEngine;

namespace CCE.LevelLoading
{
    public class AudioPicker : MonoBehaviour
    {
        [SerializeField] private LevelList LevelList;

        private bool _isRunning;
        private string _finalPath;

        private void PickAudioMobile()
        {
            NativeGallery.GetAudioFromGallery(path =>
            {
                _isRunning = false;
                _finalPath = path;
            });
        }

        private void HandleFilePickedDesktop(string file)
        {
            if (Path.GetExtension(file) is ".ogg" or ".mp3" or ".wav")
            {
                return;
            }

            LevelImporter importer;
            if (Path.GetFileName(file) == "level.json")
            {
                importer = new LevelImporter(Path.GetDirectoryName(file));
            }
            else if (Path.GetExtension(file) == ".cytoidlevel")
            {
                importer = new LevelImporter(file);
            }
            else
            {
                Debug.LogError("CCELog: File chosen is of an unsupported format.");
                return;
            }

            importer.ImportFile();
            string levelPath = Path.Combine(importer.FilePath, "level.json");
            var level = JsonConvert.DeserializeObject<LevelData>(File.ReadAllText(levelPath));
            LevelPopulator.CacheBackground(Path.Combine(importer.FilePath, level.Background.Path), Path.Combine(importer.FilePath, ".bg"));
            LevelList.AddLevel(level);
            LevelList.Behaviour.SearchInputField.SetTextWithoutNotify(level.ID);
            LevelList.Query(level.ID);
        }

        private void PickAudioDesktop()
        {
            var extensions = new[]
            {
                new ExtensionFilter("Audio Files", "ogg", "mp3", "wav"),
                new ExtensionFilter("Cytoid levels", "cytoidlevel", "json")
            };

            StandaloneFileBrowser.OpenFilePanelAsync("Choose an audio file or cytoidlevel", "", extensions, false,
                paths =>
                {
                    if (paths.Length == 0) return;
                    _isRunning = false;
                    _finalPath = paths[0];
                    HandleFilePickedDesktop(_finalPath);
                });
        }

        public void PickAudio()
        {
            _isRunning = true;
            if (Application.isMobilePlatform)
            {
                PickAudioMobile();
            }
            else
            {
                PickAudioDesktop();
            }

            StartCoroutine(WaitForAudioPickedCoroutine());
        }

        private IEnumerator WaitForAudioPickedCoroutine()
        {
            while (_isRunning)
            {
                yield return null;
            }

            if (!File.Exists(_finalPath)) yield break;
            if (Path.GetExtension(_finalPath) is not (".ogg" or ".mp3" or ".wav"))
            {
                yield break;
            }

            LevelList.Behaviour.ShowLevelMetadataPopup(_finalPath);
        }
    }
}