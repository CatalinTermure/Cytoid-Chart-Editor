using System.IO;
using CCE.Core;
using CCE.Data;
using CCE.LevelLoading;
using CCE.Utils;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

namespace CCE.UI
{
    [RequireComponent(typeof(ClassInfoDisplay))]
    public class LevelDataDisplay : MonoBehaviour
    {
        [SerializeField] private ToastMessageManager ErrorToaster;
        [SerializeField] private TMP_InputField LevelIDInputField;
        [SerializeField] private GameObject PreviewSelectorTemplate;

        public bool DidPreviewChange;
        private string _oldId;
        private string _oldBackgroundPath;

        private void Awake()
        {
            _oldBackgroundPath = GlobalState.CurrentLevel.Background.Path;
            gameObject.GetComponent<ClassInfoDisplay>().DrawGui(GlobalState.CurrentLevel, 75, "Existing Level");
            FindObjectOfType<ImagePicker>()
                .LoadImage(Path.Combine(GlobalState.Config.LevelStoragePath,
                    GlobalState.CurrentLevel.ID,
                    GlobalState.CurrentLevel.Background.Path));

            LevelIDInputField.SetTextWithoutNotify(GlobalState.CurrentLevel.ID);
            LevelIDInputField.onEndEdit.AddListener(ChangeID);

            var rectTransform = gameObject.GetComponent<RectTransform>();
            Vector2 sizeDelta = rectTransform.sizeDelta;
            sizeDelta = new Vector2(sizeDelta.x, sizeDelta.y + 525);
            rectTransform.sizeDelta = sizeDelta;
            GameObject previewSelector =
                Instantiate(PreviewSelectorTemplate, Vector3.zero, Quaternion.identity, rectTransform);
            previewSelector.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(35, -sizeDelta.y + 525, 0);
            previewSelector.GetComponent<PreviewSelector>().LevelDataDisplay = this;
        }

        private void ChangeID(string newId)
        {
            if (newId == GlobalState.CurrentLevel.ID) return;

            LevelIDInputField.SetTextWithoutNotify(GlobalState.CurrentLevel.ID);

            if (!LevelMetadataPopupController.IsLevelIDValid(newId, ErrorToaster)) return;

            _oldId = GlobalState.CurrentLevel.ID;
            GlobalState.CurrentLevel.ID = newId;
            LevelIDInputField.SetTextWithoutNotify(GlobalState.CurrentLevel.ID);
        }

        public void SaveLevel()
        {
            if (DidPreviewChange)
            {
                File.Move(Path.Combine(GlobalState.CurrentLevelPath, "tmp-preview.ogg"),
                    Path.Combine(GlobalState.CurrentLevelPath, "preview.ogg"));

                GlobalState.CurrentLevel.MusicPreview = new LevelData.MusicData { Path = "preview.ogg" };
            }

            if (_oldId != null)
            {
                Directory.Move(Path.Combine(GlobalState.Config.LevelStoragePath, _oldId),
                    Path.Combine(GlobalState.Config.LevelStoragePath, GlobalState.CurrentLevel.ID));

                _oldId = GlobalState.CurrentLevel.ID;
            }

            string levelDirPath = Path.Combine(GlobalState.Config.LevelStoragePath, GlobalState.CurrentLevel.ID);

            SaveBackground(levelDirPath);

            File.WriteAllText(Path.Combine(levelDirPath, "level.json"),
                JsonConvert.SerializeObject(GlobalState.CurrentLevel, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                }));

            ChartCardController.DeleteDeadAssets(GlobalState.CurrentLevel);

            ErrorToaster.CreateToast("Level metadata saved!");
        }

        private void SaveBackground(string levelDirPath)
        {
            string backgroundPath =
                Path.GetFullPath(Path.Combine(levelDirPath, GlobalState.CurrentLevel.Background.Path));
            string oldBackgroundFullPath = Path.GetFullPath(Path.Combine(levelDirPath, _oldBackgroundPath));

            if (oldBackgroundFullPath == Path.GetFullPath(GlobalState.CurrentLevel.Background.Path))
            {
                GlobalState.CurrentLevel.Background.Path = _oldBackgroundPath;
            }
            else if (backgroundPath != oldBackgroundFullPath)
            {
                File.Delete(oldBackgroundFullPath);
                string extension = Path.GetExtension(GlobalState.CurrentLevel.Background.Path);
                backgroundPath = FileUtils.GetUniqueFilePath(Path.Combine(levelDirPath, "background" + extension));
                File.Copy(GlobalState.CurrentLevel.Background.Path!, backgroundPath);
                GlobalState.CurrentLevel.Background.Path = Path.GetFileName(backgroundPath);

                GlobalState.LoadBackground();
                LevelPopulator.CacheBackground(Path.Combine(levelDirPath, GlobalState.CurrentLevel.Background.Path),
                    Path.Combine(levelDirPath, ".bg"));
            }
        }
    }
}