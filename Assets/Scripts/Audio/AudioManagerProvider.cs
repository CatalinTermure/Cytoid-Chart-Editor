using CCE.Data;
using CCE.Utils;

namespace CCE.Audio
{
    public class AudioManagerProvider : SingletonMonoBehaviour<AudioManagerProvider>, IEditorConfigChangedListener
    {
        private IAudioManager _audioManager;

        public static IAudioManager AudioManager { get => Instance._audioManager; set => Instance._audioManager = value; }

        void Awake()
        {
            _audioManager = new BassAudioManager();
            _audioManager.Initialize();
            EditorConfigProvider.AddConfigChangedListener(this);
        }

        void OnApplicationQuit()
        {
            AudioManager.Stop();
        }

        public void OnEditorConfigChanged(EditorConfig config)
        {
            _audioManager.SetHitsoundVolume(config.HitsoundVolume);
            _audioManager.SetMusicVolume(config.MusicVolume);
        }
    }
}