using System.Collections;
using System.IO;
using CCE.Data;
using SFB;
using UnityEngine;
using UnityEngine.UI;

namespace CCE.LevelLoading
{
    public class ImagePicker : MonoBehaviour
    {
        public Image ImagePreview;

        private bool _isRunning;
        private string _finalPath;
        
        public delegate void OnImagePickedEvent(string path);
        public OnImagePickedEvent OnImagePicked;

        private void PickImageMobile()
        {
            NativeGallery.GetImageFromGallery(path =>
            {
                _isRunning = false;
                _finalPath = path;
            });
        }

        private void PickImageDesktop()
        {
            var extensions = new[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") };
            
            StandaloneFileBrowser.OpenFilePanelAsync("Choose a background", "", extensions, false, paths =>
            {
                _isRunning = false;
                _finalPath = paths[0];
            });
        }
        
        public void PickImage()
        {
            _isRunning = true;
            if (Application.isMobilePlatform)
            {
                PickImageMobile();
            }
            else
            {
                PickImageDesktop();
            }
            StartCoroutine(nameof(PickImageCoroutine));
        }

        private IEnumerator PickImageCoroutine()
        {
            while (_isRunning)
            {
                yield return null;
            }

            ImagePreview.sprite = LoadSpriteFromPath(_finalPath);
            OnImagePicked(_finalPath);
        }
        
        private static Sprite LoadSpriteFromPath(string path)
        {
            var tex = new Texture2D(1, 1);
            tex.LoadImage(File.ReadAllBytes(path));
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        }
     }
}