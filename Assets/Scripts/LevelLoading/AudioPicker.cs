using System.Collections;
using System.IO;
using SFB;
using UnityEngine;

namespace CCE.LevelLoading
{
    public class AudioPicker : MonoBehaviour
    {
        [SerializeField] private LevelListBehaviour LevelListBehaviour;
        
        private bool _isRunning;
        private string _finalPath;
        
        private void PickAudioMobile()
        {
            NativeGallery.GetAudioFromGallery(path =>
            {
                _isRunning = false;
                _finalPath = path;
            });
        }

        private void PickAudioDesktop()
        {
            var extensions = new[] { new ExtensionFilter("Audio Files", "ogg", "mp3", "wav") };
            
            StandaloneFileBrowser.OpenFilePanelAsync("Choose an audio file", "", extensions, false, paths =>
            {
                if (paths.Length == 0) return;
                _isRunning = false;
                _finalPath = paths[0];
            });
        }
        
        public void PickAudio()
        {
            if (Application.isMobilePlatform)
            {
                PickAudioMobile();
            }
            else
            {
                PickAudioDesktop();
            }

            StartCoroutine(WaitForAudioPickedCoroutine());
        }

        private IEnumerator WaitForAudioPickedCoroutine()
        {
            while (_isRunning)
            {
                yield return null;
            }

            if (!File.Exists(_finalPath)) yield break;
            
            LevelListBehaviour.ShowLevelMetadataPopup(_finalPath);
        }
    }
}