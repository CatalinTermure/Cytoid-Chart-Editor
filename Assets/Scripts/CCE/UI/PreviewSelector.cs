using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using CCE.Core;
using CCE.Utils;
using ManagedBass;
using ManagedBass.Enc;
using UnityEngine;
using UnityEngine.UI;

namespace CCE.UI
{
    public class PreviewSelector : MonoBehaviour
    {
        private const int MinimumPreviewDuration = 5;
        private const int MaximumPreviewDuration = 30;
        [SerializeField] private InputField StartTimeInputField;
        [SerializeField] private InputField EndTimeInputField;

        [SerializeField] private GameObject StartBar;
        [SerializeField] private GameObject EndBar;

        [SerializeField] private RawImage WaveformCanvas;

        [SerializeField] private ToastMessageManager MessageToaster;
        private double _audioLength; // in seconds
        private int _canvasHeight;

        private int _canvasWidth;
        private int _decodeStream;
        private PreviewBarDragController _endBarDragController;
        private double _endTime;

        private bool _isSampleDataValid;
        private float[] _sampleData;

        private PreviewBarDragController _startBarDragController;
        private double _startTime;
        [NonSerialized] public LevelDataDisplay LevelDataDisplay;

        private void Awake()
        {
            _startBarDragController = StartBar.GetComponent<PreviewBarDragController>();
            _endBarDragController = EndBar.GetComponent<PreviewBarDragController>();

            _startBarDragController.OnDragged.AddListener(OnStartBarDragged);
            _endBarDragController.OnDragged.AddListener(OnEndBarDragged);

            StartTimeInputField.onEndEdit.AddListener(StartTimeTextChanged);
            EndTimeInputField.onEndEdit.AddListener(EndTimeTextChanged);

            var canvasRect = WaveformCanvas.rectTransform.rect;
            _canvasWidth = (int)canvasRect.width;
            _canvasHeight = (int)canvasRect.height;

            _audioLength = GlobalState.AudioManager.MaxTime;
        }

        private void Start()
        {
            MessageToaster.CreateToast("Loading audio file info...", 10000);
            new Thread(GetAudioData).Start();

            SetStartTime(0.0);
            SetEndTime(MiscUtils.Clamp(5.0, 0, _audioLength));
        }

        private void Update()
        {
            if (GlobalState.AudioManager.IsPlaying && GlobalState.AudioManager.Time >= _endTime)
            {
                GlobalState.AudioManager.Stop();
            }

            if (_isSampleDataValid)
            {
                StartCoroutine(GenerateWaveformCoroutine());
                _isSampleDataValid = false;
            }
        }

        private void OnDisable()
        {
            Bass.StreamFree(_decodeStream);
        }

        private void SetStartTime(double time)
        {
            _startTime = time;
            _startBarDragController.SetPositionWithoutNotify((float)(_startTime / _audioLength));
            StartTimeInputField.SetTextWithoutNotify(TimestampParser.Serialize(_startTime));
        }

        private void SetEndTime(double time)
        {
            _endTime = time;
            _endBarDragController.SetPositionWithoutNotify((float)(_endTime / _audioLength));
            EndTimeInputField.SetTextWithoutNotify(TimestampParser.Serialize(_endTime));
        }

        private void OnStartBarDragged(float position)
        {
            _startTime = _audioLength * position;
            CheckStartTime();

            StartTimeInputField.SetTextWithoutNotify(TimestampParser.Serialize(_startTime));
        }

        private void CheckStartTime()
        {
            if (_startTime + MinimumPreviewDuration > _audioLength)
            {
                SetStartTime(_audioLength - MinimumPreviewDuration);
            }

            if (_endTime - _startTime > MaximumPreviewDuration)
            {
                SetEndTime(_startTime + MaximumPreviewDuration);
            }

            if (_endTime - _startTime < MinimumPreviewDuration)
            {
                SetEndTime(_startTime + MinimumPreviewDuration);
            }
        }

        private void OnEndBarDragged(float position)
        {
            _endTime = _audioLength * position;
            CheckEndTime();

            EndTimeInputField.SetTextWithoutNotify(TimestampParser.Serialize(_endTime));
        }

        private void CheckEndTime()
        {
            if (_endTime - MinimumPreviewDuration < 0)
            {
                SetEndTime(MinimumPreviewDuration);
            }

            if (_endTime - _startTime > MaximumPreviewDuration)
            {
                SetStartTime(_endTime - MaximumPreviewDuration);
            }

            if (_endTime - _startTime < MinimumPreviewDuration)
            {
                SetStartTime(_endTime - MinimumPreviewDuration);
            }
        }

