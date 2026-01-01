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
        private readonly float _flickArrowMaxOffset;
        private readonly float _longHoldVisibleSize;

        public PlaybackRenderer(INoteProvider noteProvider, IChartToScreenCoordinatesConverter chartToScreenConverter)
        {
            _noteProvider = noteProvider;
            _longHoldVisibleSize = chartToScreenConverter.ScreenSize * 1.0f;
            _flickArrowMaxOffset = chartToScreenConverter.ScreenSize * 0.15f;
        }

        public void Render(double time)
        {
            foreach (ClickNoteInfo clickNoteInfo in _noteProvider.GetClickNotes())
            {
                clickNoteInfo.NoteTransform.localPosition = new Vector3(clickNoteInfo.X, clickNoteInfo.Y, 0.0f);
                float approachPercentage = (float)((time - clickNoteInfo.IntroTime) / (clickNoteInfo.Time - clickNoteInfo.IntroTime));
                float noteSize = clickNoteInfo.Size * (0.4f + approachPercentage * 0.6f);
                clickNoteInfo.NoteTransform.localScale = new Vector3(noteSize, noteSize, 1.0f);
                clickNoteInfo.NoteFillTransform.localScale = new Vector3(approachPercentage, approachPercentage, 1.0f);
                float opacity = clickNoteInfo.Opacity * approachPercentage;
                clickNoteInfo.NoteFill.color = clickNoteInfo.NoteFill.color.WithAlpha(opacity);
                clickNoteInfo.NoteRing.color = clickNoteInfo.NoteRing.color.WithAlpha(opacity);
            }

            foreach (HoldNoteInfo holdNoteInfo in _noteProvider.GetHoldNotes())
            {
                holdNoteInfo.NoteTransform.localPosition = new Vector3(holdNoteInfo.X, holdNoteInfo.Y, 0.0f);
                float approachPercentage = Mathf.Clamp01((float)((time - holdNoteInfo.IntroTime) / (holdNoteInfo.StartTime - holdNoteInfo.IntroTime)));
                float noteSize = holdNoteInfo.Size * (0.4f + approachPercentage * 0.6f);
                holdNoteInfo.NoteTransform.localScale = new Vector3(noteSize, noteSize, 1.0f);
                float opacity = holdNoteInfo.Opacity * approachPercentage;
                holdNoteInfo.NoteFill.color = holdNoteInfo.NoteFill.color.WithAlpha(opacity);
                holdNoteInfo.NoteRing.color = holdNoteInfo.NoteRing.color.WithAlpha(opacity);
                Vector2 bodyBackgroundScale = holdNoteInfo.NoteBodyBackground.size;
                holdNoteInfo.NoteBodyBackground.color = holdNoteInfo.NoteBodyBackground.color.WithAlpha(opacity);
                holdNoteInfo.NoteBodyBackground.size = new Vector2(1.0f, bodyBackgroundScale.y);
                float completionPercentage = Mathf.Clamp01((float)((time - holdNoteInfo.StartTime) / (holdNoteInfo.EndTime - holdNoteInfo.StartTime)));
                holdNoteInfo.NoteCompletedBody.size = new Vector2(1.0f, bodyBackgroundScale.y * completionPercentage);
                // When miniaturizing the chart, we want the bars of the hold notes to have less space between them
                float scalingFactor = holdNoteInfo.NoteBodyTransform.localScale.y;
                holdNoteInfo.NoteBodyTransform.localScale = new Vector2(approachPercentage * 0.6f, scalingFactor / holdNoteInfo.Size) / (0.4f + approachPercentage * 0.6f);
            }

            foreach (LongHoldNoteInfo longHoldNoteInfo in _noteProvider.GetLongHoldNotes())
            {
                longHoldNoteInfo.NoteTransform.localPosition = new Vector3(longHoldNoteInfo.X, longHoldNoteInfo.Y, 0.0f);
                float approachPercentage = Mathf.Clamp01((float)((time - longHoldNoteInfo.IntroTime) / (longHoldNoteInfo.StartTime - longHoldNoteInfo.IntroTime)));
                float noteSize = longHoldNoteInfo.Size * (0.4f + approachPercentage * 0.6f);
                longHoldNoteInfo.NoteTransform.localScale = new Vector3(noteSize, noteSize, 1.0f);
                float opacity = longHoldNoteInfo.Opacity * approachPercentage;
                longHoldNoteInfo.NoteFill.color = longHoldNoteInfo.NoteFill.color.WithAlpha(opacity);
                longHoldNoteInfo.NoteRing.color = longHoldNoteInfo.NoteRing.color.WithAlpha(opacity);
                longHoldNoteInfo.NoteBodyBackgroundTop.color = longHoldNoteInfo.NoteBodyBackgroundTop.color.WithAlpha(opacity);
                longHoldNoteInfo.NoteBodyBackgroundBottom.color = longHoldNoteInfo.NoteBodyBackgroundBottom.color.WithAlpha(opacity);
                float completionPercentage = Mathf.Clamp01((float)((time - longHoldNoteInfo.StartTime) / (longHoldNoteInfo.EndTime - longHoldNoteInfo.StartTime)));
                // When miniaturizing the chart, we want the bars of the hold notes to have less space between them
                float scalingFactor = longHoldNoteInfo.NoteBodyTransform.localScale.y;
                longHoldNoteInfo.NoteCompletedBodyTop.size = new Vector2(1.0f, (_longHoldVisibleSize / 2 - longHoldNoteInfo.Y) * completionPercentage / scalingFactor);
                longHoldNoteInfo.NoteCompletedBodyBottom.size = new Vector2(1.0f, (_longHoldVisibleSize / 2 + longHoldNoteInfo.Y) * completionPercentage / scalingFactor);
                longHoldNoteInfo.NoteBodyTransform.localScale = new Vector2(approachPercentage * 0.6f, scalingFactor / longHoldNoteInfo.Size) / (0.4f + approachPercentage * 0.6f);
            }

            foreach (FlickNoteInfo flickNoteInfo in _noteProvider.GetFlickNotes())
            {
                flickNoteInfo.NoteTransform.localPosition = new Vector3(flickNoteInfo.X, flickNoteInfo.Y, 0.0f);
                float approachPercentage = (float)((time - flickNoteInfo.IntroTime) / (flickNoteInfo.Time - flickNoteInfo.IntroTime));
                float arrowApproachPercentage = Mathf.Clamp01((float)((time - flickNoteInfo.IntroTime) / (flickNoteInfo.Time - flickNoteInfo.IntroTime - 0.25f)));
                float noteSize = flickNoteInfo.Size * (0.4f + approachPercentage * 0.6f);
                flickNoteInfo.NoteTransform.localScale = new Vector3(noteSize, noteSize, 1.0f);
                flickNoteInfo.NoteFillTransform.localScale = new Vector3(approachPercentage, approachPercentage, 1.0f);
                flickNoteInfo.LeftArrowTransform.localPosition = Vector3.Lerp(
                    new Vector3(-_flickArrowMaxOffset, 0.0f, 0.0f),
                    Vector3.zero,
                    arrowApproachPercentage) / noteSize;
                flickNoteInfo.RightArrowTransform.localPosition = Vector3.Lerp(
                    new Vector3(_flickArrowMaxOffset, 0.0f, 0.0f),
                    Vector3.zero,
                    arrowApproachPercentage) / noteSize;
                float opacity = flickNoteInfo.Opacity * approachPercentage;
                flickNoteInfo.NoteFill.color = flickNoteInfo.NoteFill.color.WithAlpha(opacity);
                flickNoteInfo.NoteRing.color = flickNoteInfo.NoteRing.color.WithAlpha(opacity);
                flickNoteInfo.LeftArrow.color = flickNoteInfo.LeftArrow.color.WithAlpha(opacity);
                flickNoteInfo.RightArrow.color = flickNoteInfo.RightArrow.color.WithAlpha(opacity);
            }

            foreach (DragChildNoteInfo dragChildNoteInfo in _noteProvider.GetDragChildNotes())
            {
                dragChildNoteInfo.NoteTransform.localPosition = new Vector3(dragChildNoteInfo.X, dragChildNoteInfo.Y, 0.0f);
                float approachPercentage = (float)((time - dragChildNoteInfo.IntroTime) / (dragChildNoteInfo.Time - dragChildNoteInfo.IntroTime));
                float noteSize = dragChildNoteInfo.Size * (0.7f + approachPercentage * 0.3f);
                dragChildNoteInfo.NoteTransform.localScale = new Vector3(noteSize, noteSize, 1.0f);
                float opacity = dragChildNoteInfo.Opacity * approachPercentage;
                dragChildNoteInfo.NoteFill.color = dragChildNoteInfo.NoteFill.color.WithAlpha(opacity);
            }
        }
    }
}
