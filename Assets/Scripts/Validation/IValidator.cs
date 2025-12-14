using System.Collections.Generic;

namespace CCE.Validation
{
    public interface IValidator
    {
        List<ValidationResult> Validate(object value);
    }
}