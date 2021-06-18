using CCE.Core;
using UnityEngine;

namespace CCE.UI
{
    [RequireComponent(typeof(ClassInfoDisplay))]
    public class EditorConfigDisplay : MonoBehaviour
    {
        private void Awake()
        {
            gameObject.GetComponent<ClassInfoDisplay>().DrawGui(GlobalState.Config);
        }
    }
}
