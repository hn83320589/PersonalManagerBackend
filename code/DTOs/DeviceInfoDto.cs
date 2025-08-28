using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.DTOs;

/// <summary>
/// 設備資訊 DTO - 用於登入時提供設備信息
/// </summary>
public class DeviceInfoDto
{
    /// <summary>
    /// 設備名稱 (可選)
    /// </summary>
    [StringLength(100)]
    public string? DeviceName { get; set; }
    
    /// <summary>
    /// 設備類型 (Mobile, Desktop, Tablet)
    /// </summary>
    [StringLength(20)]
    public string? DeviceType { get; set; }
    
    /// <summary>
    /// 作業系統
    /// </summary>
    [StringLength(50)]
    public string? OperatingSystem { get; set; }
    
    /// <summary>
    /// 設備指紋 (可選的唯一識別)
    /// </summary>
    [StringLength(100)]
    public string? DeviceFingerprint { get; set; }
}

/// <summary>
/// 用戶會話資訊 DTO - 用於返回會話信息
/// </summary>
public class UserSessionDto
{
    public int Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string? DeviceName { get; set; }
    public string DeviceType { get; set; } = string.Empty;
    public string? OperatingSystem { get; set; }
    public string? IpAddress { get; set; }
    public string? Location { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastActiveAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsCurrent { get; set; }
    public DateTime? EndedAt { get; set; }
    public string? EndReason { get; set; }
}

/// <summary>
/// 會話管理請求 DTO
/// </summary>
public class SessionActionDto
{
    /// <summary>
    /// 會話ID
    /// </summary>
    [Required]
    public int SessionId { get; set; }
    
    /// <summary>
    /// 操作類型 (terminate, revoke)
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Action { get; set; } = string.Empty;
    
    /// <summary>
    /// 操作原因 (可選)
    /// </summary>
    [StringLength(100)]
    public string? Reason { get; set; }
}

/// <summary>
/// 增強的登入請求 DTO - 包含設備資訊
/// </summary>
public class EnhancedLoginDto : LoginDto
{
    /// <summary>
    /// 設備資訊
    /// </summary>
    public DeviceInfoDto? DeviceInfo { get; set; }
    
    /// <summary>
    /// 是否記住此設備
    /// </summary>
    public bool RememberDevice { get; set; } = false;
}

/// <summary>
/// 增強的註冊請求 DTO - 包含設備資訊
/// </summary>
public class EnhancedRegisterDto : RegisterDto
{
    /// <summary>
    /// 設備資訊
    /// </summary>
    public DeviceInfoDto? DeviceInfo { get; set; }
    
    /// <summary>
    /// 是否記住此設備
    /// </summary>
    public bool RememberDevice { get; set; } = false;
}