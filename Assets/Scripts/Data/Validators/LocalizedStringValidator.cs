using System;
using System.Collections.Generic;
using System.Linq;
using CCE.Validation;

namespace CCE.Data.Validators
{
    public class LocalizedStringValidator : IValidator
    {
        public List<ValidationResult> Validate(object value)
        {
            if (value is not string localizedTitle)
            {
                throw new ArgumentException("Argument must be a string");
            }

            var results = new List<ValidationResult>();

            string allowedLetters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

            if (!localizedTitle.All(c => allowedLetters.Contains(c) || Char.IsNumber(c) || Char.IsSymbol(c) || Char.IsPunctuation(c) || c == ' '))
            {
                results.Add(ValidationResult.Error("Localized strings must only contain English letters, numbers, symbols, and spaces."));
            }

            return results;
        }
    }
}