using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PersonalManager.Api.Controllers;

/// <summary>
/// 共用基礎控制器，提供從 JWT Token 取得當前登入使用者 ID 的輔助方法。
/// </summary>
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// 從 JWT Token Claims 取得當前登入使用者的 ID。
    /// 若 Token 無效或缺少對應 Claim，回傳 null。
    /// </summary>
    protected int? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && int.TryParse(claim.Value, out var id) ? id : null;
    }
}
