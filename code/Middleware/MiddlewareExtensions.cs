namespace PersonalManagerAPI.Middleware
{
    /// <summary>
    /// 中介軟體擴展方法
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// 註冊錯誤處理中介軟體
        /// </summary>
        /// <param name="builder">應用程式建構器</param>
        /// <returns>應用程式建構器</returns>
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }

        /// <summary>
        /// 註冊請求日誌中介軟體
        /// </summary>
        /// <param name="builder">應用程式建構器</param>
        /// <returns>應用程式建構器</returns>
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }

        /// <summary>
        /// 註冊 API 限流中介軟體
        /// </summary>
        /// <param name="builder">應用程式建構器</param>
        /// <returns>應用程式建構器</returns>
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SimpleRateLimitingMiddleware>();
        }

        /// <summary>
        /// 註冊 RBAC 權限檢查中介軟體
        /// </summary>
        /// <param name="builder">應用程式建構器</param>
        /// <returns>應用程式建構器</returns>
        public static IApplicationBuilder UseRbacAuthorization(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RbacAuthorizationMiddleware>();
        }
    }
}