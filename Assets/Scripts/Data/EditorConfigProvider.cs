using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using CCE.Utils;

namespace CCE.Data
{
    public class EditorConfigProvider : SingletonMonoBehaviour<EditorConfigProvider>
    {
        private EditorConfig _config;

        public static EditorConfig Config { get => Instance._config; set => SetConfig(value); }

        private readonly List<IEditorConfigChangedListener> _editorConfigChangedListeners = new();

        public static void SaveConfig()
        {
            File.WriteAllText(Path.Combine(Application.persistentDataPath, "data.txt"),
                JsonConvert.SerializeObject(Config));
        }

        void Awake()
        {
            if (File.Exists(Path.Combine(Application.persistentDataPath, "data.txt")))
            {
                try
                {
                    _config = JsonConvert.DeserializeObject<EditorConfig>(
                        File.ReadAllText(Path.Combine(Application.persistentDataPath, "data.txt")));
                }
                catch (Exception)
                {
                    _config = new EditorConfig();
                }
            }
            else
            {
                _config = new EditorConfig();
            }

            if (!Directory.Exists(_config.DirPath)) _config.DirPath = Application.persistentDataPath;

            if (!Directory.Exists(_config.LevelStoragePath)) Directory.CreateDirectory(_config.LevelStoragePath);
            if (!Directory.Exists(_config.TempStoragePath)) Directory.CreateDirectory(_config.TempStoragePath);
        }

        private static void SetConfig(EditorConfig value)
        {
            Instance._config = value;
            foreach (var listener in Instance._editorConfigChangedListeners)
            {
                listener.OnEditorConfigChanged(value);
            }
        }

        public static void AddConfigChangedListener(IEditorConfigChangedListener listener)
        {
            Instance._editorConfigChangedListeners.Add(listener);
            listener.OnEditorConfigChanged(Config);
        }
    }
}