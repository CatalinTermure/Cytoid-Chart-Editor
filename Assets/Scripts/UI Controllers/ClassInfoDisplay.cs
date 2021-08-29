using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CCE.Core;
using CCE.Data;
using CCE.LevelLoading;
using UnityEngine;

namespace CCE.UI
{
    public class ClassInfoDisplay : MonoBehaviour
    {
        [SerializeField] private GameObject IntegerDisplayTemplate;
        [SerializeField] private GameObject StringDisplayTemplate;
        [SerializeField] private GameObject FloatDisplayTemplate;
        [SerializeField] private GameObject BooleanDisplayTemplate;
        [SerializeField] private GameObject BackgroundDisplayTemplate;
        [SerializeField] private GameObject SectionHeaderDisplayTemplate;

        [SerializeField] private RectTransform FillTarget;
        [SerializeField] private bool ShouldStretchFillTarget;
        [SerializeField] private float ElementSpacing;
        [SerializeField] private float ElementLeftMargin;

        private readonly Type[] _possibleTypes =
        {
            typeof(int), typeof(float), typeof(bool), typeof(string), typeof(LevelData.BackgroundData)
        };

        private float _currentElementTopMargin;

        private object _targetObject;

        // The class type restriction is so that value types don't accidentally get passed to this.
        public void DrawGui<TTarget>(TTarget targetObject, int offset, string filter = "") where TTarget : class
        {
            if(offset == 0)
            {
                foreach (Transform child in FillTarget)
                {
                    Destroy(child.gameObject);
                }
            }

            _targetObject = targetObject;
            _currentElementTopMargin = -offset + ElementSpacing; // to compensate for the first section header

            IEnumerable<FieldInfo> fieldsToDisplay = typeof(TTarget)
                .GetFields()
                .Where(x =>
                {
                    if (!Attribute.IsDefined(x, typeof(DisplayableAttribute))) return false;
                    
                    return String.IsNullOrEmpty(filter) || GetAttributeInfo(x).Filter == filter;
                });

            IOrderedEnumerable<IGrouping<string, FieldInfo>> sections = fieldsToDisplay
                .GroupBy(fieldInfo => GetAttributeInfo(fieldInfo).Section)
                .OrderBy(grouping => grouping.Key);

            foreach (IGrouping<string, FieldInfo> section in sections)
            {
                DrawSection(section.Key, section);
            }

            if (ShouldStretchFillTarget)
            {
                FillTarget.sizeDelta =
                    new Vector2(FillTarget.sizeDelta.x, -_currentElementTopMargin + ElementSpacing * 2);
            }
        }

        private void DrawSection(string title, IEnumerable<FieldInfo> fields)
        {
            if (!String.IsNullOrEmpty(title))
            {
                DrawSectionHeader(title);
            }

            foreach (FieldInfo field in fields)
            {
                DrawField(field);
            }
        }

        private void DrawSectionHeader(string title)
        {
            _currentElementTopMargin -= ElementSpacing * 1.5f;
            GameObject obj = Instantiate(SectionHeaderDisplayTemplate, FillTarget);
            obj.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(ElementLeftMargin * 0.5f, _currentElementTopMargin);
            obj.GetComponent<ClassFieldDisplay>().FieldName.text = title;
        }

        private void DrawField(FieldInfo fieldInfo)
        {
            if (!_possibleTypes.Contains(fieldInfo.FieldType))
            {
                Logging.LogError($"{nameof(ClassInfoDisplay)}.{nameof(DrawField)}",
                    $"Field {fieldInfo.Name} is of a type that is " +
                    $"not supported by the {nameof(DisplayableAttribute)} attribute. " +
                    "Add support for it or remove the attribute.");

                return;
            }

            _currentElementTopMargin -= ElementSpacing;
            if (fieldInfo.FieldType == typeof(int))
            {
                DrawIntegerField(fieldInfo);
            }
            else if (fieldInfo.FieldType == typeof(float))
            {
                DrawFloatField(fieldInfo);
            }
            else if (fieldInfo.FieldType == typeof(bool))
            {
                DrawBooleanField(fieldInfo);
            }
            else if (fieldInfo.FieldType == typeof(string))
            {
                DrawStringField(fieldInfo);
            }
            else if (fieldInfo.FieldType == typeof(LevelData.BackgroundData))
            {
                DrawBackgroundField(fieldInfo);
            }
        }

        private static DisplayableAttribute GetAttributeInfo(FieldInfo fieldInfo)
        {
            return (DisplayableAttribute)fieldInfo.GetCustomAttribute(typeof(DisplayableAttribute));
        }

        private void DrawBackgroundField(FieldInfo fieldInfo)
        {
            GameObject obj = Instantiate(BackgroundDisplayTemplate, FillTarget);
            obj.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(ElementLeftMargin, _currentElementTopMargin);

            obj.GetComponent<ClassFieldDisplay>().FieldName.text = GetAttributeInfo(fieldInfo).Name ?? fieldInfo.Name;

            obj.GetComponent<ImagePicker>().OnImagePicked += path =>
            {
                if ((LevelData.BackgroundData)fieldInfo.GetValue(_targetObject) == null)
                {
                    fieldInfo.SetValue(_targetObject, new LevelData.BackgroundData
                    {
                        Path = path
                    });
                }
                else
                {
                    ((LevelData.BackgroundData)fieldInfo.GetValue(_targetObject)).Path = path;
                }
            };

            _currentElementTopMargin -= ElementSpacing * 1.5f;
        }

