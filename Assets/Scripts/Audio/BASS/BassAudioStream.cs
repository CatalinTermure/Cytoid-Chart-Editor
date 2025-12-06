using System;
using CCE.Audio.Abstract;
using ManagedBass;
using UnityEngine.Assertions;

namespace CCE.Audio.BASS
{
    public class AudioBuffer
    {
        public byte[] Data;
        public IntPtr Pointer;
    }

    public class BassAudioStream : IAudioStream
    {
        private readonly AudioBuffer _buffer;

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

        public float[] GetSampleData()
        {
            var bufferLength = (int)Bass.ChannelGetLength(Handle);
            var resultBuffer = new float[bufferLength / 4]; // Each float is 4 bytes
            var decodedLength = Bass.ChannelGetData(Handle, resultBuffer, bufferLength);

            // Decoded length should be approximately equal to buffer length
            if (!AreBufferLengthsApproximatelyEqual(bufferLength, decodedLength))
            {
                throw new Exception($"Failed to get sample data. Expected {bufferLength} bytes, got {decodedLength} bytes.");
            }

            return resultBuffer;
        }
    }
}