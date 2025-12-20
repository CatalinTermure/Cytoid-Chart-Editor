using CCE.Utils;
using UnityEngine;
using UnityEngine.Android;

namespace CCE.Core
{
    public class ScreenDimensionsProvider : SingletonMonoBehaviour<ScreenDimensionsProvider>
    {
        public static float PlayAreaWidth { get => Instance._playAreaWidth; }
        public const float PlayAreaHeight = 12;
        public const float NormalAspectRatio = 16f / 9f;
        public static readonly float AspectRatio = (float)Screen.width / Screen.height;
        public static float UnityHeight { get => Instance._height; }

        void Awake()
        {
            _playAreaWidth = 24 * AspectRatio / NormalAspectRatio;
            _height = Camera.main!.orthographicSize;
        }

        private float _playAreaWidth;
        private float _height;
    }
}