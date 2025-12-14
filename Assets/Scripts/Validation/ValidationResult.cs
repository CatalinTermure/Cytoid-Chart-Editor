namespace CCE.Validation
{
    public record ValidationResult
    {
        public ValidationSeverity Severity { get; private set; }

        public string Message { get; private set; }

        public static ValidationResult Warning(string message)
        {
            return new ValidationResult(ValidationSeverity.Warning, message);
        }

        public static ValidationResult Error(string message)
        {
            return new ValidationResult(ValidationSeverity.Error, message);
        }

        private ValidationResult(ValidationSeverity severity, string message)
        {
            Severity = severity;
            Message = message;
        }
    }
}