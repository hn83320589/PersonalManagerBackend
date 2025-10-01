using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Services.Interfaces;
using PersonalManagerAPI.Middleware.Exceptions;
using PersonalManagerAPI.Attributes;

namespace PersonalManagerAPI.Controllers;

/// <summary>
/// 設備安全管理控制器 - 提供設備檢測、信任管理、可疑活動檢測等功能
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DeviceSecurityController : BaseController
{
    private readonly IDeviceSecurityService _deviceSecurityService;
    private readonly IUserSessionService _sessionService;
    private readonly ILogger<DeviceSecurityController> _logger;

    public DeviceSecurityController(
        IDeviceSecurityService deviceSecurityService,
        IUserSessionService sessionService,
        ILogger<DeviceSecurityController> logger)
    {
        _deviceSecurityService = deviceSecurityService;
        _sessionService = sessionService;
        _logger = logger;
    }

    /// <summary>
    /// 獲取當前設備資訊
    /// </summary>
    /// <returns>設備資訊</returns>
    [HttpGet("current-device")]
    public ActionResult<ApiResponse<DeviceInfoDto>> GetCurrentDeviceInfo()
    {
        try
        {
            var userAgent = Request.Headers["User-Agent"].ToString();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            
            var additionalHeaders = new Dictionary<string, string>();
            if (Request.Headers.ContainsKey("Accept-Language"))
                additionalHeaders["Accept-Language"] = Request.Headers["Accept-Language"].ToString();
            if (Request.Headers.ContainsKey("Accept-Encoding"))
                additionalHeaders["Accept-Encoding"] = Request.Headers["Accept-Encoding"].ToString();

            var deviceInfo = _deviceSecurityService.ExtractDeviceInfo(userAgent, ipAddress, additionalHeaders);
            
            return Ok(ApiResponse<DeviceInfoDto>.Success(deviceInfo, "成功獲取設備資訊"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "獲取設備資訊時發生錯誤");
            return BadRequest(ApiResponse<DeviceInfoDto>.Failure("獲取設備資訊失敗"));
        }
    }

    /// <summary>
    /// 評估當前登入的安全風險
    /// </summary>
    /// <returns>風險評估結果</returns>
    [HttpPost("assess-risk")]
    public async Task<ActionResult<ApiResponse<SecurityRiskAssessment>>> AssessCurrentLoginRisk()
    {
        try
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(ApiResponse<SecurityRiskAssessment>.Failure("無效的令牌"));
            }

            var userAgent = Request.Headers["User-Agent"].ToString();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var location = DetermineLocation(ipAddress);
            
            var additionalHeaders = new Dictionary<string, string>();
            if (Request.Headers.ContainsKey("Accept-Language"))
                additionalHeaders["Accept-Language"] = Request.Headers["Accept-Language"].ToString();

            var deviceInfo = _deviceSecurityService.ExtractDeviceInfo(userAgent, ipAddress, additionalHeaders);
            var riskAssessment = await _deviceSecurityService.AssessLoginRiskAsync(userId, deviceInfo, ipAddress, location);

            return Ok(ApiResponse<SecurityRiskAssessment>.Success(riskAssessment, "風險評估完成"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "評估登入風險時發生錯誤");
            return BadRequest(ApiResponse<SecurityRiskAssessment>.Failure("風險評估失敗"));
        }
    }

    /// <summary>
    /// 獲取受信任設備列表
    /// </summary>
    /// <returns>受信任設備列表</returns>
    [HttpGet("trusted-devices")]
    public async Task<ActionResult<ApiResponse<List<TrustedDeviceDto>>>> GetTrustedDevices()
    {
        try
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(ApiResponse<List<TrustedDeviceDto>>.Failure("無效的令牌"));
            }

            var trustedDevices = await _deviceSecurityService.GetTrustedDevicesAsync(userId);
            return Ok(ApiResponse<List<TrustedDeviceDto>>.Success(trustedDevices, "成功獲取受信任設備列表"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "獲取受信任設備列表時發生錯誤");
            return BadRequest(ApiResponse<List<TrustedDeviceDto>>.Failure("獲取受信任設備列表失敗"));
        }
    }

    /// <summary>
    /// 將當前設備標記為受信任
    /// </summary>
    /// <returns>操作結果</returns>
    [HttpPost("trust-current-device")]
    public async Task<ActionResult<ApiResponse<bool>>> TrustCurrentDevice()
    {
        try
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(ApiResponse<bool>.Failure("無效的令牌"));
            }

            var userAgent = Request.Headers["User-Agent"].ToString();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            
            var additionalHeaders = new Dictionary<string, string>();
            if (Request.Headers.ContainsKey("Accept-Language"))
                additionalHeaders["Accept-Language"] = Request.Headers["Accept-Language"].ToString();

            var deviceInfo = _deviceSecurityService.ExtractDeviceInfo(userAgent, ipAddress, additionalHeaders);
            var result = await _deviceSecurityService.TrustDeviceAsync(userId, deviceInfo.DeviceFingerprint!, deviceInfo);

            if (result)
            {
                return Ok(ApiResponse<bool>.Success(true, "設備已標記為受信任"));
            }
            else
            {
                return BadRequest(ApiResponse<bool>.Failure("標記設備為受信任失敗"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "標記設備為受信任時發生錯誤");
            return BadRequest(ApiResponse<bool>.Failure("標記設備為受信任失敗"));
        }
    }

    /// <summary>
    /// 撤銷設備信任
    /// </summary>
    /// <param name="deviceFingerprint">設備指紋</param>
    /// <returns>操作結果</returns>
    [HttpPost("revoke-trust/{deviceFingerprint}")]
    public async Task<ActionResult<ApiResponse<bool>>> RevokeTrust(string deviceFingerprint)
    {
        try
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(ApiResponse<bool>.Failure("無效的令牌"));
            }

            var result = await _deviceSecurityService.RevokeTrustAsync(userId, deviceFingerprint);

            if (result)
            {
                return Ok(ApiResponse<bool>.Success(true, "設備信任已撤銷"));
            }
            else
            {
                return NotFound(ApiResponse<bool>.Failure("找不到指定的受信任設備"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "撤銷設備信任時發生錯誤");
            return BadRequest(ApiResponse<bool>.Failure("撤銷設備信任失敗"));
        }
    }

    /// <summary>
    /// 檢測可疑活動
    /// </summary>
    /// <returns>可疑活動列表</returns>
    [HttpGet("suspicious-activities")]
    public async Task<ActionResult<ApiResponse<List<SuspiciousActivityDto>>>> GetSuspiciousActivities()
    {
        try
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(ApiResponse<List<SuspiciousActivityDto>>.Failure("無效的令牌"));
            }

            var activities = await _deviceSecurityService.DetectSuspiciousActivityAsync(userId);
            return Ok(ApiResponse<List<SuspiciousActivityDto>>.Success(activities, "成功檢測可疑活動"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "檢測可疑活動時發生錯誤");
            return BadRequest(ApiResponse<List<SuspiciousActivityDto>>.Failure("檢測可疑活動失敗"));
        }
    }

    /// <summary>
    /// 終止所有可疑會話
    /// </summary>
    /// <returns>終止的會話數量</returns>
    [HttpPost("terminate-suspicious-sessions")]
    public async Task<ActionResult<ApiResponse<int>>> TerminateSuspiciousSessions()
    {
        try
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(ApiResponse<int>.Failure("無效的令牌"));
            }

            var terminatedCount = await _deviceSecurityService.TerminateSuspiciousSessionsAsync(userId, "User requested termination of suspicious sessions");
            
            return Ok(ApiResponse<int>.Success(terminatedCount, $"已終止 {terminatedCount} 個可疑會話"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "終止可疑會話時發生錯誤");
            return BadRequest(ApiResponse<int>.Failure("終止可疑會話失敗"));
        }
    }

    /// <summary>
    /// 強化會話安全檢查
    /// </summary>
    /// <returns>安全檢查結果</returns>
    [HttpPost("security-check")]
    public async Task<ActionResult<ApiResponse<SecurityCheckResult>>> PerformSecurityCheck()
    {
        try
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(ApiResponse<SecurityCheckResult>.Failure("無效的令牌"));
            }

            var result = new SecurityCheckResult();

            // 1. 檢查活躍會話
            var activeSessions = await _sessionService.GetActiveSessionsAsync(userId);
            result.ActiveSessionsCount = activeSessions.Count;

            // 2. 檢查可疑活動
            var suspiciousActivities = await _deviceSecurityService.DetectSuspiciousActivityAsync(userId);
            result.SuspiciousActivitiesCount = suspiciousActivities.Count;
            result.HighRiskActivitiesCount = suspiciousActivities.Count(a => a.RiskLevel >= RiskLevel.High);

            // 3. 檢查受信任設備
            var trustedDevices = await _deviceSecurityService.GetTrustedDevicesAsync(userId);
            result.TrustedDevicesCount = trustedDevices.Count(d => d.IsActive);

            // 4. 安全建議
            var recommendations = new List<string>();
            
            if (result.ActiveSessionsCount > 5)
                recommendations.Add("建議檢查並終止不必要的活躍會話");
            
            if (result.HighRiskActivitiesCount > 0)
                recommendations.Add("檢測到高風險活動，建議立即檢查");
            
            if (result.TrustedDevicesCount == 0)
                recommendations.Add("建議將常用設備標記為受信任");

            if (recommendations.Count == 0)
                recommendations.Add("您的帳戶安全狀況良好");

            result.SecurityRecommendations = recommendations;
            result.OverallSecurityScore = CalculateSecurityScore(result);

            return Ok(ApiResponse<SecurityCheckResult>.Success(result, "安全檢查完成"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "執行安全檢查時發生錯誤");
            return BadRequest(ApiResponse<SecurityCheckResult>.Failure("安全檢查失敗"));
        }
    }

    #region Private Helper Methods

    private string DetermineLocation(string? ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress))
            return "Unknown";

        // 簡化的地理位置檢測
        if (ipAddress == "127.0.0.1" || ipAddress == "::1" || 
            ipAddress.StartsWith("192.168.") || ipAddress.StartsWith("10."))
        {
            return "本地網路";
        }

        // 在實際應用中，這裡會使用 GeoIP 服務
        return "External Network";
    }

    private int CalculateSecurityScore(SecurityCheckResult result)
    {
        int score = 100;

        // 扣分規則
        if (result.ActiveSessionsCount > 5)
            score -= 10;
        
        if (result.SuspiciousActivitiesCount > 0)
            score -= result.SuspiciousActivitiesCount * 5;
        
        if (result.HighRiskActivitiesCount > 0)
            score -= result.HighRiskActivitiesCount * 15;
        
        if (result.TrustedDevicesCount == 0)
            score -= 5;

        return Math.Max(score, 0);
    }

    #endregion
}

/// <summary>
/// 安全檢查結果
/// </summary>
public class SecurityCheckResult
{
    /// <summary>
    /// 活躍會話數量
    /// </summary>
    public int ActiveSessionsCount { get; set; }

    /// <summary>
    /// 可疑活動數量
    /// </summary>
    public int SuspiciousActivitiesCount { get; set; }

    /// <summary>
    /// 高風險活動數量
    /// </summary>
    public int HighRiskActivitiesCount { get; set; }

    /// <summary>
    /// 受信任設備數量
    /// </summary>
    public int TrustedDevicesCount { get; set; }

    /// <summary>
    /// 整體安全分數 (0-100)
    /// </summary>
    public int OverallSecurityScore { get; set; }

    /// <summary>
    /// 安全建議
    /// </summary>
    public List<string> SecurityRecommendations { get; set; } = new();
}