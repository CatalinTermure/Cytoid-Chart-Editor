using System;

namespace CCE.Utils
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ValidatableAttribute : Attribute
    {
        public Type Validator;
    }
}