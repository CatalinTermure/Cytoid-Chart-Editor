using CCE.Rendering.Notes;

namespace CCE.Rendering
{
    /// <summary>
    /// Renderer for playback mode.
    /// </summary>
    public class PlaybackRenderer : ILevelRenderer
    {
        private readonly INoteProvider _noteProvider;

        public PlaybackRenderer(INoteProvider noteProvider)
        {
            _noteProvider = noteProvider;
        }

        public void Render(double time)
        {
            throw new System.NotImplementedException();
        }
    }
}
