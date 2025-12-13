using System.Collections.Generic;

namespace CCE.Utils
{
    public interface IValidator
    {
        List<ValidationResult> Validate(object value);
    }
}