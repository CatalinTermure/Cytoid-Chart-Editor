using UnityEngine;
using UnityEngine.UI;

namespace CCE.Utils
{
    /// <summary>
    ///     Class responsible for showing toast messages.
    ///     Must be attached to a <see cref="GameObject" /> with a <see cref="Text" /> component.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class ToastMessageManager : MonoBehaviour
    {
        private float _toastEndTime = -1;
        private Text _textComponent;

        private void Awake()
        {
            _textComponent = gameObject.GetComponent<Text>();
        }

        private void Update()
        {
            if (_toastEndTime < 0) return;

            if (_toastEndTime < Time.time)
            {
                _textComponent.text = null;
                _toastEndTime = -1;
            }
            else
            {
                var c = _textComponent.color;
                _textComponent.color = new Color(c.r, c.g, c.b, _toastEndTime - Time.time);
            }
        }

        public void CreateToast(string toast, int toastDuration = 3)
        {
            _textComponent.text = toast;
            _toastEndTime = Time.time + toastDuration;
        }
    }
}