﻿using System.Collections.Generic;
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
        /// Number of levels of which to keep assets in memory at once.
        private const int _poolSize = 48;

        /// Width and height in pixels of the cached preview image for each level.
        private const int _cacheImageSize = 256;

        private readonly HashSet<string> _currentlyProcessingLevels =
            new HashSet<string>();

        private readonly List<string> _levelIdOrderList = new List<string>();

        private readonly Dictionary<string, LevelAssets> _loadedLevels =
            new Dictionary<string, LevelAssets>(_poolSize);

        private readonly Sprite _defaultBackground;
        
        public LevelAssetsManager()
        {
            _defaultBackground =
                GameObject.Find("Screen Background").GetComponent<BackgroundManager>().DefaultBackground;
        }

        public async void ScheduleLevelLoad(LevelCardInfo levelCardInfo, LevelData level)
        {
            if (_loadedLevels.ContainsKey(level.ID))
            {
                _levelIdOrderList.Remove(level.ID);
                _levelIdOrderList.Add(level.ID);
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

            string audioPreviewFilePath =
                Path.Combine(GlobalState.Config.LevelStoragePath, level.ID, level.MusicPreview.Path);

            string audioFilePath = Path.Combine(GlobalState.Config.LevelStoragePath, level.ID, level.Music.Path);
            
            var assets = new LevelAssets
            {
                PreviewStreamHandle =
                    await LoadPreviewStream(File.Exists(audioPreviewFilePath) ? audioPreviewFilePath : audioFilePath),
                OriginalBackgroundPath = backgroundFilePath
            };
            
            if (GlobalState.Config.LoadBackgroundsInLevelSelect && File.Exists(backgroundFilePath))
                assets.PreviewTexture = await LoadBackground(backgroundFilePath);
            else
                assets.PreviewTexture = null;

            AddAssetsToCard(levelCardInfo, assets);

            _loadedLevels[level.ID] = assets;
            _levelIdOrderList.Add(level.ID);

            _currentlyProcessingLevels.Remove(level.ID);
        }

        private void AddAssetsToCard(LevelCardInfo levelCardInfo, LevelAssets levelAssets)
        {
            levelCardInfo.PreviewAudioHandle = levelAssets.PreviewStreamHandle;
            levelCardInfo.OriginalBackgroundPath = levelAssets.OriginalBackgroundPath;
            
            levelCardInfo.BackgroundPreview.texture = 
                levelAssets.PreviewTexture ? levelAssets.PreviewTexture : _defaultBackground.texture;
        }
        
        private void FreeOldestLevel()
        {
            string id = _levelIdOrderList[0];
            _levelIdOrderList.RemoveAt(0);
            Bass.StreamFree(_loadedLevels[id].PreviewStreamHandle);
            Object.Destroy(_loadedLevels[id].PreviewTexture);
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
        
        private static async Task<Texture2D> LoadBackground(string path)
        {
            string cachePath = Path.Combine(Path.GetDirectoryName(path)!, ".bg");

            var tex = new Texture2D(_cacheImageSize, _cacheImageSize, TextureFormat.ARGB32, false);
            tex.LoadRawTextureData(await LoadFileAsync(cachePath));
            tex.Apply();
            return tex;
        }
        
        public static async Task<byte[]> LoadFileAsync(string path)
        {
            using FileStream stream = File.Open(path, FileMode.Open);
            byte[] result = new byte[stream.Length];
            await stream.ReadAsync(result, 0, (int) stream.Length);
            stream.Close();

            return result;
        }

        private class LevelAssets
        {
            public Texture2D PreviewTexture;
            public string OriginalBackgroundPath;
            public int PreviewStreamHandle;
        }
    }
}