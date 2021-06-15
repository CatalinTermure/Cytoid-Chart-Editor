using UnityEngine;
using UnityEngine.UI;

namespace CCE.MusicLoading
{
    public class MusicInfoController : MonoBehaviour
    {
        [SerializeField] private Text MusicTitleText;

        public void SetTitle(string title)
        {
            MusicTitleText.text = $"<b>{title}</b>";
        }
    }
}