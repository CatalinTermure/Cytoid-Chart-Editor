using CCE.Rendering.Notes;
using UnityEngine;

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
            foreach (ClickNoteInfo clickNoteInfo in _noteProvider.GetClickNotes())
            {
                float approachPercentage = (float)((time - clickNoteInfo.StartTime) / (clickNoteInfo.EndTime - clickNoteInfo.StartTime));
                float noteSize = clickNoteInfo.Size * (0.4f + approachPercentage * 0.6f);
                clickNoteInfo.NoteTransform.localScale = new Vector3(noteSize, noteSize, 1.0f);
                clickNoteInfo.NoteFillTransform.localScale = new Vector3(approachPercentage, approachPercentage, 1.0f);
                float opacity = clickNoteInfo.Opacity * approachPercentage;
                clickNoteInfo.NoteFill.color = clickNoteInfo.NoteFill.color.WithAlpha(opacity);
                clickNoteInfo.NoteRing.color = clickNoteInfo.NoteRing.color.WithAlpha(opacity);
            }
        }
    }
}
