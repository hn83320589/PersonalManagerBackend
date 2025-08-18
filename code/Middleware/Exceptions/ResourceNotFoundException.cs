namespace PersonalManagerAPI.Middleware.Exceptions
{
    /// <summary>
    /// 資源未找到例外，用於處理資源不存在的情況
    /// </summary>
    public class ResourceNotFoundException : Exception
    {
        public string ResourceType { get; }
        public object? ResourceId { get; }

        public ResourceNotFoundException(string resourceType, object resourceId) 
            : base($"{resourceType} with ID '{resourceId}' was not found")
        {
            ResourceType = resourceType;
            ResourceId = resourceId;
        }

        public ResourceNotFoundException(string resourceType, object resourceId, string message) 
            : base(message)
        {
            ResourceType = resourceType;
            ResourceId = resourceId;
        }

        public ResourceNotFoundException(string message) : base(message)
        {
            ResourceType = "Unknown";
        }

        public ResourceNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
            ResourceType = "Unknown";
        }
    }
}