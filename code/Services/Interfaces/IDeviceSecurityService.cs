using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Models;

namespace PersonalManagerAPI.Services.Interfaces;

/// <summary>
/// 設備安全服務介面 - 提供設備檢測、指紋驗證、可疑活動檢測等功能
/// </summary>
public interface IDeviceSecurityService
{
    /// <summary>
    /// 解析並提取設備資訊
    /// </summary>
    /// <param name="userAgent">User-Agent 字串</param>
    /// <param name="ipAddress">IP 地址</param>
    /// <param name="additionalHeaders">額外的 HTTP 標頭</param>
    /// <returns>設備資訊</returns>
    DeviceInfoDto ExtractDeviceInfo(string? userAgent, string? ipAddress, Dictionary<string, string>? additionalHeaders = null);

    /// <summary>
    /// 生成設備指紋
    /// </summary>
    /// <param name="deviceInfo">設備資訊</param>
    /// <param name="userAgent">User-Agent 字串</param>
    /// <param name="ipAddress">IP 地址</param>
    /// <param name="additionalData">額外數據</param>
    /// <returns>設備指紋</returns>
    string GenerateDeviceFingerprint(DeviceInfoDto deviceInfo, string? userAgent, string? ipAddress, Dictionary<string, string>? additionalData = null);

    /// <summary>
    /// 檢測可疑登入活動
    /// </summary>
    /// <param name="userId">用戶ID</param>
    /// <param name="deviceInfo">設備資訊</param>
    /// <param name="ipAddress">IP 地址</param>
    /// <param name="location">地理位置</param>
    /// <returns>風險評估結果</returns>
    Task<SecurityRiskAssessment> AssessLoginRiskAsync(int userId, DeviceInfoDto deviceInfo, string? ipAddress, string? location);

    /// <summary>
    /// 檢查設備是否受信任
    /// </summary>
    /// <param name="userId">用戶ID</param>
    /// <param name="deviceFingerprint">設備指紋</param>
    /// <returns>是否受信任</returns>
    Task<bool> IsDeviceTrustedAsync(int userId, string deviceFingerprint);

    /// <summary>
    /// 將設備標記為受信任
    /// </summary>
    /// <param name="userId">用戶ID</param>
    /// <param name="deviceFingerprint">設備指紋</param>
    /// <param name="deviceInfo">設備資訊</param>
    /// <returns>操作結果</returns>
    Task<bool> TrustDeviceAsync(int userId, string deviceFingerprint, DeviceInfoDto deviceInfo);

    /// <summary>
    /// 撤銷設備信任
    /// </summary>
    /// <param name="userId">用戶ID</param>
    /// <param name="deviceFingerprint">設備指紋</param>
    /// <returns>操作結果</returns>
    Task<bool> RevokeTrustAsync(int userId, string deviceFingerprint);

    /// <summary>
    /// 獲取用戶的受信任設備列表
    /// </summary>
    /// <param name="userId">用戶ID</param>
    /// <returns>受信任設備列表</returns>
    Task<List<TrustedDeviceDto>> GetTrustedDevicesAsync(int userId);

    /// <summary>
    /// 檢測同時登入的異常活動
    /// </summary>
    /// <param name="userId">用戶ID</param>
    /// <returns>異常活動檢測結果</returns>
    Task<List<SuspiciousActivityDto>> DetectSuspiciousActivityAsync(int userId);

    /// <summary>
    /// 強制終止所有可疑會話
    /// </summary>
    /// <param name="userId">用戶ID</param>
    /// <param name="reason">終止原因</param>
    /// <returns>終止的會話數量</returns>
    Task<int> TerminateSuspiciousSessionsAsync(int userId, string reason = "Suspicious activity detected");
}

/// <summary>
/// 安全風險評估結果
/// </summary>
public class SecurityRiskAssessment
{
    /// <summary>
    /// 風險等級 (Low, Medium, High, Critical)
    /// </summary>
    public RiskLevel RiskLevel { get; set; }

    /// <summary>
    /// 風險分數 (0-100)
    /// </summary>
    public int RiskScore { get; set; }

    /// <summary>
    /// 風險因素列表
    /// </summary>
    public List<string> RiskFactors { get; set; } = new();

    /// <summary>
    /// 建議操作
    /// </summary>
    public List<string> RecommendedActions { get; set; } = new();

    /// <summary>
    /// 是否需要額外驗證
    /// </summary>
    public bool RequiresAdditionalVerification { get; set; }

    /// <summary>
    /// 是否應該阻止登入
    /// </summary>
    public bool ShouldBlockLogin { get; set; }
}

/// <summary>
/// 風險等級枚舉
/// </summary>
public enum RiskLevel
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

/// <summary>
/// 受信任設備 DTO
/// </summary>
public class TrustedDeviceDto
{
    public int Id { get; set; }
    public string DeviceFingerprint { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string? OperatingSystem { get; set; }
    public DateTime FirstSeenAt { get; set; }
    public DateTime LastSeenAt { get; set; }
    public DateTime TrustedAt { get; set; }
    public bool IsActive { get; set; }
    public string? LastIpAddress { get; set; }
    public string? LastLocation { get; set; }
}

/// <summary>
/// 可疑活動 DTO
/// </summary>
public class SuspiciousActivityDto
{
    public int Id { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RiskLevel RiskLevel { get; set; }
    public DateTime DetectedAt { get; set; }
    public string? IpAddress { get; set; }
    public string? Location { get; set; }
    public string? DeviceInfo { get; set; }
    public bool IsResolved { get; set; }
    public List<string> AffectedSessions { get; set; } = new();
}