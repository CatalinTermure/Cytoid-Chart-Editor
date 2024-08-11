using System;
using System.Collections.Generic;
using System.IO;
using CCE.Data;
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

        public static AudioStream CurrentAudioStream => new() { Handle = _audioHandle };

        private struct AudioBuffer
        {
            public byte[] Data;
            public IntPtr Pointer;
        }

        private static readonly Dictionary<AudioStream, AudioBuffer> _streamBuffers = new();

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

#if !UNITY_EDITOR
        public static void Cleanup()
        {
            Bass.Free();
        }
#endif

        /// <summary>
        ///     Loads the audio stream into the <see cref="AudioManager" />.
        /// </summary>
        /// <param name="audio"> Stream containing audio to be loaded. </param>
        /// <param name="loadForPlaybackSpeed">
        ///     If set, playback speed can be edited. If not set, <see cref="SetPlaybackSpeed" />
        ///     does nothing.
        /// </param>
        public static void LoadAudio(AudioStream audio, bool loadForPlaybackSpeed = false)
        {
            Stop();
            _audioHandle = audio.Handle;
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
            IsInitialized = true;

#if UNITY_EDITOR
            if (Bass.LastError == Errors.Already)
            {
                Debug.Log("Could not start BASS, please restart unity.");
                return;
            }
#endif

            LoadHitsounds();
        }

        public static void Free(AudioStream stream)
        {
            if (!_streamBuffers.ContainsKey(stream))
            {
                throw new ArgumentException($"Trying to free stream that was not loaded. Handle: {stream.Handle}");
            }

            Bass.StreamFree(stream.Handle);

            _streamBuffers.Remove(stream);
        }

        private static AudioBuffer CreateBuffer(string path)
        {
            var buffer = new AudioBuffer
            {
                Data = File.ReadAllBytes(path),
                Pointer = IntPtr.Zero
            };
            unsafe
            {
                fixed (byte* ptr = buffer.Data)
                {
                    buffer.Pointer = (IntPtr)ptr;
                }
            }

            return buffer;
        }

        public static AudioStream CreateStream(string path, bool looping = false)
        {
            var buffer = CreateBuffer(path);
            var flags = looping ? BassFlags.Loop : BassFlags.Decode;
            var handle = Bass.CreateStream(buffer.Pointer, 0, buffer.Data.Length, flags);
            _streamBuffers.Add(new AudioStream { Handle = handle }, buffer);
            BassUtils.PrintLastError();
            return new AudioStream { Handle = handle };
        }
    }
}