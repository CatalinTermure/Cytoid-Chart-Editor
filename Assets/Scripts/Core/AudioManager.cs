using System.IO;
using ManagedBass;
using UnityEngine;
using UnityEngine.Networking;

namespace CCE.Core
{
    /// <summary>
    /// Class responsible for playing audio resources.
    /// </summary>
    public class AudioManager
    { // TODO: make everything static and integrate fully with BASS API, see: MainScene
        public static bool IsInitialized;

        public bool IsPlaying;

        public double PlaybackSpeed = 1;

        private static int _audioHandle;

        private static AudioClip _music;
        
        public static double Time
        {
            get => Bass.ChannelBytes2Seconds(_audioHandle,
                Bass.ChannelGetPosition(_audioHandle));

            set =>
                Bass.ChannelSetPosition(_audioHandle, 
                    Bass.ChannelSeconds2Bytes(_audioHandle, value));
        }

        public double MaxTime
        {
            get => Bass.ChannelBytes2Seconds(_audioHandle,
                Bass.ChannelGetLength(_audioHandle));
        }

        /// <summary>
        /// Plays the <see cref="AudioClip"/> 
        /// loaded into the <see cref="AudioManager"/>.
        /// </summary>
        public double Play()
        {
            if (!IsInitialized || IsPlaying || _audioHandle == 0)
            {
                return 0;
            }
            
            IsPlaying = true;
            Bass.ChannelPlay(_audioHandle);

            return AudioSettings.dspTime;
        }

        public static void PlayBass()
        { // TODO: remove this and integrate it with Play()
            if (!IsInitialized || _audioHandle == 0)
            {
                return;
            }

            Bass.ChannelPlay(_audioHandle);
        }

        public void Pause()
        {
            IsPlaying = false;
            Bass.ChannelPause(_audioHandle);
        }

        public static void Stop()
        {
            Bass.ChannelStop(_audioHandle);
        }

        public AudioManager(string musicPath)
        {
            LoadAudio(musicPath);
        }
        
        public void UnloadAudio()
        {
            if (_music == null) return;
            _music.UnloadAudioData();
            Object.Destroy(_music);
            _music = null;
        }

        private static void UnloadAudioBass()
        {
            Bass.StreamFree(_audioHandle);
        }
        
        /// <summary>
        /// Loads audio from the file specified by path.
        /// </summary>
        /// <param name="path"> Path of the audio file to load. </param>
        private void LoadAudio(string path)
        {
            UnloadAudio();
            LoadMusicFromFile(path);
            _music.LoadAudioData();
        }

        /// <summary>
        /// Loads audio from the specified handle.
        /// </summary>
        /// <param name="handle"> Handle to a BASS stream. </param>
        public static void LoadAudio(int handle)
        {
            Bass.ChannelStop(_audioHandle);
            UnloadAudioBass();
            _audioHandle = handle;
        }

        private void LoadMusicFromFile(string path)
        {
            AudioType type = AudioType.UNKNOWN;
            switch (Path.GetExtension(path))
            {
                case ".ogg":
                    type = AudioType.OGGVORBIS;
                    break;
                case ".mp3":
                    type = AudioType.MPEG;
                    break;
                case ".wav":
                    type = AudioType.WAV;
                    break;
                default:
                    Debug.LogError("CCELog: Audio file type is unsupported.");
                    break;
            }

            Logging.AddToLog(Path.Combine(Application.persistentDataPath, "LoadChartLog.txt"), $"Loading music file from path: file://{path}\n");

            using (var www =
                UnityWebRequestMultimedia.GetAudioClip("file://" + path, type))
            {
                var req = www.SendWebRequest();

                while (!req.isDone)
                {

                }

                if (www.result == UnityWebRequest.Result.ConnectionError ||
                    www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("CCELog: " + www.error);
                }
                else
                {
                    _music = DownloadHandlerAudioClip.GetContent(www);
                }
            }
        }

        public static void Initialize()
        {
            Bass.Configure(Configuration.TruePlayPosition, 0);
            Bass.Configure(Configuration.DevNonStop, true);

            Bass.Init();

            IsInitialized = true;
        }
    }
}