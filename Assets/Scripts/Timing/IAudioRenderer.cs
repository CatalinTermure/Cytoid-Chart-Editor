using CCE.Audio;
using UnityEngine;

namespace CCE.Timing
{
    /// <summary>
    /// Interface for classes that can render an <see cref="IAudioStream"/> to a <see cref="Texture2D"/>.
    /// </summary>
    public interface IAudioRenderer
    {
        /// <summary>
        /// Renders an audio stream to a texture.
        /// </summary>
        /// <param name="audioStream"> The audio stream to render. Must be opened with <see cref="AudioStreamType.ForDecoding"/> </param>
        Texture2D RenderAudio(IAudioStream audioStream);
    }
}
