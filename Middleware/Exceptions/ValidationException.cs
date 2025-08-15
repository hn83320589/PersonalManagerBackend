namespace PersonalManagerAPI.Middleware.Exceptions
{
    /// <summary>
    /// 資料驗證例外，用於處理輸入資料驗證失敗的情況
    /// </summary>
    public class ValidationException : Exception
    {
        public Dictionary<string, string[]> ValidationErrors { get; }

        public ValidationException(string message) : base(message)
        {
            ValidationErrors = new Dictionary<string, string[]>();
        }

        public ValidationException(string message, Dictionary<string, string[]> validationErrors) : base(message)
        {
            ValidationErrors = validationErrors;
        }

        public ValidationException(string fieldName, string error) : base($"Validation failed for {fieldName}")
        {
            ValidationErrors = new Dictionary<string, string[]>
            {
                { fieldName, new[] { error } }
            };
        }

        public ValidationException(string message, Exception innerException) : base(message, innerException)
        {
            ValidationErrors = new Dictionary<string, string[]>();
        }
    }
}