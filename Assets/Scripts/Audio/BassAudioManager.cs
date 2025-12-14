using System;
using System.IO;
using ManagedBass;
using ManagedBass.Fx;
using UnityEngine;
using UnityEngine.Assertions;

namespace CCE.Audio
{
    public class BassAudioManager : IAudioManager
    {
        private const int ConcurrentHitsoundCount = 4;
        private readonly int[] _hitsoundChannels = new int[ConcurrentHitsoundCount];
        private int _hitsoundChannelIndex;
        private int _hitsoundHandle;
        private float _hitsoundVolume = 1;
        private bool _isPlaybackSpeedEditable;

        // Handle to the original audio stream to apply effects on.
        private BassAudioStream _loadedAudioStream;

        private float _musicVolume = 1;

        // Handle to the audio stream used for playback.
        private int _playingAudioHandle;

        public bool IsInitialized { get; private set; }
        public bool IsPlaying { get; private set; }

        public double Time
        {
            get => Bass.ChannelBytes2Seconds(_playingAudioHandle, Bass.ChannelGetPosition(_playingAudioHandle));
            set => Bass.ChannelSetPosition(_playingAudioHandle, Bass.ChannelSeconds2Bytes(_playingAudioHandle, value));
        }

        public double MaxTime =>
            Bass.ChannelBytes2Seconds(_playingAudioHandle, Bass.ChannelGetLength(_playingAudioHandle));

        public void SetPlaybackSpeed(double playbackSpeed)
        {
            if (!_isPlaybackSpeedEditable)
            {
                throw new InvalidOperationException("CCELog: Tried to change playback speed without loading the" +
                                                    " audio for playback speed editing. See: LoadAudio.");
            }

            var success =
                Bass.ChannelSetAttribute(_playingAudioHandle, ChannelAttribute.Tempo, (playbackSpeed - 1) * 100);
            if (!success)
            {
                HandleBassError($"Could not set playback speed of {_playingAudioHandle} to {playbackSpeed}");
            }
        }

        public double Play()
        {
            if (!IsInitialized || IsPlaying || _loadedAudioStream == null)
            {
                return 0;
            }

            IsPlaying = true;
            var success = Bass.ChannelPlay(_playingAudioHandle);
            if (!success)
            {
                HandleBassError($"Could not play audio on channel {_playingAudioHandle}");
            }

            return AudioSettings.dspTime;
        }

        public void Pause()
        {
            if (!IsPlaying || _playingAudioHandle == 0) return;
            IsPlaying = false;
            var success = Bass.ChannelPause(_playingAudioHandle);
            if (!success)
            {
                HandleBassError($"Could not pause audio on channel {_playingAudioHandle}");
            }
        }

        public void Stop()
        {
            if (!IsPlaying || _playingAudioHandle == 0) return;
            IsPlaying = false;
            var success = Bass.ChannelStop(_playingAudioHandle);
            if (!success)
            {
                HandleBassError($"Could not stop audio on channel {_playingAudioHandle}");
            }
        }

        public void LoadAudio(IAudioStream audioStream, bool loadForPlaybackSpeed = false)
        {
            if (audioStream == null)
            {
                Debug.Log("CCELog: Tried to load null audio stream.");
                return;
            }

            if (audioStream is not BassAudioStream bassAudio)
            {
                throw new ArgumentException("CCELog: Audio stream must be of type BassAudioStream.");
            }

            Stop();
            _loadedAudioStream = bassAudio;
            _isPlaybackSpeedEditable = loadForPlaybackSpeed;

            if (loadForPlaybackSpeed)
            {
                if (_playingAudioHandle != 0)
                {
                    var success = Bass.StreamFree(_playingAudioHandle);
                    if (!success)
                    {
                        HandleBassError($"Could not free previous stream with handle {_playingAudioHandle}");
                    }
                }

                _playingAudioHandle =
                    BassFx.TempoCreate(_loadedAudioStream.Handle, BassFlags.Default | BassFlags.FxFreeSource);
                if (_playingAudioHandle == 0)
                {
                    HandleBassError(
                        $"Could not create playback speed editable stream from handle {_loadedAudioStream}");
                }
            }
            else
            {
                _playingAudioHandle = _loadedAudioStream.Handle;
            }

            SetMusicVolume(_musicVolume);
        }

