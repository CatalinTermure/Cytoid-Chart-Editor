using CCE.Utils;
using ManagedBass;
using ManagedBass.Fx;
using System.Linq;
using System.IO;
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

        // Handle to the original audio stream to apply effects on.
        private static int _audioHandle;

        // Handle to the audio stream used for playback.
        private static int _audioChannel;

        private const int _concurrentHitsoundCount = 4;
        private static int _hitsoundHandle;
        private static int[] _hitsoundChannels = new int[_concurrentHitsoundCount];
        private static int _hitsoundChannelIndex;

        private static bool _isPlaybackSpeedEditable;

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

                Bass.ChannelSetAttribute(_audioChannel, ChannelAttribute.Tempo, (value - 1) * 100);
                BassUtils.PrintLastError();
            }
        }

        public static double Time
        {
            get => Bass.ChannelBytes2Seconds(_audioChannel, Bass.ChannelGetPosition(_audioChannel));
            set => Bass.ChannelSetPosition(_audioChannel, Bass.ChannelSeconds2Bytes(_audioChannel, value));
        }

        public static double MaxTime =>
            Bass.ChannelBytes2Seconds(_audioChannel, Bass.ChannelGetLength(_audioChannel));

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
            Bass.ChannelPlay(_audioChannel);
            BassUtils.PrintLastError();


            return AudioSettings.dspTime;
        }

        public static void Pause()
        {
            IsPlaying = false;
            Bass.ChannelPause(_audioChannel);
        }

        public static void Stop()
        {
            IsPlaying = false;
            Bass.ChannelStop(_audioChannel);
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
                Bass.StreamFree(_audioChannel);
                _audioChannel = BassFx.TempoCreate(_audioHandle, BassFlags.Default | BassFlags.FxFreeSource);
                BassUtils.PrintLastError();
            }
            else
            {
                _audioChannel = _audioHandle;
            }
        }

        private static void LoadDefaultHitsounds()
        {
            AudioClip hitsoundClip = Resources.Load<AudioClip>("hitsound");
            hitsoundClip.LoadAudioData();

            int sampleCount = hitsoundClip.samples * hitsoundClip.channels;
            float[] samples = new float[sampleCount];

            hitsoundClip.GetData(samples, 0);

            _hitsoundHandle =
                Bass.CreateSample(sampleCount * 4, hitsoundClip.frequency, hitsoundClip.channels,
                    _concurrentHitsoundCount, BassFlags.Float | BassFlags.SampleOverrideLongestPlaying);

            Bass.SampleSetData(_hitsoundHandle, samples);
        }

        private static void LoadHitsounds()
        {
            string customHitsoundPath = Path.Combine(Application.persistentDataPath, "Hitsound.wav");
            if (File.Exists(customHitsoundPath))
            {
                _hitsoundHandle = Bass.SampleLoad(customHitsoundPath, 0, 0,
                    _concurrentHitsoundCount, BassFlags.Default);
            }
            else
            {
                LoadDefaultHitsounds();
            }

            for (int i = 0; i < _concurrentHitsoundCount; i++)
            {
                _hitsoundChannels[i] = Bass.SampleGetChannel(_hitsoundHandle, true);
            }
        }

        public static void PlayHitsound()
        {
            Bass.ChannelPlay(_hitsoundChannels[_hitsoundChannelIndex++], true);
            if (_hitsoundChannelIndex == _concurrentHitsoundCount) _hitsoundChannelIndex = 0;
        }

        public static void SetHitsoundVolume(double volume)
        {
            for (int i = 0; i < _concurrentHitsoundCount; i++)
            {
                Bass.ChannelSetAttribute(_hitsoundChannels[i], ChannelAttribute.Volume, volume);
            }
        }

        public static void SetMusicVolume(double volume)
        {
            Bass.ChannelSetAttribute(_audioChannel, ChannelAttribute.Volume, volume);
        }

        public static void Initialize()
        {
            Bass.Configure(Configuration.TruePlayPosition, 0);
            Bass.Configure(Configuration.DevNonStop, true);

            Bass.Init();
#if UNITY_EDITOR
            if(Bass.LastError == Errors.Already)
            {
                Debug.Log("Could not start BASS, please restart unity.");
                return;
            }
#endif

            LoadHitsounds();

            IsInitialized = true;
        }
    }
}