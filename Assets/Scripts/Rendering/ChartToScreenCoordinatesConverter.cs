using UnityEngine;

namespace CCE.Rendering
{
    public class ChartToScreenCoordinatesConverter : IChartToScreenCoordinatesConverter
    {
        public const float CAMERA_ORTOGRAPHIC_SIZE = 5.0f;

        public ChartToScreenCoordinatesConverter(VisualSettings visualSettings = null)
        {
            _visualSettings = visualSettings ?? new VisualSettings();

            var baseNoteSize = CAMERA_ORTOGRAPHIC_SIZE * 0.39433f;
            baseNoteSize *= 1.133333f + _visualSettings.NoteSize;
            _noteSizes.ClickNoteSize = baseNoteSize;
            _noteSizes.FlickNoteSize = baseNoteSize * 1.125f;
            _noteSizes.DragHeadNoteSize = baseNoteSize * 0.8f;
            _noteSizes.CDragHeadNoteSize = baseNoteSize;
            _noteSizes.DragChildNoteSize = baseNoteSize * 0.65f;
            _noteSizes.HoldNoteSize = baseNoteSize;
            _noteSizes.LongHoldNoteSize = baseNoteSize;

            _screenRatio = (float)Screen.width / Screen.height;
            if (_visualSettings.RestrictPlayAreaAspectRatio)
            {
                _screenRatio = Mathf.Clamp(_screenRatio, 4.0f / 3.0f, 16.0f / 9.0f);
            }

            float height = CAMERA_ORTOGRAPHIC_SIZE * 2.0f;
            float width = height * _screenRatio;
            // Taken from Cytoid's source code
            const float topRatio = 0.0966666f;
            const float bottomRatio = 0.07f;
            _horizontalRatio = 0.8f + (4 - _visualSettings.HorizontalMargin) * 0.02f;
            _verticalRatio = 1 - width * (topRatio + bottomRatio) / height + (3 - _visualSettings.VerticalMargin) * 0.05f;
            _verticalOffset = -(width * (topRatio - (topRatio + bottomRatio) / 2.0f));
        }

        public float ClickNoteSize { get => _noteSizes.ClickNoteSize; }
        public float FlickNoteSize { get => _noteSizes.FlickNoteSize; }
        public float DragHeadNoteSize { get => _noteSizes.DragHeadNoteSize; }
        public float CDragHeadNoteSize { get => _noteSizes.CDragHeadNoteSize; }
        public float DragChildNoteSize { get => _noteSizes.DragChildNoteSize; }
        public float HoldNoteSize { get => _noteSizes.HoldNoteSize; }
        public float LongHoldNoteSize { get => _noteSizes.LongHoldNoteSize; }
        public float ScreenSize { get => CAMERA_ORTOGRAPHIC_SIZE * 2.0f; }

        public float ScreenXFromChartX(double chartX)
        {
            return ((float)chartX * 2 * _horizontalRatio - _horizontalRatio) * CAMERA_ORTOGRAPHIC_SIZE * _screenRatio;
        }

        public float ScreenYFromChartY(double chartY)
        {
            return _verticalRatio * (-CAMERA_ORTOGRAPHIC_SIZE + 2.0f * CAMERA_ORTOGRAPHIC_SIZE * (float)chartY) + _verticalOffset;
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
        private readonly float _screenRatio;
    }
}