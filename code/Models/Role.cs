using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.Models;

/// <summary>
/// 角色模型 - RBAC 權限系統核心
/// </summary>
public class Role
{
    /// <summary>
    /// 角色ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 角色名稱 (唯一)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 角色顯示名稱
    /// </summary>
    [Required]
    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 角色描述
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// 是否為系統預設角色 (不可刪除)
    /// </summary>
    public bool IsSystemRole { get; set; } = false;

    /// <summary>
    /// 角色優先級 (數字越小優先級越高)
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 建立者ID
    /// </summary>
    public int? CreatedById { get; set; }

    /// <summary>
    /// 更新者ID
    /// </summary>
    public int? UpdatedById { get; set; }

    // Navigation Properties
    /// <summary>
    /// 角色權限對應
    /// </summary>
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    /// <summary>
    /// 使用者角色對應
    /// </summary>
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    /// <summary>
    /// 建立者
    /// </summary>
    public User? CreatedBy { get; set; }

    /// <summary>
    /// 更新者
    /// </summary>
    public User? UpdatedBy { get; set; }
}