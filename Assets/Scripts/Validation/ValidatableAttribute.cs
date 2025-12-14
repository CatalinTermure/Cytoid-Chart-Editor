using System;

namespace CCE.Validation
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ValidatableAttribute : Attribute
    {
        public Type Validator;
    }
}