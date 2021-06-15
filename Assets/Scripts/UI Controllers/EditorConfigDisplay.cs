using CCE.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CCE.UI
{
    public class EditorConfigDisplay : MonoBehaviour
    {
        [SerializeField] Slider sliderTemplate;
        [SerializeField] TextMeshProUGUI headerTemplate;
        [SerializeField] Toggle toggleTemplate;

        [SerializeField] Transform fillTarget; //The transform we're going to spawn templates into.

        EditorConfig configInstance => GlobalState.Config;

        Type[] possibleTypes =
        {
            typeof(bool), typeof(int), typeof(float)
        };

        void OnEnable()
        {
            var fieldsToDisplay = typeof(EditorConfig)
                .GetFields()
                .Where(x => Attribute.IsDefined(x,
                typeof(DrawEditorSettingsAttribute)));

            foreach (var field in fieldsToDisplay)
            {
                if (possibleTypes.Contains(field.FieldType))
                    DrawField(field);
                else
                    Debug.LogError($"{field.Name} is using an attribute " +
                    $"that is not supported by the editor config display. " +
                    $"Add support for it or remove the attribute.");
            }
        }

        void DrawField(FieldInfo field)
        {
            var attribute = (DrawEditorSettingsAttribute) Attribute.GetCustomAttribute(field, typeof(DrawEditorSettingsAttribute));
            if (field.FieldType == typeof(bool))
            {
                Toggle toggle = GameObject.Instantiate(toggleTemplate, fillTarget);
                toggle.SetIsOnWithoutNotify((bool)field.GetValue(configInstance));
                toggle.onValueChanged
                    .AddListener(value => field.SetValue(configInstance, value));
            }
            else if(field.FieldType == typeof(float) || field.FieldType == typeof(int))
            {
                Slider slider = GameObject.Instantiate(sliderTemplate, fillTarget);
                slider.SetValueWithoutNotify((float)field.GetValue(configInstance));
                slider.onValueChanged
                    .AddListener(value => field.SetValue(configInstance, value));
                slider.wholeNumbers = field.FieldType == typeof(int);
                slider.maxValue = attribute.MaxValue;
                slider.minValue = attribute.MinValue;
            }
        }
    }
}
