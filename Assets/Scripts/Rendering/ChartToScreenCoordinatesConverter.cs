using UnityEngine;

namespace CCE.Rendering
{
    public class ChartToScreenCoordinatesConverter : IChartToScreenCoordinatesConverter
    {
        public const float DEFAULT_CYTOID_CAMERA_SIZE = 10.0f;

        public ChartToScreenCoordinatesConverter(Rect playArea, VisualSettings visualSettings = null)
        {
            // In Cytoid, all the calculations were done with the Camera.main.orthograpicSize field
            // that has the half-height of the camera's view, so we also use the halfHeight of the
            // play area rectangle. After that we scale the computations based on how many times
            // smaller the height of the play area is versus the camera size.
            var halfHeight = playArea.height / 2.0f;
            _playAreaRect = playArea;

            _visualSettings = visualSettings ?? new VisualSettings();

            var baseNoteSize = halfHeight * 0.39433f * (1.133333f + _visualSettings.NoteSize);
            _noteSizes.ClickNoteSize = baseNoteSize;
            _noteSizes.FlickNoteSize = baseNoteSize * 1.125f;
            _noteSizes.DragHeadNoteSize = baseNoteSize * 0.8f;
            _noteSizes.CDragHeadNoteSize = baseNoteSize;
            _noteSizes.DragChildNoteSize = baseNoteSize * 0.65f;
            _noteSizes.HoldNoteSize = baseNoteSize;
            _noteSizes.LongHoldNoteSize = baseNoteSize;

            _screenRatio = (float)playArea.width / playArea.height;
            if (_visualSettings.RestrictPlayAreaAspectRatio)
            {
                _screenRatio = Mathf.Clamp(_screenRatio, 4.0f / 3.0f, 16.0f / 9.0f);
            }

            float width = playArea.height * _screenRatio;
            // Taken from Cytoid's source code
            const float topRatio = 0.0966666f;
            const float bottomRatio = 0.07f;
            _horizontalRatio = 0.8f + (4 - _visualSettings.HorizontalMargin) * 0.02f;
            _verticalRatio = 1 - width * (topRatio + bottomRatio) / playArea.height + (3 - _visualSettings.VerticalMargin) * 0.05f;
            // This might make more sense with "height" instead of "width", but that's how Cytoid
            // does the calculation
            _verticalOffset = -(width * (topRatio - (topRatio + bottomRatio) / 2.0f)) * playArea.height / DEFAULT_CYTOID_CAMERA_SIZE;

            // Add offsets based on the play area
            _horizontalOffset = playArea.center.x;
            _verticalOffset += playArea.center.y;
        }

        public float ClickNoteSize { get => _noteSizes.ClickNoteSize; }
        public float FlickNoteSize { get => _noteSizes.FlickNoteSize; }
        public float DragHeadNoteSize { get => _noteSizes.DragHeadNoteSize; }
        public float CDragHeadNoteSize { get => _noteSizes.CDragHeadNoteSize; }
        public float DragChildNoteSize { get => _noteSizes.DragChildNoteSize; }
        public float HoldNoteSize { get => _noteSizes.HoldNoteSize; }
        public float LongHoldNoteSize { get => _noteSizes.LongHoldNoteSize; }
        public float ScreenSize { get => _playAreaRect.height; }
        public float AspectRatio { get => _playAreaRect.width / _playAreaRect.height; }

        public float ScreenXFromChartX(double chartX)
        {
            return ((float)chartX * 2 * _horizontalRatio - _horizontalRatio) * _screenRatio * _playAreaRect.height / 2.0f + _horizontalOffset;
        }

        public float ScreenYFromChartY(double chartY)
        {
            return _verticalRatio * (-_playAreaRect.height / 2.0f + _playAreaRect.height * (float)chartY) + _verticalOffset;
        }

        private struct NoteSizes
        {
            public float ClickNoteSize;
            public float FlickNoteSize;
            public float DragHeadNoteSize;
            public float CDragHeadNoteSize;
            public float DragChildNoteSize;
            public float HoldNoteSize;
            public float LongHoldNoteSize;
        }
        private readonly NoteSizes _noteSizes;
        private readonly VisualSettings _visualSettings;
        private readonly float _horizontalRatio;
        private readonly float _verticalRatio;
        private readonly float _verticalOffset;
        private readonly float _horizontalOffset;
        private readonly float _screenRatio;
        private readonly Rect _playAreaRect;
    }
}