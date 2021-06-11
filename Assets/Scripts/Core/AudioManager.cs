using ManagedBass;
using UnityEngine;

namespace CCE.Core
{
    /// <summary>
    /// Class responsible for playing audio resources.
    /// </summary>
    public static class AudioManager
    { // TODO: make everything static and integrate fully with BASS API, see: MainScene
        public static bool IsInitialized;

        public static bool IsPlaying;

        public static double PlaybackSpeed = 1;

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

        public static double MaxTime =>
            Bass.ChannelBytes2Seconds(_audioHandle,
                Bass.ChannelGetLength(_audioHandle));

        /// <summary>
        /// Plays the <see cref="AudioClip"/> 
        /// loaded into the <see cref="AudioManager"/>.
        /// </summary>
        public static double Play()
        {
            if (!IsInitialized || IsPlaying || _audioHandle == 0)
            {
                return 0;
            }
            
            IsPlaying = true;
            Bass.ChannelPlay(_audioHandle);

            return AudioSettings.dspTime;
        }

        public static void Pause()
        {
            IsPlaying = false;
            Bass.ChannelPause(_audioHandle);
        }

        public static void Stop()
        {
            IsPlaying = false;
            Bass.ChannelStop(_audioHandle);
        }

        private static void UnloadAudio()
        {
            Bass.StreamFree(_audioHandle);
        }
        
        /// <summary>
        /// Loads audio from the specified handle.
        /// </summary>
        /// <param name="handle"> Handle to a BASS stream. </param>
        public static void LoadAudio(int handle)
        {
            Bass.ChannelStop(_audioHandle);
            UnloadAudio();
            _audioHandle = handle;
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