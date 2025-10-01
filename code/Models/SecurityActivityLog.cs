using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.Models;

/// <summary>
/// 安全活動日誌實體模型 - Entity Framework 版本
/// 用於記錄所有安全相關的活動與事件
/// </summary>
public class SecurityActivityLog
{
    /// <summary>
    /// 日誌ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 使用者ID
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// 活動類型 (Login, Logout, DeviceTrust, SuspiciousActivity, etc.)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string ActivityType { get; set; } = string.Empty;

    /// <summary>
    /// 活動描述
    /// </summary>
    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 設備指紋
    /// </summary>
    [StringLength(255)]
    public string? DeviceFingerprint { get; set; }

    /// <summary>
    /// IP地址
    /// </summary>
    [StringLength(45)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// 地理位置
    /// </summary>
    [StringLength(200)]
    public string? Location { get; set; }

    /// <summary>
    /// User-Agent字符串
    /// </summary>
    [StringLength(1000)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// 風險等級 (Low, Medium, High, Critical)
    /// </summary>
    [StringLength(20)]
    public string? RiskLevel { get; set; }

    /// <summary>
    /// 風險分數 (0-100)
    /// </summary>
    public int RiskScore { get; set; } = 0;

    /// <summary>
    /// 是否為可疑活動
    /// </summary>
    public bool IsSuspicious { get; set; } = false;

    /// <summary>
    /// 是否已處理
    /// </summary>
    public bool IsHandled { get; set; } = false;

    /// <summary>
    /// 處理時間
    /// </summary>
    public DateTime? HandledAt { get; set; }

    /// <summary>
    /// 活動時間
    /// </summary>
    public DateTime ActivityAt { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 使用者導航屬性
    /// </summary>
    public virtual User? User { get; set; }
}