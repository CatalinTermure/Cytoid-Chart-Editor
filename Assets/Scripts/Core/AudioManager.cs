using CCE.Utils;
using ManagedBass;
using ManagedBass.Fx;
using UnityEngine;

namespace CCE.Core
{
    /// <summary>
    ///     Class responsible for playing audio resources.
    /// </summary>
    public static class AudioManager
    {
        public static bool IsInitialized;

        public static bool IsPlaying;
        
        private static int _audioHandle; // Handle to the original audio stream
        
        /// <summary>
        /// Handle to the audio stream used for playback.
        /// </summary>
        private static int _channelStream;

        private static bool _isPlaybackSpeedEditable;

        private static AudioClip _music;
        private static double _playbackSpeed;

        public static double PlaybackSpeed
        {
            get => _playbackSpeed;
            set
            {
                _playbackSpeed = value;
                if (!_isPlaybackSpeedEditable)
                {
                    Debug.LogError("CCELog: Tried to change playback speed without loading the" +
                                   " audio for playback speed editing. See: LoadAudio.");
                    return;
                }
                
                Bass.ChannelSetAttribute(_channelStream, ChannelAttribute.Tempo, (value - 1) * 100);
                BassUtils.PrintLastError();
            }
        }

        public static double Time
        {
            get => Bass.ChannelBytes2Seconds(_channelStream, Bass.ChannelGetPosition(_channelStream));
            set => Bass.ChannelSetPosition(_channelStream, Bass.ChannelSeconds2Bytes(_channelStream, value));
        }

        public static double MaxTime =>
            Bass.ChannelBytes2Seconds(_channelStream, Bass.ChannelGetLength(_channelStream));

        

        /// <summary>
        ///     Plays the <see cref="AudioClip" />
        ///     loaded into the <see cref="AudioManager" />.
        /// </summary>
        public static double Play()
        {
            if (!IsInitialized || IsPlaying || _audioHandle == 0)
            {
                return 0;
            }

            IsPlaying = true;
            Bass.ChannelPlay(_channelStream);
            BassUtils.PrintLastError();


            return AudioSettings.dspTime;
        }

        public static void Pause()
        {
            IsPlaying = false;
            Bass.ChannelPause(_channelStream);
        }

        public static void Stop()
        {
            IsPlaying = false;
            Bass.ChannelStop(_channelStream);
        }

        /// <summary>
        ///     Loads audio from the specified handle.
        /// </summary>
        /// <param name="handle"> Handle to a BASS stream. </param>
        /// <param name="loadForPlaybackSpeed"> Indicates if the audio should be loaded for playback speed functionality </param>
        public static void LoadAudio(int handle, bool loadForPlaybackSpeed = false)
        {
            Stop();
            _audioHandle = handle;
            _isPlaybackSpeedEditable = loadForPlaybackSpeed;
            
            if (loadForPlaybackSpeed)
            {
                Bass.StreamFree(_channelStream);
                _channelStream = BassFx.TempoCreate(_audioHandle, BassFlags.Default | BassFlags.FxFreeSource);
                BassUtils.PrintLastError();
            }
            else
            {
                _channelStream = _audioHandle;
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