        private void StartTimeTextChanged(string timeText)
        {
            var time = TimestampParser.Parse(timeText);
            SetStartTime(time);
            CheckStartTime();
        }

        private void EndTimeTextChanged(string timeText)
        {
            var time = TimestampParser.Parse(timeText);
            SetEndTime(time);
            CheckEndTime();
        }

        private void GetAudioData()
        {
            var audioPath = Path.Combine(GlobalState.Config.LevelStoragePath, GlobalState.CurrentLevel.ID,
                GlobalState.CurrentLevel.Music.Path);

            _decodeStream =
                Bass.CreateStream(audioPath, 0, 0, BassFlags.Decode | BassFlags.Mono | BassFlags.Float);

            var bufferLength = (int)Bass.ChannelGetLength(_decodeStream);
            var resultBuffer = new byte[bufferLength];
            if (bufferLength != Bass.ChannelGetData(_decodeStream, resultBuffer, bufferLength))
            {
                throw new Exception("Could not get audio data for waveform");
            }

            _sampleData = new float[bufferLength / 4];
            Buffer.BlockCopy(resultBuffer, 0, _sampleData, 0, bufferLength);

            _isSampleDataValid = true;
        }

        private IEnumerator GenerateWaveformCoroutine()
        {
            const int computationsPerFrame = 10000;
            var waveformPortionsPerFrame = _canvasWidth / 120;

            var waveform = new Texture2D(_canvasWidth, _canvasHeight);

            var chunkSize = _sampleData.Length / _canvasWidth + 1;
            var waveformData = new float[_sampleData.Length / chunkSize + 1];

            var maximumValue = 0.0f;
            var index = 0;
            for (var i = 0; i < _sampleData.Length; i += chunkSize)
            {
                if (i % computationsPerFrame == 0) yield return null;
                waveformData[index] = Mathf.Abs(_sampleData[i]);
                if (waveformData[index] > maximumValue) maximumValue = waveformData[index];
                index++;
            }

            // scales the waveform data to fit into the [-1, 1] range
            var scalingFactor = 1.0f / maximumValue;
            for (var i = 0; i < index; i++)
            {
                if (i % waveformPortionsPerFrame == 0) yield return null;

                var midPoint = _canvasHeight / 2;
                var barSize = (int)(midPoint * waveformData[i] * scalingFactor);
                for (var j = 0; j <= barSize; j++)
                {
                    waveform.SetPixel(i, midPoint + j, Color.black);
                    waveform.SetPixel(i, midPoint - j, Color.black);
                }
            }

            waveform.Apply();

            WaveformCanvas.texture = waveform;

            MessageToaster.CreateToast("Audio file info loaded!");
        }

        public void PlayPreview()
        {
            if (GlobalState.AudioManager.IsPlaying)
            {
                GlobalState.AudioManager.Stop();
                return;
            }

            GlobalState.AudioManager.Time = _startTime;
            GlobalState.AudioManager.Play();
        }

        public void SavePreview()
        {
            LevelDataDisplay.DidPreviewChange = true;
            var previewFilePath = Path.Combine(GlobalState.CurrentLevelPath, "tmp-preview.ogg");

            var startSample = (int)Bass.ChannelSeconds2Bytes(_decodeStream, _startTime);
            var endSample = (int)Bass.ChannelSeconds2Bytes(_decodeStream, _endTime);
            var sampleCount = (endSample - startSample) / 4;

            var previewData = new float[sampleCount];
            var previewDataHandle = GCHandle.Alloc(previewData, GCHandleType.Pinned);
            var previewDataPtr = previewDataHandle.AddrOfPinnedObject();

            Buffer.BlockCopy(_sampleData, startSample, previewData, 0, sampleCount * 4);

            var encoderHandle =
                BassEnc_Ogg.Start(_decodeStream, null, EncodeFlags.ConvertFloatTo16BitInt, previewFilePath);

            var windowSize = (int)Bass.ChannelSeconds2Bytes(_decodeStream, BassEnc.Queue / 1000.0) / 8;
            var dataLength = sampleCount * 4;
            for (var index = 0; index + windowSize < dataLength; index += windowSize)
            {
                BassEnc.EncodeWrite(encoderHandle, previewDataPtr, windowSize);
                previewDataPtr = IntPtr.Add(previewDataPtr, windowSize);
            }

            BassEnc.EncodeWrite(encoderHandle, previewDataPtr, dataLength % windowSize);
            BassEnc.EncodeStop(encoderHandle);

            previewDataHandle.Free();

            MessageToaster.CreateToast("Preview audio saved!");
        }
    }
}