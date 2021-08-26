using UnityEngine;
using UnityEngine.Events;

namespace CCE.UI
{
    public class MessagePopupController : MonoBehaviour
    {
        private static MessagePopupController _instance;
        [SerializeField] private GameObject PopupTemplate;

        private void OnEnable()
        {
            if (_instance == null) _instance = this;
            else Debug.LogError($"Instantiating multiple {nameof(MessagePopupController)} singletons.");
        }

        private void OnDisable()
        {
            _instance = null;
        }

        public static void ShowPopup(string message, UnityAction acceptCallback, UnityAction declineCallback)
        {
            GameObject obj = Instantiate(_instance.PopupTemplate, Vector3.zero, Quaternion.identity,
                _instance.transform);

            var messagePopupInfo = obj.GetComponent<MessagePopupInfo>();
            messagePopupInfo.AcceptButton.onClick.AddListener(acceptCallback);
            messagePopupInfo.DeclineButton.onClick.AddListener(declineCallback);
            messagePopupInfo.AcceptButton.onClick.AddListener(() => Destroy(obj));
            messagePopupInfo.DeclineButton.onClick.AddListener(() => Destroy(obj));
            messagePopupInfo.MessageText.text = message;
        }
    }
}