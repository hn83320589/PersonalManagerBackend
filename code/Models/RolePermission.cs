using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.Models;

/// <summary>
/// 角色權限對應模型 - 多對多關係表
/// </summary>
public class RolePermission
{
    /// <summary>
    /// 記錄ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 角色ID
    /// </summary>
    [Required]
    public int RoleId { get; set; }

    /// <summary>
    /// 權限ID
    /// </summary>
    [Required]
    public int PermissionId { get; set; }

    /// <summary>
    /// 是否啟用此權限
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 權限條件 (JSON格式的額外限制條件)
    /// 例如: {"department": "IT", "level": "senior"}
    /// </summary>
    [StringLength(1000)]
    public string? Conditions { get; set; }

    /// <summary>
    /// 權限生效時間
    /// </summary>
    public DateTime? ValidFrom { get; set; }

    /// <summary>
    /// 權限到期時間
    /// </summary>
    public DateTime? ValidTo { get; set; }

    /// <summary>
    /// 分配時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 分配者ID
    /// </summary>
    public int? CreatedById { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    [StringLength(500)]
    public string? Notes { get; set; }

    // Navigation Properties
    /// <summary>
    /// 角色
    /// </summary>
    public Role Role { get; set; } = null!;

    /// <summary>
    /// 權限
    /// </summary>
    public Permission Permission { get; set; } = null!;

    /// <summary>
    /// 分配者
    /// </summary>
    public User? CreatedBy { get; set; }
}