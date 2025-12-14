using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CCE.Utils;
using CCE.Data;
using UnityEngine;
using UnityEngine.UI;

namespace CCE.GameUtils
{
    public class ClassInfoDisplay : MonoBehaviour
    {
        [SerializeField] private GameObject IntegerDisplayTemplate;
        [SerializeField] private GameObject StringDisplayTemplate;
        [SerializeField] private GameObject FloatDisplayTemplate;
        [SerializeField] private GameObject BooleanDisplayTemplate;
        [SerializeField] private GameObject SectionHeaderDisplayTemplate;

        [SerializeField] private RectTransform FillTarget;
        [SerializeField] private bool ShouldStretchFillTarget;
        [SerializeField] private float ElementSpacing;
        [SerializeField] private float ElementLeftMargin;

        [SerializeField] private GameObject _validationErrorPopupPrefab;

        private readonly Dictionary<Type, IClassFieldRenderer> _classFieldRenderers = new();

        private readonly Type[] _defaultTypes =
        {
            typeof(int), typeof(float), typeof(bool), typeof(string)
        };

        private float _currentElementTopMargin;

        private object _targetObject;

        private GameObject _popupParentCanvas;


        private void InitializeClassFieldRenderers()
        {
            if (_classFieldRenderers.Count != 0) return;
            foreach (var classFieldRenderer in GetComponents<IClassFieldRenderer>())
            {
                _classFieldRenderers.Add(classFieldRenderer.FieldType, classFieldRenderer);
            }
        }

        // The class type restriction is so that value types don't accidentally get passed to this.
        public void DrawGui<TTarget>(TTarget targetObject, int offset, string filter = "") where TTarget : class
        {
            InitializeClassFieldRenderers();

            if (offset == 0)
            {
                foreach (Transform child in FillTarget)
                {
                    Destroy(child.gameObject);
                }
            }

            _targetObject = targetObject;
            _currentElementTopMargin = -offset + ElementSpacing; // to compensate for the first section header

            var fieldsToDisplay = typeof(TTarget)
                .GetFields()
                .Where(x =>
                {
                    if (!Attribute.IsDefined(x, typeof(DisplayableAttribute))) return false;

                    return string.IsNullOrEmpty(filter) || GetAttributeInfo(x).Filter == filter;
                });

            var sections = fieldsToDisplay
                .GroupBy(fieldInfo => GetAttributeInfo(fieldInfo).Section)
                .OrderBy(grouping => grouping.Key);

            foreach (var section in sections)
            {
                DrawSection(section.Key, section);
            }

            if (ShouldStretchFillTarget)
            {
                FillTarget.sizeDelta =
                    new Vector2(FillTarget.sizeDelta.x, -_currentElementTopMargin + ElementSpacing * 2);
            }
        }

        private void SetValidationResults(List<ValidationResult> results, ClassFieldDisplay classFieldDisplay)
        {
            if (results.Count == 0)
            {
                classFieldDisplay.ValueInputField.textComponent.color = Color.black;
                classFieldDisplay.validationResult.gameObject.SetActive(false);
                return;
            }

            classFieldDisplay.validationResult.gameObject.SetActive(true);

            if (results.Any(result => result.Severity == ValidationSeverity.Error))
            {
                classFieldDisplay.ValueInputField.textComponent.color = Color.red;
            }
            else if (results.Any(result => result.Severity == ValidationSeverity.Warning))
            {
                classFieldDisplay.ValueInputField.textComponent.color = Color.yellowNice;
            }

            var resultsPopupButton =
                classFieldDisplay.validationResult.gameObject.GetComponent<Button>();
            resultsPopupButton.onClick.RemoveAllListeners();
            resultsPopupButton.onClick.AddListener(() =>
            {
                var popup = Instantiate(_validationErrorPopupPrefab, _popupParentCanvas.transform);
                popup.GetComponent<ValidationErrorsPopupController>().PopulateValidationResults(results);
            });
        }

        private void DrawSection(string title, IEnumerable<FieldInfo> fields)
        {
            if (!string.IsNullOrEmpty(title))
            {
                DrawSectionHeader(title);
            }

            foreach (var field in fields)
            {
                DrawField(field);
            }
        }

        private void DrawSectionHeader(string title)
        {
            _currentElementTopMargin -= ElementSpacing * 1.5f;
            var obj = Instantiate(SectionHeaderDisplayTemplate, FillTarget);
            obj.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(ElementLeftMargin * 0.5f, _currentElementTopMargin);
            obj.GetComponent<ClassFieldDisplay>().FieldName.text = title;
        }

