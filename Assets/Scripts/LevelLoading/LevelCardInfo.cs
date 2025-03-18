using System;
using CCE.Audio.Abstract;
using UnityEngine;
using UnityEngine.UI;

namespace CCE.LevelLoading
{
    public class LevelCardInfo : MonoBehaviour
    {
        public RectTransform RectTransform;
        public Text ArtistName;
        public Text CharterName;
        public Text Title;
        public RawImage BackgroundPreview;
        [NonSerialized] public int LevelIndex;

        [NonSerialized] public string OriginalBackgroundPath;
        [NonSerialized] public IAudioStream PreviewAudioHandle;
    }
}