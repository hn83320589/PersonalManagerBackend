using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Services.Interfaces;
using PersonalManagerAPI.Middleware.Exceptions;

namespace PersonalManagerAPI.Controllers;

/// <summary>
/// 會話管理控制器 - 處理多設備登入控制
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SessionController : BaseController
{
    private readonly IUserSessionService _sessionService;
    private readonly ILogger<SessionController> _logger;

    public SessionController(IUserSessionService sessionService, ILogger<SessionController> logger)
    {
        _sessionService = sessionService;
        _logger = logger;
    }

    /// <summary>
    /// 獲取當前用戶的所有會話
    /// </summary>
    /// <returns>用戶會話列表</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<UserSessionDto>>>> GetMySessions()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(ApiResponse<List<UserSessionDto>>.Failure("無效的令牌"));
        }

        var sessions = await _sessionService.GetUserSessionsAsync(userId);
        return Ok(ApiResponse<List<UserSessionDto>>.Success(sessions, "成功獲取會話列表"));
    }

    /// <summary>
    /// 獲取當前用戶的活躍會話
    /// </summary>
    /// <returns>活躍會話列表</returns>
    [HttpGet("active")]
    public async Task<ActionResult<ApiResponse<List<UserSessionDto>>>> GetActiveSessions()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(ApiResponse<List<UserSessionDto>>.Failure("無效的令牌"));
        }

        var sessions = await _sessionService.GetActiveSessionsAsync(userId);
        return Ok(ApiResponse<List<UserSessionDto>>.Success(sessions, "成功獲取活躍會話"));
    }

    /// <summary>
    /// 結束指定的會話
    /// </summary>
    /// <param name="sessionId">會話ID</param>
    /// <returns>操作結果</returns>
    [HttpDelete("{sessionId}")]
    public async Task<ActionResult<ApiResponse<object>>> EndSession(int sessionId)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("無效的令牌"));
        }

        // 驗證會話屬於當前用戶
        var sessions = await _sessionService.GetUserSessionsAsync(userId);
        var targetSession = sessions.FirstOrDefault(s => s.Id == sessionId);

        if (targetSession == null)
        {
            return NotFound(ApiResponse<object>.Failure("會話不存在或無權限"));
        }

        var result = await _sessionService.RevokeSessionAsync(sessionId, "UserTerminated");
        if (result)
        {
            _logger.LogInformation("用戶結束會話: UserId={UserId}, SessionId={SessionId}", userId, sessionId);
            return Ok(ApiResponse<object>.Success(null!, "會話已成功結束"));
        }

        return BadRequest(ApiResponse<object>.Failure("結束會話失敗"));
    }

    /// <summary>
    /// 結束所有其他會話 (保留當前會話)
    /// </summary>
    /// <returns>操作結果</returns>
    [HttpPost("logout-others")]
    public async Task<ActionResult<ApiResponse<object>>> LogoutOtherSessions()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        var currentSessionId = User.FindFirst("jti")?.Value;

        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId) || string.IsNullOrEmpty(currentSessionId))
        {
            return Unauthorized(ApiResponse<object>.Failure("無效的令牌"));
        }

        var result = await _sessionService.EndOtherSessionsAsync(userId, currentSessionId, "LogoutOthers");
        if (result)
        {
            _logger.LogInformation("用戶登出其他會話: UserId={UserId}, CurrentSession={CurrentSession}",
                userId, currentSessionId);
            return Ok(ApiResponse<object>.Success(null!, "已成功登出所有其他設備"));
        }

        return BadRequest(ApiResponse<object>.Failure("登出其他會話失敗"));
    }

    /// <summary>
    /// 結束所有會話 (全部登出)
    /// </summary>
    /// <returns>操作結果</returns>
    [HttpPost("logout-all")]
    public async Task<ActionResult<ApiResponse<object>>> LogoutAllSessions()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("無效的令牌"));
        }

        var result = await _sessionService.EndAllUserSessionsAsync(userId, "LogoutAll");
        if (result)
        {
            _logger.LogInformation("用戶登出所有會話: UserId={UserId}", userId);
            return Ok(ApiResponse<object>.Success(null!, "已成功登出所有設備"));
        }

        return BadRequest(ApiResponse<object>.Failure("登出所有會話失敗"));
    }

    /// <summary>
    /// 獲取會話統計資訊
    /// </summary>
    /// <returns>統計資料</returns>
    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<object>>> GetSessionStats()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("無效的令牌"));
        }

        var stats = await _sessionService.GetSessionStatsAsync(userId);
        return Ok(ApiResponse<object>.Success(stats, "成功獲取會話統計"));
    }

    /// <summary>
    /// 獲取設備使用歷史
    /// </summary>
    /// <param name="days">查詢天數 (預設30天)</param>
    /// <returns>設備歷史</returns>
    [HttpGet("device-history")]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetDeviceHistory([FromQuery] int days = 30)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(ApiResponse<List<object>>.Failure("無效的令牌"));
        }

        if (days < 1 || days > 90)
        {
            return BadRequest(ApiResponse<List<object>>.Failure("查詢天數必須在 1-90 天之間"));
        }

        var history = await _sessionService.GetDeviceHistoryAsync(userId, days);
        return Ok(ApiResponse<List<object>>.Success(history, "成功獲取設備歷史"));
    }

    /// <summary>
    /// 更新當前會話活躍時間
    /// </summary>
    /// <returns>操作結果</returns>
    [HttpPost("heartbeat")]
    public async Task<ActionResult<ApiResponse<object>>> Heartbeat()
    {
        var sessionId = User.FindFirst("jti")?.Value;
        if (string.IsNullOrEmpty(sessionId))
        {
            return Unauthorized(ApiResponse<object>.Failure("無效的會話"));
        }

        var result = await _sessionService.UpdateLastActiveAsync(sessionId);
        if (result)
        {
            return Ok(ApiResponse<object>.Success(null!, "會話活躍時間已更新"));
        }

        return BadRequest(ApiResponse<object>.Failure("更新會話失敗"));
    }

    /// <summary>
    /// 檢查可疑活動 (僅供測試)
    /// </summary>
    /// <param name="deviceInfo">設備資訊</param>
    /// <returns>檢測結果</returns>
    [HttpPost("security-check")]
    public async Task<ActionResult<ApiResponse<object>>> SecurityCheck([FromBody] DeviceInfoDto? deviceInfo = null)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("無效的令牌"));
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _sessionService.DetectSuspiciousActivityAsync(userId, deviceInfo, ipAddress);

        return Ok(ApiResponse<object>.Success(result, "安全檢查完成"));
    }
}