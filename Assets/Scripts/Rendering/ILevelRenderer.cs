namespace CCE.Rendering
{
    /// <summary>
    /// Interface for interacting with various level renderers.
    /// </summary>
    public interface ILevelRenderer
    {
        /// <summary>
        /// Updates all the note visuals to what they should be at a specific time. This assumes the
        /// notes exist.
        /// </summary>
        void Render(double time);
    }
}
