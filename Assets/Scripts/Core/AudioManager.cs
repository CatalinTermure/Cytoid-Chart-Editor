using System.IO;
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
        private const int ConcurrentHitsoundCount = 4;

        public static bool IsInitialized;
        public static bool IsPlaying;

        // Handle to the original audio stream to apply effects on.
        private static int _audioHandle;

        // Handle to the audio stream used for playback.
        private static int _audioChannel;
        private static int _hitsoundHandle;
        private static readonly int[] _hitsoundChannels = new int[ConcurrentHitsoundCount];
        private static int _hitsoundChannelIndex;
        private static bool _isPlaybackSpeedEditable;

        private static float _musicVolume = 1;
        private static float _hitsoundVolume = 1;

        public static double Time
        {
            get => Bass.ChannelBytes2Seconds(_audioChannel, Bass.ChannelGetPosition(_audioChannel));
            set => Bass.ChannelSetPosition(_audioChannel, Bass.ChannelSeconds2Bytes(_audioChannel, value));
        }

        public static double MaxTime =>
            Bass.ChannelBytes2Seconds(_audioChannel, Bass.ChannelGetLength(_audioChannel));

        public static void SetPlaybackSpeed(double value)
        {
            if (!_isPlaybackSpeedEditable)
            {
                Debug.LogError("CCELog: Tried to change playback speed without loading the" +
                               " audio for playback speed editing. See: LoadAudio.");
                return;
            }

            Bass.ChannelSetAttribute(_audioChannel, ChannelAttribute.Tempo, (value - 1) * 100);
            BassUtils.PrintLastError();
        }

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
        /// <param name="loadForPlaybackSpeed"> Indicates if the playback speed may be changed eventually for this audio handle </param>
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

            Bass.ChannelSetAttribute(_audioChannel, ChannelAttribute.Volume, _musicVolume);
        }

        private static void LoadDefaultHitsounds()
        {
            var hitsoundClip = Resources.Load<AudioClip>("hitsound");
            hitsoundClip.LoadAudioData();

            var sampleCount = hitsoundClip.samples * hitsoundClip.channels;
            var samples = new float[sampleCount];

            hitsoundClip.GetData(samples, 0);

            _hitsoundHandle =
                Bass.CreateSample(sampleCount * 4, hitsoundClip.frequency, hitsoundClip.channels,
                    ConcurrentHitsoundCount, BassFlags.Float | BassFlags.SampleOverrideLongestPlaying);

            Bass.SampleSetData(_hitsoundHandle, samples);
        }

        private static void LoadHitsounds()
        {
            var customHitsoundPath = Path.Combine(Application.persistentDataPath, "Hitsound.wav");
            if (File.Exists(customHitsoundPath))
            {
                _hitsoundHandle = Bass.SampleLoad(customHitsoundPath, 0, 0,
                    ConcurrentHitsoundCount, BassFlags.Default);
            }
            else
            {
                LoadDefaultHitsounds();
            }

            for (var i = 0; i < ConcurrentHitsoundCount; i++)
            {
                _hitsoundChannels[i] = Bass.SampleGetChannel(_hitsoundHandle, true);
                Bass.ChannelSetAttribute(_hitsoundChannels[i], ChannelAttribute.Volume, _hitsoundVolume);
            }
        }

        public static void PlayHitsound()
        {
            Bass.ChannelPlay(_hitsoundChannels[_hitsoundChannelIndex++], true);
            if (_hitsoundChannelIndex == ConcurrentHitsoundCount) _hitsoundChannelIndex = 0;
        }

        public static void SetHitsoundVolume(float volume)
        {
            for (var i = 0; i < ConcurrentHitsoundCount; i++)
            {
                Bass.ChannelSetAttribute(_hitsoundChannels[i], ChannelAttribute.Volume, volume);
            }

            _hitsoundVolume = volume;
        }

        public static void SetMusicVolume(float volume)
        {
            Bass.ChannelSetAttribute(_audioChannel, ChannelAttribute.Volume, volume);
            _musicVolume = volume;
        }

        public static void Initialize()
        {
            Bass.Configure(Configuration.TruePlayPosition, 0);
            Bass.Configure(Configuration.DevNonStop, true);

            Bass.Init();
#if UNITY_EDITOR
            if (Bass.LastError == Errors.Already)
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