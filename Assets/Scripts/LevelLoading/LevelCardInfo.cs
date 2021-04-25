using System;
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
        public Image BackgroundPreview;

        [NonSerialized] public int LevelIndex;

        [NonSerialized] public int PreviewAudioHandle;
    }
}