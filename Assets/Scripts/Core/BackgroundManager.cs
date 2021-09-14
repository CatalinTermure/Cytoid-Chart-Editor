using UnityEngine;
using UnityEngine.UI;

namespace CCE.Core
{
    /// <summary>
    ///     Utility class for changing the background.
    ///     Should be attached to the <see cref="GameObject" /> which displays the background.
    /// </summary>
    public class BackgroundManager : MonoBehaviour
    {
        [Range(0f, 1f)]
        public float Brightness;

        public static Sprite BackgroundOverride = null;

        public Sprite DefaultBackground;

        private float _canvasAspectRatio;
        private float _canvasHeight;
        private float _canvasWidth;

        private void Start()
        {
            Rect canvasRect = gameObject.GetComponent<Image>()
                .canvas.gameObject.GetComponent<RectTransform>().rect;

            _canvasHeight = canvasRect.height;
            _canvasWidth = canvasRect.width;
            _canvasAspectRatio = _canvasWidth / _canvasHeight;

            ChangeBackground(BackgroundOverride == null ? DefaultBackground : BackgroundOverride);
        }

        public void ChangeBackground(Sprite sprite)
        {
            GameObject obj = gameObject;
            var backgroundImage = obj.GetComponent<Image>();
            backgroundImage.sprite = sprite;

            backgroundImage.color = new Color(Brightness, Brightness, Brightness);

            backgroundImage.preserveAspect = true;

            // Crop image to fit into the screen's aspect ratio without stretching
            // or letter boxing

            float imageAspectRatio =
                (float)sprite.texture.width / sprite.texture.height;

            if (imageAspectRatio > _canvasAspectRatio)
            // if the image is longer in width than the canvas
            {
                // then fill the height and spill on the sides
                ((RectTransform)obj.transform).sizeDelta =
                    new Vector2(_canvasHeight * imageAspectRatio, _canvasHeight);
            }
            else
            // if the image is taller in height than the canvas
            {
                // then fill the width and spill on the top and bottom
                ((RectTransform)obj.transform).sizeDelta =
                    new Vector2(_canvasWidth, _canvasWidth / imageAspectRatio);
            }
        }
    }
}