using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using CCE.Validation;

namespace CCE.Utils
{
    /// <summary>
    /// A controller for the validation errors popup. This allows creating a popup where the user
    /// can see a list of validation results.
    /// </summary>
    public class ValidationErrorsPopupController : MonoBehaviour
    {
        [SerializeField]
        private ScrollViewController _scrollViewController;

        [SerializeField]
        private GameObject _warningPrefab;

        [SerializeField]
        private GameObject _errorPrefab;

        public void PopulateValidationResults(List<ValidationResult> validationResults)
        {
            foreach (var validationResult in validationResults)
            {
                var prefab = validationResult.Severity switch
                {
                    ValidationSeverity.Warning => _warningPrefab,
                    ValidationSeverity.Error => _errorPrefab,
                    _ => throw new ArgumentOutOfRangeException()
                };
                var resultObject = Instantiate(prefab);
                resultObject.GetComponentInChildren<TMP_Text>().text = validationResult.Message;
                _scrollViewController.AddGameObject(resultObject, 10);
            }
        }

        public void ClosePopup()
        {
            Destroy(gameObject);
        }

        public void OnValidate()
        {
            if (_scrollViewController == null)
            {
                Debug.LogError("ScrollViewController is not assigned", this);
            }

            if (_warningPrefab == null)
            {
                Debug.LogError("Warning prefab is not assigned", this);
            }

            if (_errorPrefab == null)
            {
                Debug.LogError("Error prefab is not assigned", this);
            }
        }
    }
}