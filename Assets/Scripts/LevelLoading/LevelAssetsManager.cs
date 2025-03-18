using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CCE.Audio.Abstract;
using CCE.Core;
using CCE.Data;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CCE.LevelLoading
{
    /// <summary>
    ///     Utility class for loading, storing and automatic freeing of level assets
    ///     such as backgrounds and preview audio stream handles.
    /// </summary>
    public class LevelAssetsManager
    {
        /// Number of levels of which to keep assets in memory at once.
        private const int PoolSize = 48;

        /// Width and height in pixels of the cached preview image for each level.
        private const int CacheImageSize = 256;

        private readonly HashSet<string> _currentlyProcessingLevels = new();

        private readonly Sprite _defaultBackground =
            GameObject.Find("Screen Background").GetComponent<BackgroundManager>().DefaultBackground;

        private readonly List<string> _levelIdOrderList = new();

        private readonly Dictionary<string, LevelAssets> _loadedLevels = new(PoolSize);

        public async void ScheduleLevelLoad(LevelCardInfo levelCardInfo, Level level)
        {
            if (_loadedLevels.TryGetValue(level.ID, out var loadedLevel))
            {
                _levelIdOrderList.Remove(level.ID);
                _levelIdOrderList.Add(level.ID);
                AddAssetsToCard(levelCardInfo, loadedLevel);
                return;
            }

            if (!_currentlyProcessingLevels.Add(level.ID))
            {
                return;
            }

            if (_currentlyProcessingLevels.Count + _loadedLevels.Count >= PoolSize)
            {
                FreeOldestLevel();
            }

            var backgroundFilePath =
                Path.Combine(GlobalState.Config.LevelStoragePath, level.ID, level.Background.Path);

            var audioPreviewFilePath =
                Path.Combine(GlobalState.Config.LevelStoragePath, level.ID, level.MusicPreview.Path);

            var audioFilePath = Path.Combine(GlobalState.Config.LevelStoragePath, level.ID, level.Music.Path);

            var assets = new LevelAssets
            {
                PreviewAudio =
                    LoadPreviewAudio(File.Exists(audioPreviewFilePath) ? audioPreviewFilePath : audioFilePath),
                OriginalBackgroundPath = backgroundFilePath
            };

            if (GlobalState.Config.LoadBackgroundsInLevelSelect && File.Exists(backgroundFilePath))
            {
                assets.PreviewTexture = await LoadBackground(backgroundFilePath);
            }
            else
            {
                assets.PreviewTexture = null;
            }

            AddAssetsToCard(levelCardInfo, assets);

            _loadedLevels[level.ID] = assets;
            _levelIdOrderList.Add(level.ID);

            _currentlyProcessingLevels.Remove(level.ID);
        }

        private void AddAssetsToCard(LevelCardInfo levelCardInfo, LevelAssets levelAssets)
        {
            levelCardInfo.PreviewAudioHandle = levelAssets.PreviewAudio;
            levelCardInfo.OriginalBackgroundPath = levelAssets.OriginalBackgroundPath;

            levelCardInfo.BackgroundPreview.texture =
                levelAssets.PreviewTexture ? levelAssets.PreviewTexture : _defaultBackground.texture;
        }

        private void FreeOldestLevel()
        {
            var id = _levelIdOrderList[0];
            _levelIdOrderList.RemoveAt(0);
            Object.Destroy(_loadedLevels[id].PreviewTexture);
            _loadedLevels.Remove(id);
        }

        private static IAudioStream LoadPreviewAudio(string path)
        {
            if (!File.Exists(path))
            {
                throw new ArgumentException("Could not find audio file at " + path);
            }

            return GlobalState.AudioManager.CreateStream(path, true);
        }

        private static async Task<Texture2D> LoadBackground(string path)
        {
            var cachePath = Path.Combine(Path.GetDirectoryName(path)!, ".bg");

            var tex = new Texture2D(CacheImageSize, CacheImageSize, TextureFormat.ARGB32, false);
            tex.LoadRawTextureData(await File.ReadAllBytesAsync(cachePath));
            tex.Apply();
            return tex;
        }

        private class LevelAssets
        {
            public string OriginalBackgroundPath;
            public IAudioStream PreviewAudio;
            public Texture2D PreviewTexture;
        }
    }
}