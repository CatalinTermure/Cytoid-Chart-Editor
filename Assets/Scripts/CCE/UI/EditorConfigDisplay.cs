using CCE.Data;
using CCE.GameUtils;
using Newtonsoft.Json;
using UnityEngine;

namespace CCE.UI
{
    [RequireComponent(typeof(ClassInfoDisplay))]
    public class EditorConfigDisplay : MonoBehaviour
    {
        private EditorConfig _configCopy;

        private void Awake()
        {
            _configCopy = JsonConvert.DeserializeObject<EditorConfig>(JsonConvert.SerializeObject(EditorConfigProvider.Config));
            gameObject.GetComponent<ClassInfoDisplay>().DrawGui(_configCopy, 0);
        }

        public void SaveConfig()
        {
            EditorConfigProvider.Config = _configCopy;
            EditorConfigProvider.SaveConfig();
        }
    }
}