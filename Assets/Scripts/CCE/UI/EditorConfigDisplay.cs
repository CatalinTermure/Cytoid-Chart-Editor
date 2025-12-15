using CCE.Core;
using CCE.Data;
using CCE.GameUtils;
using UnityEngine;

namespace CCE.UI
{
    [RequireComponent(typeof(ClassInfoDisplay))]
    public class EditorConfigDisplay : MonoBehaviour
    {
        private void Awake()
        {
            gameObject.GetComponent<ClassInfoDisplay>().DrawGui(GlobalState.Config, 0);
        }

        public void SaveConfig()
        {
            EditorConfigProvider.SaveConfig();
        }
    }
}