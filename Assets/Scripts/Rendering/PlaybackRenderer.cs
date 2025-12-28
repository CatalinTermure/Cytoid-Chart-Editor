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
        private readonly float _flickArrowMaxOffset = 5.0f * 0.3f;

        public PlaybackRenderer(INoteProvider noteProvider)
        {
            _noteProvider = noteProvider;
        }

        public void Render(double time)
        {
            foreach (ClickNoteInfo clickNoteInfo in _noteProvider.GetClickNotes())
            {
                clickNoteInfo.NoteTransform.localPosition = new Vector3(clickNoteInfo.X, clickNoteInfo.Y, 0.0f);
                float approachPercentage = (float)((time - clickNoteInfo.StartTime) / (clickNoteInfo.EndTime - clickNoteInfo.StartTime));
                float noteSize = clickNoteInfo.Size * (0.4f + approachPercentage * 0.6f);
                clickNoteInfo.NoteTransform.localScale = new Vector3(noteSize, noteSize, 1.0f);
                clickNoteInfo.NoteFillTransform.localScale = new Vector3(approachPercentage, approachPercentage, 1.0f);
                float opacity = clickNoteInfo.Opacity * approachPercentage;
                clickNoteInfo.NoteFill.color = clickNoteInfo.NoteFill.color.WithAlpha(opacity);
                clickNoteInfo.NoteRing.color = clickNoteInfo.NoteRing.color.WithAlpha(opacity);
                if (clickNoteInfo.gameObject.activeSelf == false)
                {
                    clickNoteInfo.gameObject.SetActive(true);
                }
            }
            foreach (FlickNoteInfo flickNoteInfo in _noteProvider.GetFlickNotes())
            {
                flickNoteInfo.NoteTransform.localPosition = new Vector3(flickNoteInfo.X, flickNoteInfo.Y, 0.0f);
                float approachPercentage = (float)((time - flickNoteInfo.StartTime) / (flickNoteInfo.EndTime - flickNoteInfo.StartTime));
                float arrowApproachPercentage = Mathf.Clamp01((float)((time - flickNoteInfo.StartTime) / (flickNoteInfo.EndTime - flickNoteInfo.StartTime - 0.25f)));
                float noteSize = flickNoteInfo.Size * (0.4f + approachPercentage * 0.6f);
                flickNoteInfo.NoteTransform.localScale = new Vector3(noteSize, noteSize, 1.0f);
                flickNoteInfo.NoteFillTransform.localScale = new Vector3(approachPercentage, approachPercentage, 1.0f);
                flickNoteInfo.LeftArrowTransform.localPosition = Vector3.Lerp(
                    new Vector3(-_flickArrowMaxOffset, 0.0f, 0.0f),
                    Vector3.zero,
                    arrowApproachPercentage);
                flickNoteInfo.RightArrowTransform.localPosition = Vector3.Lerp(
                    new Vector3(_flickArrowMaxOffset, 0.0f, 0.0f),
                    Vector3.zero,
                    arrowApproachPercentage);
                float opacity = flickNoteInfo.Opacity * approachPercentage;
                flickNoteInfo.NoteFill.color = flickNoteInfo.NoteFill.color.WithAlpha(opacity);
                flickNoteInfo.NoteRing.color = flickNoteInfo.NoteRing.color.WithAlpha(opacity);
                flickNoteInfo.LeftArrow.color = flickNoteInfo.LeftArrow.color.WithAlpha(opacity);
                flickNoteInfo.RightArrow.color = flickNoteInfo.RightArrow.color.WithAlpha(opacity);
                if (flickNoteInfo.gameObject.activeSelf == false)
                {
                    flickNoteInfo.gameObject.SetActive(true);
                }
            }
        }
    }
}
