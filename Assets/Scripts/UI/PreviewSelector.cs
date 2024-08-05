using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using CCE.Core;
using CCE.Data;
using CCE.LevelLoading;
using CCE.Utils;
using ManagedBass;
using ManagedBass.Enc;
using UnityEngine;
using UnityEngine.UI;

namespace CCE.UI
{
    public class PreviewSelector : MonoBehaviour
    {
        [SerializeField] private InputField StartTimeInputField;
        [SerializeField] private InputField EndTimeInputField;

        [SerializeField] private GameObject StartBar;
        [SerializeField] private GameObject EndBar;

        [SerializeField] private RawImage WaveformCanvas;

        [SerializeField] private ToastMessageManager MessageToaster;
        [NonSerialized] public LevelDataDisplay LevelDataDisplay;

        private const int _minimumPreviewDuration = 5;
        private const int _maximumPreviewDuration = 30;

        private bool _isSampleDataValid;
        private float[] _sampleData;
        private int _decodeStream;

        private PreviewBarDragController _startBarDragController;
        private PreviewBarDragController _endBarDragController;
        private double _startTime;
        private double _endTime;
        private double _audioLength; // in seconds

        private int _canvasWidth;
        private int _canvasHeight;

        private void Awake()
        {
            _startBarDragController = StartBar.GetComponent<PreviewBarDragController>();
            _endBarDragController = EndBar.GetComponent<PreviewBarDragController>();

            _startBarDragController.OnDragged.AddListener(OnStartBarDragged);
            _endBarDragController.OnDragged.AddListener(OnEndBarDragged);

            StartTimeInputField.onEndEdit.AddListener(StartTimeTextChanged);
            EndTimeInputField.onEndEdit.AddListener(EndTimeTextChanged);

            Rect canvasRect = WaveformCanvas.rectTransform.rect;
            _canvasWidth = (int)canvasRect.width;
            _canvasHeight = (int)canvasRect.height;

            _audioLength = AudioManager.MaxTime;
        }

        private void Start()
        {
            MessageToaster.CreateToast("Loading audio file info...", 10000);
            new Thread(GetAudioData).Start();

            SetStartTime(0.0);
            SetEndTime(GlobalState.Clamp(5.0, 0, _audioLength));
        }

        private void Update()
        {
            if (AudioManager.IsPlaying && AudioManager.Time >= _endTime)
            {
                AudioManager.Stop();
            }

            if (_isSampleDataValid)
            {
                StartCoroutine(GenerateWaveformCoroutine());
                _isSampleDataValid = false;
            }
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
            if (_startTime + _minimumPreviewDuration > _audioLength)
            {
                SetStartTime(_audioLength - _minimumPreviewDuration);
            }

            if (_endTime - _startTime > _maximumPreviewDuration)
            {
                SetEndTime(_startTime + _maximumPreviewDuration);
            }

            if (_endTime - _startTime < _minimumPreviewDuration)
            {
                SetEndTime(_startTime + _minimumPreviewDuration);
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
            if (_endTime - _minimumPreviewDuration < 0)
            {
                SetEndTime(_minimumPreviewDuration);
            }

            if (_endTime - _startTime > _maximumPreviewDuration)
            {
                SetStartTime(_endTime - _maximumPreviewDuration);
            }

            if (_endTime - _startTime < _minimumPreviewDuration)
            {
                SetStartTime(_endTime - _minimumPreviewDuration);
            }
        }

        private void StartTimeTextChanged(string timeText)
        {
            double time = TimestampParser.Parse(timeText);
            SetStartTime(time);
            CheckStartTime();
        }

        private void EndTimeTextChanged(string timeText)
        {
            double time = TimestampParser.Parse(timeText);
            SetEndTime(time);
            CheckEndTime();
        }

        private void GetAudioData()
        {
            string audioPath = Path.Combine(GlobalState.Config.LevelStoragePath, GlobalState.CurrentLevel.ID,
                GlobalState.CurrentLevel.Music.Path);

            _decodeStream =
                Bass.CreateStream(audioPath, 0, 0, BassFlags.Decode | BassFlags.Mono | BassFlags.Float);

            int bufferLength = (int)Bass.ChannelGetLength(_decodeStream);
            byte[] resultBuffer = new byte[bufferLength];
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
            int waveformPortionsPerFrame = _canvasWidth / 120;

            var waveform = new Texture2D(_canvasWidth, _canvasHeight);

            int chunkSize = _sampleData.Length / _canvasWidth + 1;
            float[] waveformData = new float[_sampleData.Length / chunkSize + 1];

            float maximumValue = 0.0f;
            int index = 0;
            for (int i = 0; i < _sampleData.Length; i += chunkSize)
            {
                if (i % computationsPerFrame == 0) yield return null;
                waveformData[index] = Mathf.Abs(_sampleData[i]);
                if (waveformData[index] > maximumValue) maximumValue = waveformData[index];
                index++;
            }

            float scalingFactor = 1.0f / maximumValue; // scales the waveform data
            // so it fits more nicely in the [-1, 1] interval
            for (int i = 0; i < index; i++)
            {
                if (i % waveformPortionsPerFrame == 0) yield return null;

                int midPoint = _canvasHeight / 2;
                int barSize = (int)(midPoint * waveformData[i] * scalingFactor);
                for (int j = 0; j <= barSize; j++)
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
            if (AudioManager.IsPlaying)
            {
                AudioManager.Stop();
                return;
            }

            AudioManager.Time = _startTime;
            AudioManager.Play();
        }

        public void SavePreview()
        {
            LevelDataDisplay.DidPreviewChange = true;
            string previewFilePath = Path.Combine(GlobalState.CurrentLevelPath, "tmp-preview.ogg");

            int startSample = (int)Bass.ChannelSeconds2Bytes(_decodeStream, _startTime);
            int endSample = (int)Bass.ChannelSeconds2Bytes(_decodeStream, _endTime);
            int sampleCount = (endSample - startSample) / 4;

            float[] previewData = new float[sampleCount];
            GCHandle previewDataHandle = GCHandle.Alloc(previewData, GCHandleType.Pinned);
            IntPtr previewDataPtr = previewDataHandle.AddrOfPinnedObject();
            
            Buffer.BlockCopy(_sampleData, startSample, previewData, 0, sampleCount * 4);

            int encoderHandle = BassEnc_Ogg.Start(_decodeStream, null, EncodeFlags.ConvertFloatTo16BitInt, previewFilePath);

            int windowSize = (int)Bass.ChannelSeconds2Bytes(_decodeStream, BassEnc.Queue / 1000.0) / 8;
            int dataLength = sampleCount * 4;
            for (int index = 0; index + windowSize < dataLength; index += windowSize)
            {
                BassEnc.EncodeWrite(encoderHandle, previewDataPtr, windowSize);
                previewDataPtr = IntPtr.Add(previewDataPtr, windowSize);
            }
            
            BassEnc.EncodeWrite(encoderHandle, previewDataPtr, dataLength % windowSize);
            BassEnc.EncodeStop(encoderHandle);

            previewDataHandle.Free();
            
            MessageToaster.CreateToast("Preview audio saved!");
        }

        private void OnDisable()
        {
            Bass.StreamFree(_decodeStream);
        }
    }
}