using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CCE.UI
{
    public class ClassFieldDisplay : MonoBehaviour
    {
        public TextMeshProUGUI FieldName;
        public Toggle BooleanToggle; // To be used for boolean types
        public Slider ValueSlider; // To be used for floats and integers
        public TMP_InputField ValueInputField; // To be used for strings, floats and integers
    }
}