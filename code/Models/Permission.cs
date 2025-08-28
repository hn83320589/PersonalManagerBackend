using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.Models;

/// <summary>
/// 權限模型 - 定義系統中的具體權限
/// </summary>
public class Permission
{
    /// <summary>
    /// 權限ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 權限名稱 (唯一標識符)
    /// 例如: users.create, users.read, posts.update, etc.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 權限顯示名稱
    /// </summary>
    [Required]
    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 權限描述
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// 權限分類/模組 (例如: Users, Posts, System)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// 權限動作類型
    /// </summary>
    public PermissionAction Action { get; set; }

    /// <summary>
    /// 權限資源 (例如: User, Post, Comment)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Resource { get; set; } = string.Empty;

    /// <summary>
    /// 是否為系統預設權限 (不可刪除)
    /// </summary>
    public bool IsSystemPermission { get; set; } = false;

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

    // Navigation Properties
    /// <summary>
    /// 角色權限對應
    /// </summary>
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

/// <summary>
/// 權限動作列舉
/// </summary>
public enum PermissionAction
{
    /// <summary>
    /// 建立
    /// </summary>
    Create = 1,

    /// <summary>
    /// 讀取
    /// </summary>
    Read = 2,

    /// <summary>
    /// 更新
    /// </summary>
    Update = 3,

    /// <summary>
    /// 刪除
    /// </summary>
    Delete = 4,

    /// <summary>
    /// 管理 (全部權限)
    /// </summary>
    Manage = 5,

    /// <summary>
    /// 執行特殊操作
    /// </summary>
    Execute = 6,

    /// <summary>
    /// 匯出資料
    /// </summary>
    Export = 7,

    /// <summary>
    /// 匯入資料
    /// </summary>
    Import = 8,

    /// <summary>
    /// 發佈內容
    /// </summary>
    Publish = 9,

    /// <summary>
    /// 審核內容
    /// </summary>
    Approve = 10
}