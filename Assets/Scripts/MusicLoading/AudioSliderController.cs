using System.Text;
using CCE.Core;
using UnityEngine;
using UnityEngine.UI;

namespace CCE.MusicLoading
{
    public class AudioSliderController : MonoBehaviour
    {
        public Text TimeTrackerText;
        
        private Slider _attachedSlider;

        private void Render()
        {
            _attachedSlider.value = (float)(AudioManager.Time / AudioManager.MaxTime);
        }

        private void RenderTimeText()
        {
            if (TimeTrackerText == null) return;
            
            double time = AudioManager.Time;
            double maxTime = AudioManager.MaxTime;
            var sb = new StringBuilder();
            sb.Append(((int)time / 60).ToString("D2"));
            sb.Append(':');
            sb.Append(((int)time % 60).ToString("D2"));
            sb.Append(':');
            sb.Append(((int)((time - (int)time) * 1000)).ToString("D3"));
            sb.Append(" / ");
            sb.Append(((int)maxTime / 60).ToString("D2"));
            sb.Append(':');
            sb.Append(((int)maxTime % 60).ToString("D2"));
            sb.Append(':');
            sb.Append(((int)((maxTime - (int)maxTime) * 1000)).ToString("D3"));
            TimeTrackerText.text = sb.ToString();
        }

        public void Update()
        {
            if (!AudioManager.IsPlaying) return;
            Render();
            RenderTimeText();
        }

        public void Awake()
        {
            _attachedSlider = gameObject.GetComponent<Slider>();
        }
    }
}