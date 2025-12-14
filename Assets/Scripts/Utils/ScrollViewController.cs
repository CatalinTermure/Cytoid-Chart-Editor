using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace CCE.Utils
{
    /// <summary>
    /// A controller for a scroll view that is populated at runtime. Makes sure the canvas
    /// is appropriately sized for the contents of the scroll view.
    /// </summary>
    [RequireComponent(typeof(ScrollView))]
    public class ScrollViewController : MonoBehaviour
    {
        [SerializeField]
        private RectTransform _scrollViewContent;
        [SerializeField]
        private float _startingHeight;

        public void AddGameObject(GameObject gameObject, float topMargin = 0)
        {
            if (gameObject.transform is not RectTransform rectTransform)
            {
                throw new ArgumentException("GameObject must have a RectTransform component.");
            }

            if (rectTransform.anchorMin != new Vector2(0, 1)
                || rectTransform.anchorMax != new Vector2(1, 1))
            {
                throw new ArgumentException("RectTransform of GameObject must stretch across the scroll view.");
            }

            rectTransform.SetParent(_scrollViewContent, false);
            rectTransform.localScale = Vector2.one;
            rectTransform.anchoredPosition = new Vector2(0, -CurrentHeight - topMargin);
            rectTransform.sizeDelta = new Vector2(0, rectTransform.sizeDelta.y);
            CurrentHeight += rectTransform.sizeDelta.y + topMargin;
        }

        private float _currentHeightValue = 0;
        private float CurrentHeight
        {
            get => _currentHeightValue;
            set
            {
                _currentHeightValue = value;
                _scrollViewContent.sizeDelta = new Vector2(_scrollViewContent.sizeDelta.x, value);
            }
        }

        public void Awake()
        {
            CurrentHeight = _startingHeight;
        }

        public void OnValidate()
        {
            if (_scrollViewContent == null)
            {
                Debug.LogError("ScrollViewContent is not assigned", this);
            }
        }
    }
}