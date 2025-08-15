namespace PersonalManagerAPI.Middleware.Exceptions
{
    /// <summary>
    /// 業務邏輯例外，用於處理商業規則違反的情況
    /// </summary>
    public class BusinessLogicException : Exception
    {
        public string? ErrorCode { get; }
        public object? Details { get; }

        public BusinessLogicException(string message) : base(message)
        {
        }

        public BusinessLogicException(string message, string errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }

        public BusinessLogicException(string message, string errorCode, object details) : base(message)
        {
            ErrorCode = errorCode;
            Details = details;
        }

        public BusinessLogicException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}