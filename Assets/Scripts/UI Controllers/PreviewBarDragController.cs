using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace CCE.UI
{
    public class PreviewBarDragController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public UnityEvent<float> OnDragged;
        
        private RectTransform _rectTransform;

        private float _position;
        private float _startPosition;
        private float _parentWidth;

        private void Awake()
        {
            GameObject obj = gameObject;
            _rectTransform = obj.GetComponent<RectTransform>();
            _parentWidth = obj.transform.parent.gameObject.GetComponent<RectTransform>().sizeDelta.x;
        }

        /// <param name="position"> Position should be in the [0, 1] range. </param>
        public void SetPositionWithoutNotify(float position)
        {
            _position = position;
            _rectTransform.anchoredPosition = new Vector2(position * _parentWidth, _rectTransform.anchoredPosition.y);
        }

        public void OnDrag(PointerEventData eventData)
        {
            float position = 
                Mathf.Clamp01(_startPosition + (eventData.position.x - eventData.pressPosition.x) / _parentWidth);
            SetPositionWithoutNotify(position);
            OnDragged.Invoke(position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _position = _rectTransform.anchoredPosition.x / _parentWidth;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _startPosition = _position;
        }
    }
}