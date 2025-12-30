using System.Collections.Generic;

namespace CCE.Rendering.Notes
{
    /// <summary>
    /// Interface for classes that track the currently visible notes.
    /// </summary>
    public interface INoteProvider
    {
        /// <summary>
        /// Gets all the currently visible click notes.
        /// </summary>
        List<ClickNoteInfo> GetClickNotes();
        /// <summary>
        /// Gets all the currently visible hold notes.
        /// </summary>
        List<HoldNoteInfo> GetHoldNotes();
        /// <summary>
        /// Gets all the currently visible long hold notes.
        /// </summary>
        List<LongHoldNoteInfo> GetLongHoldNotes();
        /// <summary>
        /// Gets all the currently visible flick notes.
        /// </summary>
        List<FlickNoteInfo> GetFlickNotes();
        /// <summary>
        /// Gets all the currently visible drag child notes.
        /// </summary>
        List<DragChildNoteInfo> GetDragChildNotes();
    }
}
