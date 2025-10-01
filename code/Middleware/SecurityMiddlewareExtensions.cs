namespace PersonalManagerAPI.Middleware;

/// <summary>
/// 安全中介軟體擴展方法
/// </summary>
public static class SecurityMiddlewareExtensions
{
    /// <summary>
    /// 添加安全驗證中介軟體
    /// </summary>
    public static IApplicationBuilder UseSecurityValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityValidationMiddleware>();
    }

    /// <summary>
    /// 添加完整的安全中介軟體管線
    /// </summary>
    public static IApplicationBuilder UseSecurityPipeline(this IApplicationBuilder builder)
    {
        // 按照正確的順序添加安全中介軟體
        builder.UseSecurityValidation();
        // 可以在這裡添加其他安全相關的中介軟體
        return builder;
    }
}