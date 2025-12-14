using System;
using ManagedBass;
using UnityEngine.Assertions;

namespace CCE.Audio
{
    public class AudioBuffer
    {
        public byte[] Data;
        public IntPtr Pointer;
    }

    public class BassAudioStream : IAudioStream
    {
        private readonly AudioBuffer _buffer;
        private float[] _sampleData;

        public BassAudioStream(int handle, AudioBuffer buffer)
        {
            Handle = handle;
            _buffer = buffer;
        }

        public int Handle { get; }

        ~BassAudioStream()
        {
            Assert.AreNotEqual(_buffer.Pointer, IntPtr.Zero);
            Bass.StreamFree(Handle);
        }

        private bool AreBufferLengthsApproximatelyEqual(int bufferLength, int decodedLength)
        {
            return decodedLength <= bufferLength && bufferLength - decodedLength < bufferLength / 10000;
        }

        public ArraySegment<float> GetSampleData()
        {
            if (_sampleData != null)
            {
                return new ArraySegment<float>(_sampleData);
            }

            var bufferLength = (int)Bass.ChannelGetLength(Handle);
            _sampleData = new float[bufferLength / 4]; // Each float is 4 bytes
            var decodedLength = Bass.ChannelGetData(Handle, _sampleData, bufferLength);

            // Decoded length should be approximately equal to buffer length
            if (!AreBufferLengthsApproximatelyEqual(bufferLength, decodedLength))
            {
                throw new Exception($"Failed to get sample data. Expected {bufferLength} bytes, got {decodedLength} bytes.");
            }

            return new ArraySegment<float>(_sampleData);
        }
    }
}