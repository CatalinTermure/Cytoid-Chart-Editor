using System;
using System.IO;
using System.Linq;
using CCE.Audio.Abstract;
using CCE.Audio.BASS;
using NUnit.Framework;
using UnityEngine;

namespace CCE.EditorTests
{
    public class BassAudioTests
    {
        public static readonly string SampleAudioPath =
            Path.Combine(Application.dataPath, "Editor", "Resources", "chovvy.test", "test-audio.mp3");

        private BassAudioManager _audioManager;

        [OneTimeSetUp]
        public void SetUp()
        {
            _audioManager = new BassAudioManager();
            _audioManager.Initialize();
        }

        [TestCase(AudioStreamType.ForPlayback)]
        [TestCase(AudioStreamType.ForPlaybackLooping)]
        [TestCase(AudioStreamType.ForDecoding)]
        public void CreateStream_CreatesStream(AudioStreamType audioStreamType)
        {
            var audioData = File.ReadAllBytes(SampleAudioPath);
            var stream = _audioManager.CreateStream(audioData, audioStreamType);
            Assert.IsTrue(stream is not null);
        }

        [Test]
        public void GetAudioData_GetsAudioData()
        {
            var audioData = File.ReadAllBytes(SampleAudioPath);
            var stream = _audioManager.CreateStream(audioData, AudioStreamType.ForDecoding);

            float[] sampleData = stream.GetSampleData();
            Assert.IsTrue(sampleData.Length > 0);
            // all samples should be in the range [-1, 1], with some margin
            // Checking all sample values with Is.All.InRange may return a huge diff, so we do it manually
            for (var i = 0; i < sampleData.Length; i++)
            {
                if (sampleData[i] < -1.1f || sampleData[i] > 1.1f)
                {
                    Assert.Fail($"Sample data should be within the range [-1, 1], with some margin, but we got: {sampleData[i]} at index {i}");
                }
            }
        }
    }
}