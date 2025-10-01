using PersonalManagerAPI.Services.Interfaces;
using System.Security.Claims;
using System.Text.Json;

namespace PersonalManagerAPI.Middleware;

/// <summary>
/// RBAC 權限檢查中介軟體
/// </summary>
public class RbacAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RbacAuthorizationMiddleware> _logger;

    // 路由權限對應表
    private static readonly Dictionary<string, string> RoutePermissionMap = new()
    {
        // 使用者管理
        { "GET:/api/users", "users.read" },
        { "POST:/api/users", "users.create" },
        { "PUT:/api/users", "users.update" },
        { "DELETE:/api/users", "users.delete" },
        
        // 個人資料管理
        { "GET:/api/personalprofiles", "profiles.read" },
        { "POST:/api/personalprofiles", "profiles.create" },
        { "PUT:/api/personalprofiles", "profiles.update" },
        { "DELETE:/api/personalprofiles", "profiles.delete" },
        
        // 學歷管理
        { "GET:/api/educations", "educations.read" },
        { "POST:/api/educations", "educations.create" },
        { "PUT:/api/educations", "educations.update" },
        { "DELETE:/api/educations", "educations.delete" },
        
        // 工作經歷管理
        { "GET:/api/workexperiences", "workexperiences.read" },
        { "POST:/api/workexperiences", "workexperiences.create" },
        { "PUT:/api/workexperiences", "workexperiences.update" },
        { "DELETE:/api/workexperiences", "workexperiences.delete" },
        
        // 技能管理
        { "GET:/api/skills", "skills.read" },
        { "POST:/api/skills", "skills.create" },
        { "PUT:/api/skills", "skills.update" },
        { "DELETE:/api/skills", "skills.delete" },
        
        // 作品集管理
        { "GET:/api/portfolios", "portfolios.read" },
        { "POST:/api/portfolios", "portfolios.create" },
        { "PUT:/api/portfolios", "portfolios.update" },
        { "DELETE:/api/portfolios", "portfolios.delete" },
        
        // 行事曆管理
        { "GET:/api/calendarevents", "calendar.read" },
        { "POST:/api/calendarevents", "calendar.create" },
        { "PUT:/api/calendarevents", "calendar.update" },
        { "DELETE:/api/calendarevents", "calendar.delete" },
        
        // 待辦事項管理
        { "GET:/api/todoitems", "todos.read" },
        { "POST:/api/todoitems", "todos.create" },
        { "PUT:/api/todoitems", "todos.update" },
        { "DELETE:/api/todoitems", "todos.delete" },
        
        // 工作任務管理
        { "GET:/api/worktasks", "worktasks.read" },
        { "POST:/api/worktasks", "worktasks.create" },
        { "PUT:/api/worktasks", "worktasks.update" },
        { "DELETE:/api/worktasks", "worktasks.delete" },
        
        // 部落格管理
        { "GET:/api/blogposts", "blog.read" },
        { "POST:/api/blogposts", "blog.create" },
        { "PUT:/api/blogposts", "blog.update" },
        { "DELETE:/api/blogposts", "blog.delete" },
        { "POST:/api/blogposts/*/publish", "blog.publish" },
        
        // 留言管理
        { "GET:/api/guestbookentries", "guestbook.read" },
        { "POST:/api/guestbookentries", "guestbook.create" },
        { "PUT:/api/guestbookentries", "guestbook.update" },
        { "DELETE:/api/guestbookentries", "guestbook.delete" },
        { "POST:/api/guestbookentries/*/approve", "guestbook.approve" },
        
        // 聯絡方式管理
        { "GET:/api/contactmethods", "contacts.read" },
        { "POST:/api/contactmethods", "contacts.create" },
        { "PUT:/api/contactmethods", "contacts.update" },
        { "DELETE:/api/contactmethods", "contacts.delete" },
        
        // 檔案管理
        { "POST:/api/files/upload", "files.upload" },
        { "DELETE:/api/files", "files.delete" },
        { "GET:/api/files/quarantined", "files.manage" },
        
        // 快取管理
        { "GET:/api/cache/stats", "cache.read" },
        { "POST:/api/cache/clear", "cache.manage" },
        { "DELETE:/api/cache", "cache.manage" },
        
        // 設備安全管理
        { "GET:/api/devicesecurity", "security.read" },
        { "POST:/api/devicesecurity/trust-device", "security.manage" },
        { "POST:/api/devicesecurity/revoke-trust", "security.manage" },
        { "POST:/api/devicesecurity/terminate-sessions", "security.manage" },
        
        // RBAC 管理
        { "GET:/api/rbac/roles", "rbac.read" },
        { "POST:/api/rbac/roles", "rbac.manage" },
        { "PUT:/api/rbac/roles", "rbac.manage" },
        { "DELETE:/api/rbac/roles", "rbac.manage" },
        { "GET:/api/rbac/permissions", "rbac.read" },
        { "POST:/api/rbac/permissions", "rbac.manage" },
        { "POST:/api/rbac/users/*/roles", "rbac.manage" },
        { "DELETE:/api/rbac/users/*/roles", "rbac.manage" }
    };

    // 需要跳過權限檢查的路由
    private static readonly HashSet<string> SkipAuthorizationRoutes = new()
    {
        "GET:/api/auth/me",
        "POST:/api/auth/login",
        "POST:/api/auth/register",
        "POST:/api/auth/refresh",
        "POST:/api/auth/logout",
        "GET:/api/auth/validate",
        "POST:/api/auth/smart-refresh",
        "GET:/api/auth/token-status",
        
        // 公開端點
        "GET:/api/personalprofiles/public",
        "GET:/api/skills/public",
        "GET:/api/portfolios/public",
        "GET:/api/blogposts/public",
        "GET:/api/blogposts/*/public",
        "GET:/api/calendarevents/public",
        "GET:/api/guestbookentries/public",
        "POST:/api/guestbookentries/public",
        "GET:/api/contactmethods/public"
    };

    public RbacAuthorizationMiddleware(RequestDelegate next, IServiceProvider serviceProvider, ILogger<RbacAuthorizationMiddleware> logger)
    {
        _next = next;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 檢查是否為需要權限檢查的端點
        var routeKey = $"{context.Request.Method}:{context.Request.Path}";
        
        // 先檢查精確匹配
        if (SkipAuthorizationRoutes.Contains(routeKey))
        {
            await _next(context);
            return;
        }

        // 檢查模式匹配 (對於包含參數的路由)
        if (ShouldSkipAuthorization(context))
        {
            await _next(context);
            return;
        }

        // 檢查使用者是否已認證
        if (!context.User.Identity?.IsAuthenticated == true)
        {
            _logger.LogWarning("未認證的使用者嘗試存取: {Method} {Path}", context.Request.Method, context.Request.Path);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { 
                success = false, 
                message = "使用者未認證",
                errors = new[] { "請先登入系統" }
            }));
            return;
        }

        // 取得使用者ID (嘗試多種可能的 claim 名稱)
        var userIdClaim = context.User.FindFirst("userId")?.Value ?? 
                         context.User.FindFirst("user_id")?.Value ?? 
                         context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("無法解析使用者ID: {UserIdClaim}", userIdClaim);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { 
                success = false, 
                message = "使用者身份無效",
                errors = new[] { "無法識別使用者身份" }
            }));
            return;
        }

        // 尋找對應的權限要求
        var requiredPermission = GetRequiredPermission(context);
        if (string.IsNullOrEmpty(requiredPermission))
        {
            // 如果沒有找到對應的權限要求，預設允許存取
            await _next(context);
            return;
        }

        // 建立 scope 來獲取 RBAC 服務
        using var scope = _serviceProvider.CreateScope();
        var rbacService = scope.ServiceProvider.GetRequiredService<IRbacService>();

        try
        {
            // 檢查使用者權限
            var hasPermission = await rbacService.CheckPermissionAsync(userId, requiredPermission);
            
            if (!hasPermission)
            {
                _logger.LogWarning("使用者 {UserId} 沒有權限 {Permission} 存取 {Method} {Path}", 
                    userId, requiredPermission, context.Request.Method, context.Request.Path);
                
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync(JsonSerializer.Serialize(new { 
                    success = false, 
                    message = "權限不足",
                    errors = new[] { $"您沒有 '{requiredPermission}' 權限" }
                }));
                return;
            }

            _logger.LogDebug("使用者 {UserId} 成功通過權限檢查 {Permission} 存取 {Method} {Path}", 
                userId, requiredPermission, context.Request.Method, context.Request.Path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "權限檢查時發生錯誤: {UserId}, {Permission}", userId, requiredPermission);
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { 
                success = false, 
                message = "權限檢查失敗",
                errors = new[] { "系統暫時無法驗證權限，請稍後再試" }
            }));
            return;
        }

        // 權限檢查通過，繼續處理請求
        await _next(context);
    }

    /// <summary>
    /// 檢查是否應該跳過權限檢查
    /// </summary>
    private static bool ShouldSkipAuthorization(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path.Value?.ToLower() ?? "";

        // 檢查是否為公開端點的模式匹配
        if (path.Contains("/public"))
        {
            return true;
        }

        // 檢查是否為認證相關端點
        if (path.StartsWith("/api/auth/"))
        {
            return true;
        }

        // 檢查 Swagger 相關端點
        if (path.StartsWith("/swagger") || path.StartsWith("/api-docs"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 根據 HTTP 請求獲取所需的權限
    /// </summary>
    private static string? GetRequiredPermission(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path.Value?.ToLower() ?? "";
        var routeKey = $"{method}:{path}";

        // 先檢查精確匹配
        if (RoutePermissionMap.TryGetValue(routeKey, out var permission))
        {
            return permission;
        }

        // 檢查模式匹配 (處理包含ID參數的路由)
        foreach (var (pattern, perm) in RoutePermissionMap)
        {
            if (IsRouteMatch(routeKey, pattern))
            {
                return perm;
            }
        }

        return null;
    }

    /// <summary>
    /// 檢查路由是否匹配模式
    /// </summary>
    private static bool IsRouteMatch(string actualRoute, string pattern)
    {
        // 將模式中的 * 替換為正規表達式的 .*
        var regexPattern = "^" + pattern.Replace("*", @"\d+") + "$";
        return System.Text.RegularExpressions.Regex.IsMatch(actualRoute, regexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
}