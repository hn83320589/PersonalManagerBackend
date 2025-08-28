using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.Models;

/// <summary>
/// 使用者角色對應模型 - 支援多重角色
/// </summary>
public class UserRole
{
    /// <summary>
    /// 記錄ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 使用者ID
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// 角色ID
    /// </summary>
    [Required]
    public int RoleId { get; set; }

    /// <summary>
    /// 是否為主要角色
    /// </summary>
    public bool IsPrimary { get; set; } = false;

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 角色生效時間
    /// </summary>
    public DateTime? ValidFrom { get; set; }

    /// <summary>
    /// 角色到期時間
    /// </summary>
    public DateTime? ValidTo { get; set; }

    /// <summary>
    /// 分配時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 分配者ID
    /// </summary>
    public int? AssignedById { get; set; }

    /// <summary>
    /// 更新者ID
    /// </summary>
    public int? UpdatedById { get; set; }

    /// <summary>
    /// 分配原因
    /// </summary>
    [StringLength(500)]
    public string? AssignmentReason { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    [StringLength(500)]
    public string? Notes { get; set; }

    // Navigation Properties
    /// <summary>
    /// 使用者
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// 角色
    /// </summary>
    public Role Role { get; set; } = null!;

    /// <summary>
    /// 分配者
    /// </summary>
    public User? AssignedBy { get; set; }

    /// <summary>
    /// 更新者
    /// </summary>
    public User? UpdatedBy { get; set; }
}