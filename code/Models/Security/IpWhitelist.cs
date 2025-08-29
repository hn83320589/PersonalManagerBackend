using System.ComponentModel.DataAnnotations;
using PersonalManagerAPI.Models.Enums;

namespace PersonalManagerAPI.Models.Security;

/// <summary>
/// IP 白名單模型
/// </summary>
public class IpWhitelist
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(45)] // IPv6 最大長度
    public string IpAddress { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Reason { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime? ExpiresAt { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// IP 黑名單模型
/// </summary>
public class IpBlacklist
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(45)]
    public string IpAddress { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Reason { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime? ExpiresAt { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// 封鎖次數
    /// </summary>
    public int BlockCount { get; set; } = 1;

    /// <summary>
    /// 最後封鎖時間
    /// </summary>
    public DateTime? LastBlockedAt { get; set; }
}

/// <summary>
/// 安全日誌模型
/// </summary>
public class SecurityLog
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(45)]
    public string IpAddress { get; set; } = string.Empty;

    [Required]
    public SuspiciousActivityType ActivityType { get; set; }

    [Required]
    public ThreatLevel ThreatLevel { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Details { get; set; } = string.Empty;

    [MaxLength(500)]
    public string UserAgent { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Endpoint { get; set; } = string.Empty;

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public bool IsResolved { get; set; } = false;

    public DateTime? ResolvedAt { get; set; }

    [MaxLength(100)]
    public string ResolvedBy { get; set; } = string.Empty;

    [MaxLength(500)]
    public string ResolutionNotes { get; set; } = string.Empty;
}