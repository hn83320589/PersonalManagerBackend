using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.Models;

/// <summary>
/// 使用者會話管理 - 追蹤多設備登入
/// </summary>
public class UserSession
{
    public int Id { get; set; }
    
    /// <summary>
    /// 關聯的使用者ID
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// 會話識別碼 (JTI from JWT)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string SessionId { get; set; } = string.Empty;
    
    /// <summary>
    /// Refresh Token
    /// </summary>
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
    
    /// <summary>
    /// 設備名稱
    /// </summary>
    [StringLength(100)]
    public string? DeviceName { get; set; }
    
    /// <summary>
    /// 設備類型 (Mobile, Desktop, Tablet, Unknown)
    /// </summary>
    [StringLength(20)]
    public string DeviceType { get; set; } = "Unknown";
    
    /// <summary>
    /// 作業系統
    /// </summary>
    [StringLength(50)]
    public string? OperatingSystem { get; set; }
    
    /// <summary>
    /// 瀏覽器/應用程式
    /// </summary>
    [StringLength(100)]
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// IP 位址
    /// </summary>
    [StringLength(45)] // IPv6 最大長度
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// 地理位置 (城市, 國家)
    /// </summary>
    [StringLength(100)]
    public string? Location { get; set; }
    
    /// <summary>
    /// 會話建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// 最後活躍時間
    /// </summary>
    public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Refresh Token 過期時間
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// 會話狀態
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// 是否為當前會話
    /// </summary>
    public bool IsCurrent { get; set; } = false;
    
    /// <summary>
    /// 會話結束時間 (登出或過期)
    /// </summary>
    public DateTime? EndedAt { get; set; }
    
    /// <summary>
    /// 結束原因 (Logout, Expired, Revoked, NewDevice)
    /// </summary>
    [StringLength(20)]
    public string? EndReason { get; set; }

    // Navigation Property
    public User User { get; set; } = null!;
}