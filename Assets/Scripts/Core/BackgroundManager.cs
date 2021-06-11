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

        private void Start()
        {
            gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(1920, 1920);
            ChangeBackground(BackgroundOverride == null ? DefaultBackground : BackgroundOverride);
        }

        private void ChangeBackground(Sprite sprite)
        {
            GameObject obj = gameObject;
            var backgroundImage = obj.GetComponent<Image>();
            backgroundImage.sprite = sprite;

            backgroundImage.color =
                new Color(Brightness, Brightness, Brightness);

            backgroundImage.preserveAspect = true;

            // Crop image to fit into the screen's aspect ratio without stretching
            // or letter boxing

            float imageAspectRatio =
                (float) backgroundImage.sprite.texture.width
                / backgroundImage.sprite.texture.height;

            float screenAspectRatio = (float) Screen.width / Screen.height;

            ((RectTransform) obj.transform).sizeDelta =
                imageAspectRatio < screenAspectRatio
                    ? new Vector2(Screen.width, Screen.width)
                    : new Vector2(Screen.height * imageAspectRatio, Screen.height);
        }
    }
}