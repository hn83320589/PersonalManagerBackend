using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.Models;

/// <summary>
/// 受信任設備實體模型 - Entity Framework 版本
/// 用於管理使用者的受信任設備資訊與安全狀態
/// </summary>
public class TrustedDevice
{
    /// <summary>
    /// 受信任設備ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 使用者ID
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// 設備指紋 (SHA256雜湊值的前16字符)
    /// </summary>
    [Required]
    [StringLength(255)]
    public string DeviceFingerprint { get; set; } = string.Empty;

    /// <summary>
    /// 設備名稱
    /// </summary>
    [StringLength(200)]
    public string? DeviceName { get; set; }

    /// <summary>
    /// 設備類型 (Mobile, Desktop, Tablet, API)
    /// </summary>
    [StringLength(50)]
    public string? DeviceType { get; set; }

    /// <summary>
    /// 作業系統資訊
    /// </summary>
    [StringLength(200)]
    public string? OperatingSystem { get; set; }

    /// <summary>
    /// 瀏覽器資訊
    /// </summary>
    [StringLength(200)]
    public string? Browser { get; set; }

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
    /// 是否為受信任設備
    /// </summary>
    public bool IsTrusted { get; set; } = false;

    /// <summary>
    /// 首次使用時間
    /// </summary>
    public DateTime FirstUsedAt { get; set; }

    /// <summary>
    /// 最後活躍時間
    /// </summary>
    public DateTime LastActiveAt { get; set; }

    /// <summary>
    /// 信任開始時間
    /// </summary>
    public DateTime? TrustedAt { get; set; }

    /// <summary>
    /// 信任撤銷時間
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 使用者導航屬性
    /// </summary>
    public virtual User? User { get; set; }
}