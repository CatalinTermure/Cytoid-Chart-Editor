using UnityEngine;

namespace CCE.Storyboard
{
    public class StoryboardSceneUI : MonoBehaviour
    {
        [SerializeField] private StoryboardSceneController _controller;

        public void TogglePlayPause()
        {
            _controller.TogglePlayPause();
        }

        public void TimeSliderValueChanged(float value)
        {
            _controller.SetTime(value);
        }
    }
}