        private void Cleanup()
        {
            var success = Bass.Free();
            if (!success)
            {
                HandleBassError("Could not free BASS.");
            }

            IsInitialized = false;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private static void HandleBassError(string errorMessage)
        {
            Debug.LogError(errorMessage);
            Assert.AreNotEqual(Bass.LastError, Errors.OK);
            throw new BassException(Bass.LastError);
        }

        private void LoadDefaultHitsounds()
        {
            var hitsoundClip = Resources.Load<AudioClip>("hitsound");
            hitsoundClip.LoadAudioData();

            var sampleCount = hitsoundClip.samples * hitsoundClip.channels;
            var samples = new float[sampleCount];

            hitsoundClip.GetData(samples, 0);

            _hitsoundHandle =
                Bass.CreateSample(sampleCount * 4, hitsoundClip.frequency, hitsoundClip.channels,
                    ConcurrentHitsoundCount, BassFlags.Float | BassFlags.SampleOverrideLongestPlaying);
            if (_hitsoundHandle == 0)
            {
                HandleBassError("Could not create hitsound sample for default hitsounds.");
            }

            var success = Bass.SampleSetData(_hitsoundHandle, samples);
            if (!success)
            {
                HandleBassError("Could not set hitsound sample data for default hitsounds.");
            }
        }

        private void LoadHitsounds()
        {
            var customHitsoundPath = Path.Combine(Application.persistentDataPath, "Hitsound.wav");
            if (File.Exists(customHitsoundPath))
            {
                _hitsoundHandle = Bass.SampleLoad(customHitsoundPath, 0, 0,
                    ConcurrentHitsoundCount, BassFlags.Default);
                if (_hitsoundHandle == 0)
                {
                    HandleBassError("Could not load custom hitsound sample.");
                }
            }
            else
            {
                LoadDefaultHitsounds();
            }

            for (var i = 0; i < ConcurrentHitsoundCount; i++)
            {
                _hitsoundChannels[i] = Bass.SampleGetChannel(_hitsoundHandle, true);
                if (_hitsoundChannels[i] == 0)
                {
                    HandleBassError($"Could not get channel for hitsound {i}");
                }

                var success = Bass.ChannelSetAttribute(_hitsoundChannels[i], ChannelAttribute.Volume, _hitsoundVolume);
                if (!success)
                {
                    HandleBassError(
                        $"Could not set hitsound volume to {_hitsoundVolume} for channel {_hitsoundChannels[i]}");
                }
            }

            SetHitsoundVolume(_hitsoundVolume);
        }

        public void PlayHitsound()
        {
            var success = Bass.ChannelPlay(_hitsoundChannels[_hitsoundChannelIndex++], true);
            if (!success)
            {
                HandleBassError($"Could not play hitsound on channel {_hitsoundChannelIndex - 1}");
            }

            if (_hitsoundChannelIndex == ConcurrentHitsoundCount)
            {
                _hitsoundChannelIndex = 0;
            }
        }

        public void SetHitsoundVolume(float volume)
        {
            for (var i = 0; i < ConcurrentHitsoundCount; i++)
            {
                var success = Bass.ChannelSetAttribute(_hitsoundChannels[i], ChannelAttribute.Volume, volume);
                if (!success)
                {
                    HandleBassError($"Could not set hitsound volume to {volume} for channel {_hitsoundChannels[i]}");
                }
            }

            _hitsoundVolume = volume;
        }

        public void SetMusicVolume(float volume)
        {
            _musicVolume = volume;

            if (_playingAudioHandle == 0) return;
            var success = Bass.ChannelSetAttribute(_playingAudioHandle, ChannelAttribute.Volume, volume);
            if (!success)
            {
                HandleBassError($"Could not set music volume to {volume} for channel {_playingAudioHandle}");
            }
        }

        public void Initialize()
        {
            bool success = true;
#if UNITY_STANDALONE_WIN
            success = Bass.Configure(Configuration.TruePlayPosition, 0);
            if (!success)
            {
                HandleBassError("Could not configure BASS TruePlayPosition.");
            }
#endif

            success = Bass.Configure(Configuration.DevNonStop, true);
            if (!success)
            {
                HandleBassError("Could not configure BASS DeviceNonStop.");
            }

            success = Bass.Init();
            if (!success)
            {
#if UNITY_EDITOR
                if (Bass.LastError == Errors.Already)
                {
                    Debug.Log("Could not start BASS, please restart unity.");
                    return;
                }
#endif
                HandleBassError("Could not start BASS.");
            }

            IsInitialized = true;

            LoadHitsounds();
        }

        private static AudioBuffer CreateBuffer(byte[] data)
        {
            var buffer = new AudioBuffer
            {
                Data = data,
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

        private BassFlags GetFlagsForAudioStreamType(AudioStreamType audioStreamType)
        {
            return audioStreamType switch
            {
                AudioStreamType.ForPlayback => BassFlags.Decode,
                AudioStreamType.ForPlaybackLooping => BassFlags.Default | BassFlags.Loop,
                AudioStreamType.ForDecoding => BassFlags.Decode | BassFlags.Float,
                _ => throw new ArgumentOutOfRangeException(nameof(audioStreamType), audioStreamType, null)
            };
        }

        public IAudioStream CreateStream(byte[] data, AudioStreamType audioStreamType)
        {
            var buffer = CreateBuffer(data);
            var flags = GetFlagsForAudioStreamType(audioStreamType);
            var handle = Bass.CreateStream(buffer.Pointer, 0, buffer.Data.Length, flags);
            if (handle == 0)
            {
                HandleBassError($"Could not create stream from data of length {data.Length} with flags {flags}. Error: {Bass.LastError}. AudioStreamType: {audioStreamType}");
            }

            return new BassAudioStream(handle, buffer);
        }

        ~BassAudioManager()
        {
            Cleanup();
        }
    }
}