using System;
using System.Collections.Generic;
using CCE.Utils;

namespace CCE.Data.Validators
{
    public class SourceValidator : IValidator
    {
        public List<ValidationResult> Validate(object value)
        {
            if (value is not string source)
            {
                throw new ArgumentException("Argument must be a string");
            }

            var results = new List<ValidationResult>();

            if (!source.StartsWith("http://") && !source.StartsWith("https://"))
            {
                results.Add(ValidationResult.Error("Source must be a link"));
            }

            return results;
        }
    }
}