using System;

namespace CCE.Audio
{
    public interface IAudioStream
    {
        ArraySegment<float> GetSampleData();
    }
}