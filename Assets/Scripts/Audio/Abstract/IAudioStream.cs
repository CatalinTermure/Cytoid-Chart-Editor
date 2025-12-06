using System;

namespace CCE.Audio.Abstract
{
    public interface IAudioStream
    {
        ArraySegment<float> GetSampleData();
    }
}