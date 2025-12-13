using System;
using System.Collections.Generic;
using System.Linq;
using CCE.Utils;

namespace CCE.Data.Validators
{
    public class LevelIdValidator : IValidator
    {
        public List<ValidationResult> Validate(object value)
        {
            if (value is not string levelId)
            {
                throw new ArgumentException("Argument must be a string");
            }

            var results = new List<ValidationResult>();

            if (!levelId.All(c => char.IsLower(c) || char.IsDigit(c) || c == '.' || c == '_'))
            {
                results.Add(ValidationResult.Error("Level ID must contain only lowercase characters, digits, dots and underscores"));
            }

            string[] parts = levelId.Split('.');
            if (parts.Length != 2)
            {
                results.Add(ValidationResult.Warning("Level ID should be charter_name.level_name"));
            }

            return results;
        }
    }
}