using System.Collections.Generic;
using CCE.Core;
using UnityEngine;
using UnityEngine.UI;

namespace CCE.MusicLoading
{
    public class MusicItemController : MonoBehaviour, IHighlightable
    {
        [HideInInspector] public int AudioHandle;
        [HideInInspector] public MusicInfoController MusicInfoController;

        [SerializeField] private Text TitleText;
        [SerializeField] private Color HighlightColor;
        [SerializeField] private List<Graphic> HighlightableObjects;
        [SerializeField] private PlayPauseButtonController PlayPauseButton;

        private string _title;

        public bool Highlighted { get; set; }

        public void Highlight()
        {
            if (AudioHandle == 0)
            {
                Debug.LogError("CCELog: Trying to play non-existing audio track.\n" +
                               "In: MusicItemController.Highlight()");
                return;
            }

            Highlighted = !Highlighted;

            if (Highlighted)
            {
                if (MusicInfoController != null) MusicInfoController.SetTitle(_title);
                HighlightableObjects.ForEach(item => item.color = HighlightColor);
                AudioManager.LoadAudio(AudioHandle);
            }
            else
            {
                HighlightableObjects.ForEach(item => item.color = Color.white);
            }
        }

        public void SetTitle(string title)
        {
            _title = title;
            TitleText.text = _title;
        }
        
        private void Awake()
        {
            PlayPauseButton.HighlightItem = this;
        }
    }
}