        private void DrawIntegerField(FieldInfo fieldInfo)
        {
            GameObject obj = Instantiate(IntegerDisplayTemplate, FillTarget);
            obj.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(ElementLeftMargin, _currentElementTopMargin);

            var classFieldDisplay = obj.GetComponent<ClassFieldDisplay>();
            DisplayableAttribute attributeInfo = GetAttributeInfo(fieldInfo);

            classFieldDisplay.FieldName.text = attributeInfo.Name ?? fieldInfo.Name;

            classFieldDisplay.ValueSlider.minValue = attributeInfo.MinValue;
            classFieldDisplay.ValueSlider.maxValue = attributeInfo.MaxValue;
            classFieldDisplay.ValueSlider.value = (int)fieldInfo.GetValue(_targetObject);
            classFieldDisplay.ValueSlider.onValueChanged
                .AddListener(value =>
                {
                    fieldInfo.SetValue(_targetObject, Mathf.RoundToInt(value));
                    classFieldDisplay.ValueInputField.text = Mathf.RoundToInt(value).ToString();
                });

            classFieldDisplay.ValueInputField.text = ((int)fieldInfo.GetValue(_targetObject)).ToString();

            classFieldDisplay.ValueInputField.onEndEdit
                .AddListener(stringValue =>
                {
                    int value = Int32.Parse(stringValue);
                    value = Mathf.RoundToInt(Mathf.Clamp(value, attributeInfo.MinValue, attributeInfo.MaxValue));

                    classFieldDisplay.ValueInputField.text = value.ToString();
                    fieldInfo.SetValue(_targetObject, value);
                    classFieldDisplay.ValueSlider.SetValueWithoutNotify(value);
                });
        }

        private void DrawBooleanField(FieldInfo fieldInfo)
        {
            GameObject obj = Instantiate(BooleanDisplayTemplate, FillTarget);
            obj.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(ElementLeftMargin, _currentElementTopMargin);

            var classFieldDisplay = obj.GetComponent<ClassFieldDisplay>();
            DisplayableAttribute attributeInfo = GetAttributeInfo(fieldInfo);

            classFieldDisplay.FieldName.text = attributeInfo.Name ?? fieldInfo.Name;

            classFieldDisplay.BooleanToggle.isOn = (bool)fieldInfo.GetValue(_targetObject);
            classFieldDisplay.BooleanToggle.onValueChanged
                .AddListener(isOn => fieldInfo.SetValue(_targetObject, isOn));
        }

        private void DrawFloatField(FieldInfo fieldInfo)
        {
            GameObject obj = Instantiate(FloatDisplayTemplate, FillTarget);
            obj.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(ElementLeftMargin, _currentElementTopMargin);

            var classFieldDisplay = obj.GetComponent<ClassFieldDisplay>();
            DisplayableAttribute attributeInfo = GetAttributeInfo(fieldInfo);

            classFieldDisplay.FieldName.text = attributeInfo.Name ?? fieldInfo.Name;

            classFieldDisplay.ValueSlider.minValue = attributeInfo.MinValue;
            classFieldDisplay.ValueSlider.maxValue = attributeInfo.MaxValue;
            classFieldDisplay.ValueSlider.value = (float)fieldInfo.GetValue(_targetObject);
            classFieldDisplay.ValueSlider.onValueChanged
                .AddListener(value =>
                {
                    fieldInfo.SetValue(_targetObject, value);
                    classFieldDisplay.ValueInputField.text = value.ToString("F2");
                });

            classFieldDisplay.ValueInputField.text = ((float)fieldInfo.GetValue(_targetObject)).ToString("F2");

            classFieldDisplay.ValueInputField.onEndEdit
                .AddListener(stringValue =>
                {
                    float value = Single.Parse(stringValue);
                    value = Mathf.Clamp(value, attributeInfo.MinValue, attributeInfo.MaxValue);

                    classFieldDisplay.ValueInputField.text = value.ToString("F2");
                    fieldInfo.SetValue(_targetObject, value);
                    classFieldDisplay.ValueSlider.SetValueWithoutNotify(value);
                });
        }

        private void DrawStringField(FieldInfo fieldInfo)
        {
            GameObject obj = Instantiate(StringDisplayTemplate, FillTarget);
            obj.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(ElementLeftMargin, _currentElementTopMargin);

            var classFieldDisplay = obj.GetComponent<ClassFieldDisplay>();

            classFieldDisplay.FieldName.text = GetAttributeInfo(fieldInfo).Name ?? fieldInfo.Name;

            classFieldDisplay.ValueInputField.text = (string)fieldInfo.GetValue(_targetObject);
            classFieldDisplay.ValueInputField.onEndEdit
                .AddListener(stringValue => fieldInfo.SetValue(_targetObject, stringValue));
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DisplayableAttribute : Attribute
    {
        public string Filter;
        public float MaxValue = 1;

        public float MinValue = 0;
        public string Name;
        public string Section;
    }
}