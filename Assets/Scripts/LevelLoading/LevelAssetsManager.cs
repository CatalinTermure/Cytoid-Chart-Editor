using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CCE.Core;
using CCE.Data;
using ManagedBass;
using UnityEngine;

namespace CCE.LevelLoading
{
    /// <summary>
    ///     Utility class for loading, storing and automatic freeing of level assets
    ///     such as backgrounds and preview audio stream handles.
    /// </summary>
    public class LevelAssetsManager
    {
        /// <summary>
        ///     Number of levels of which to keep assets in memory at once.
        /// </summary>
        private const int _poolSize = 48;

        private readonly HashSet<string> _currentlyProcessingLevels =
            new HashSet<string>();

        private readonly Queue<string> _levelIdOrderList = new Queue<string>();

        private readonly Dictionary<string, LevelAssets> _loadedLevels =
            new Dictionary<string, LevelAssets>(_poolSize);

        public readonly Queue<string> ProcessedLevels = new Queue<string>();

        public async void ScheduleLevelLoad(LevelCardInfo levelCardInfo, LevelData level)
        {
            if (_loadedLevels.ContainsKey(level.ID))
            {
                AddAssetsToCard(levelCardInfo, _loadedLevels[level.ID]);
                return;
            }

            if (_currentlyProcessingLevels.Contains(level.ID))
            {
                return;
            }

            _currentlyProcessingLevels.Add(level.ID);
            
            if (_currentlyProcessingLevels.Count + _loadedLevels.Count >= _poolSize)
            {
                FreeOldestLevel();
            }

            string backgroundFilePath =
                Path.Combine(GlobalState.Config.LevelStoragePath, level.ID, level.Background.Path);

            string audioFilePath =
                Path.Combine(GlobalState.Config.LevelStoragePath, level.ID, level.MusicPreview.Path);

            var assets = new LevelAssets {PreviewStreamHandle = await LoadPreviewStream(audioFilePath)};

            if (GlobalState.Config.LoadBackgroundsInLevelSelect && File.Exists(backgroundFilePath))
                assets.BackgroundSprite = await LoadBackground(backgroundFilePath);
            else
                assets.BackgroundSprite = GameObject.Find("Screen Background")
                    .GetComponent<BackgroundManager>().DefaultBackground;

            AddAssetsToCard(levelCardInfo, assets);

            _loadedLevels[level.ID] = assets;
            _levelIdOrderList.Enqueue(level.ID);

            _currentlyProcessingLevels.Remove(level.ID);
        }

        private static void AddAssetsToCard(LevelCardInfo levelCardInfo, LevelAssets levelAssets)
        {
            levelCardInfo.PreviewAudioHandle = levelAssets.PreviewStreamHandle;
            
            if(levelAssets.BackgroundSprite != null)
                levelCardInfo.BackgroundPreview.sprite = levelAssets.BackgroundSprite;
        }
        
        private void FreeOldestLevel()
        {
            string id = _levelIdOrderList.Dequeue();
            Bass.StreamFree(_loadedLevels[id].PreviewStreamHandle);
            Object.Destroy(_loadedLevels[id].BackgroundSprite);
            _loadedLevels.Remove(id);
        }
        
        private static async Task<int> LoadPreviewStream(string path)
        {
            if (!File.Exists(path))
                return 0;
            
            byte[] data = await LoadFileAsync(path);
            if (data.Length == 0)
            {
                Debug.LogError($"CCE.LevelLoading: Could not load audio at {path}.");
                return 0;
            }
            
            return Bass.CreateStream(data, 0, data.Length, BassFlags.Loop);
        }
        
        private static async Task<Sprite> LoadBackground(string path)
        {
            // TODO: cache backgrounds and load them by raw data to avoid lag spikes
            var tex = new Texture2D(1, 1);
            tex.LoadImage(await LoadFileAsync(path));
            var result = Sprite.Create(tex,
                new Rect(0, 0, tex.width, tex.height),
                Vector2.zero, 
                100, 0,
                SpriteMeshType.FullRect);
            
            return result;
        }

        private static async Task<byte[]> LoadFileAsync(string path)
        {
            using FileStream stream = File.Open(path, FileMode.Open);
            byte[] result = new byte[stream.Length];
            await stream.ReadAsync(result, 0, (int) stream.Length);
            stream.Close();

            return result;
        }

        private class LevelAssets
        {
            public Sprite BackgroundSprite;
            public int PreviewStreamHandle;
        }
    }
}