        private void DrawField(FieldInfo fieldInfo)
        {
            if (_classFieldRenderers.TryGetValue(fieldInfo.FieldType, out var fieldRenderer))
            {
                _currentElementTopMargin -= fieldRenderer
                    .RenderField(new ClassFieldRenderInfo
                    {
                        FieldInfo = fieldInfo,
                        TargetObject = _targetObject,
                        CurrentElementTopMargin = _currentElementTopMargin,
                        ElementLeftMargin = ElementLeftMargin,
                        ElementSpacing = ElementSpacing,
                        FillTarget = FillTarget
                    });
                return;
            }

            if (!_defaultTypes.Contains(fieldInfo.FieldType))
            {
                throw new ArgumentException($"{nameof(ClassInfoDisplay)}.{nameof(DrawField)}",
                    $"Field {fieldInfo.Name} is of a type that is " +
                    $"not supported by the {nameof(DisplayableAttribute)} attribute. " +
                    "Add support for it or remove the attribute.");
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
            else
            {
                throw new ArgumentException($"{nameof(ClassInfoDisplay)}.{nameof(DrawField)}",
                    $"Field {fieldInfo.Name} of type {fieldInfo.FieldType} could not be drawn.");
            }
        }

        private static DisplayableAttribute GetAttributeInfo(FieldInfo fieldInfo)
        {
            return (DisplayableAttribute)fieldInfo.GetCustomAttribute(typeof(DisplayableAttribute));
        }

        private void DrawIntegerField(FieldInfo fieldInfo)
        {
            var obj = Instantiate(IntegerDisplayTemplate, FillTarget);
            obj.name = fieldInfo.Name + "Field";
            obj.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(ElementLeftMargin, _currentElementTopMargin);

            var classFieldDisplay = obj.GetComponent<ClassFieldDisplay>();
            var attributeInfo = GetAttributeInfo(fieldInfo);

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
                    var value = int.Parse(stringValue);
                    value = Mathf.RoundToInt(Mathf.Clamp(value, attributeInfo.MinValue, attributeInfo.MaxValue));

                    classFieldDisplay.ValueInputField.text = value.ToString();
                    fieldInfo.SetValue(_targetObject, value);
                    classFieldDisplay.ValueSlider.SetValueWithoutNotify(value);
                });
        }

        private void DrawBooleanField(FieldInfo fieldInfo)
        {
            var obj = Instantiate(BooleanDisplayTemplate, FillTarget);
            obj.name = fieldInfo.Name + "Field";
            obj.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(ElementLeftMargin, _currentElementTopMargin);

            var classFieldDisplay = obj.GetComponent<ClassFieldDisplay>();
            var attributeInfo = GetAttributeInfo(fieldInfo);

            classFieldDisplay.FieldName.text = attributeInfo.Name ?? fieldInfo.Name;

            classFieldDisplay.BooleanToggle.isOn = (bool)fieldInfo.GetValue(_targetObject);
            classFieldDisplay.BooleanToggle.onValueChanged
                .AddListener(isOn => fieldInfo.SetValue(_targetObject, isOn));
        }

        private void DrawFloatField(FieldInfo fieldInfo)
        {
            var obj = Instantiate(FloatDisplayTemplate, FillTarget);
            obj.name = fieldInfo.Name + "Field";
            obj.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(ElementLeftMargin, _currentElementTopMargin);

            var classFieldDisplay = obj.GetComponent<ClassFieldDisplay>();
            var attributeInfo = GetAttributeInfo(fieldInfo);

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
                    var value = float.Parse(stringValue);
                    value = Mathf.Clamp(value, attributeInfo.MinValue, attributeInfo.MaxValue);

                    classFieldDisplay.ValueInputField.text = value.ToString("F2");
                    fieldInfo.SetValue(_targetObject, value);
                    classFieldDisplay.ValueSlider.SetValueWithoutNotify(value);
                });
        }

        private void DrawStringField(FieldInfo fieldInfo)
        {
            var obj = Instantiate(StringDisplayTemplate, FillTarget);
            obj.name = fieldInfo.Name + "Field";
            obj.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(ElementLeftMargin, _currentElementTopMargin);

            var classFieldDisplay = obj.GetComponent<ClassFieldDisplay>();

            classFieldDisplay.FieldName.text = GetAttributeInfo(fieldInfo).Name ?? fieldInfo.Name;

            classFieldDisplay.ValueInputField.text = (string)fieldInfo.GetValue(_targetObject);
            classFieldDisplay.ValueInputField.onEndEdit
                .AddListener(stringValue => fieldInfo.SetValue(_targetObject, stringValue));

            var validatableAttribute = fieldInfo.GetCustomAttribute<ValidatableAttribute>();
            if (validatableAttribute == null) return;
            var validator = (IValidator)Activator.CreateInstance(validatableAttribute.Validator);
            SetValidationResults(validator.Validate(classFieldDisplay.ValueInputField.text), classFieldDisplay);
            classFieldDisplay.ValueInputField.onEndEdit.AddListener(value => SetValidationResults(validator.Validate(value), classFieldDisplay));
        }

        public void Awake()
        {
            _popupParentCanvas = GetComponentInParent<Canvas>().gameObject;
        }
    }
}