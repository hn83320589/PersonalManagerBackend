using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Services.Interfaces;

namespace PersonalManagerAPI.Attributes;

/// <summary>
/// 需要特定權限的授權屬性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    /// <summary>
    /// 需要的權限名稱
    /// </summary>
    public string Permission { get; }

    /// <summary>
    /// 權限檢查模式
    /// </summary>
    public PermissionCheckMode Mode { get; set; } = PermissionCheckMode.Any;

    /// <summary>
    /// 是否允許系統管理員自動通過
    /// </summary>
    public bool AllowSystemAdmin { get; set; } = true;

    /// <summary>
    /// 錯誤訊息
    /// </summary>
    public string? ErrorMessage { get; set; }

    public RequirePermissionAttribute(string permission)
    {
        Permission = permission ?? throw new ArgumentNullException(nameof(permission));
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // 檢查使用者是否已認證
        if (!context.HttpContext.User.Identity?.IsAuthenticated == true)
        {
            context.Result = new UnauthorizedObjectResult(
                ApiResponse<object>.Failure("使用者未認證"));
            return;
        }

        // 取得使用者ID
        var userIdClaim = context.HttpContext.User.FindFirst("userId")?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
        {
            context.Result = new UnauthorizedObjectResult(
                ApiResponse<object>.Failure("無效的使用者令牌"));
            return;
        }

        // 如果允許系統管理員自動通過
        if (AllowSystemAdmin)
        {
            var roleClaim = context.HttpContext.User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
            if (roleClaim == "Admin" || roleClaim == "SystemAdmin")
            {
                return; // 允許通過
            }
        }

        // 取得權限檢查服務
        var rbacService = context.HttpContext.RequestServices.GetService<IRbacService>();
        if (rbacService == null)
        {
            context.Result = new ObjectResult(
                ApiResponse<object>.Failure("權限檢查服務無法使用"))
            {
                StatusCode = 500
            };
            return;
        }

        // 檢查權限
        var hasPermission = await rbacService.CheckPermissionAsync(userId, Permission);
        if (!hasPermission)
        {
            var errorMessage = ErrorMessage ?? $"使用者沒有執行此操作的權限: {Permission}";
            context.Result = new ForbidResult();
            
            // 設定自訂回應
            context.HttpContext.Response.StatusCode = 403;
            await context.HttpContext.Response.WriteAsync(
                System.Text.Json.JsonSerializer.Serialize(
                    ApiResponse<object>.Failure(errorMessage)
                )
            );
        }
    }
}

/// <summary>
/// 需要多個權限的授權屬性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequirePermissionsAttribute : Attribute, IAsyncAuthorizationFilter
{
    /// <summary>
    /// 需要的權限名稱列表
    /// </summary>
    public string[] Permissions { get; }

    /// <summary>
    /// 權限檢查模式
    /// </summary>
    public PermissionCheckMode Mode { get; set; } = PermissionCheckMode.Any;

    /// <summary>
    /// 是否允許系統管理員自動通過
    /// </summary>
    public bool AllowSystemAdmin { get; set; } = true;

    /// <summary>
    /// 錯誤訊息
    /// </summary>
    public string? ErrorMessage { get; set; }

    public RequirePermissionsAttribute(params string[] permissions)
    {
        Permissions = permissions ?? throw new ArgumentNullException(nameof(permissions));
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // 檢查使用者是否已認證
        if (!context.HttpContext.User.Identity?.IsAuthenticated == true)
        {
            context.Result = new UnauthorizedObjectResult(
                ApiResponse<object>.Failure("使用者未認證"));
            return;
        }

        // 取得使用者ID
        var userIdClaim = context.HttpContext.User.FindFirst("userId")?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
        {
            context.Result = new UnauthorizedObjectResult(
                ApiResponse<object>.Failure("無效的使用者令牌"));
            return;
        }

        // 如果允許系統管理員自動通過
        if (AllowSystemAdmin)
        {
            var roleClaim = context.HttpContext.User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
            if (roleClaim == "Admin" || roleClaim == "SystemAdmin")
            {
                return; // 允許通過
            }
        }

        // 取得權限檢查服務
        var rbacService = context.HttpContext.RequestServices.GetService<IRbacService>();
        if (rbacService == null)
        {
            context.Result = new ObjectResult(
                ApiResponse<object>.Failure("權限檢查服務無法使用"))
            {
                StatusCode = 500
            };
            return;
        }

        // 檢查權限
        bool hasPermission = Mode switch
        {
            PermissionCheckMode.All => await rbacService.CheckAllPermissionsAsync(userId, Permissions),
            PermissionCheckMode.Any => await rbacService.CheckAnyPermissionAsync(userId, Permissions),
            _ => await rbacService.CheckAnyPermissionAsync(userId, Permissions)
        };

        if (!hasPermission)
        {
            var errorMessage = ErrorMessage ?? $"使用者沒有執行此操作的權限: {string.Join(", ", Permissions)}";
            context.Result = new ForbidResult();
            
            // 設定自訂回應
            context.HttpContext.Response.StatusCode = 403;
            await context.HttpContext.Response.WriteAsync(
                System.Text.Json.JsonSerializer.Serialize(
                    ApiResponse<object>.Failure(errorMessage)
                )
            );
        }
    }
}

/// <summary>
/// 權限檢查模式
/// </summary>
public enum PermissionCheckMode
{
    /// <summary>
    /// 任何一個權限符合即可
    /// </summary>
    Any = 0,

    /// <summary>
    /// 所有權限都必須符合
    /// </summary>
    All = 1
}