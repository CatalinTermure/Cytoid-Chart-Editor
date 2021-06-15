using CCE.Core;
using UnityEngine;
using UnityEngine.UI;

namespace CCE.MusicLoading
{
    public class PlayPauseButtonController : MonoBehaviour
    {
        public IHighlightable HighlightItem;
        [SerializeField] private Image ImageComponent;
        [SerializeField] private Sprite PlayButtonSprite;
        [SerializeField] private Sprite PauseButtonSprite;

        private bool _isPlaying;

        public void OnClick()
        {
            _isPlaying = !_isPlaying;
            
            if(_isPlaying)
                Play();
            else
                Pause();
        }
        
        private void Play()
        {
            ImageComponent.sprite = PlayButtonSprite;
            HighlightItem?.Highlight();
            AudioManager.Play();
        }

        private void Pause()
        {
            ImageComponent.sprite = PauseButtonSprite;
            HighlightItem?.Highlight();
            AudioManager.Pause();
        }
    }
}
