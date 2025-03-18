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
        public int Handle { get; }
        private readonly AudioBuffer _buffer;
        
        public BassAudioStream(int handle, AudioBuffer buffer)
        {
            Handle = handle;
            _buffer = buffer;
        }
        
        ~BassAudioStream()
        {
            Assert.AreNotEqual(_buffer.Pointer, IntPtr.Zero);
            Bass.StreamFree(Handle);
        }
    }
}