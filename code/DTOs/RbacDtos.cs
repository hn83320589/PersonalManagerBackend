using System.ComponentModel.DataAnnotations;
using PersonalManagerAPI.Models;

namespace PersonalManagerAPI.DTOs;

#region Role DTOs

/// <summary>
/// 建立角色 DTO
/// </summary>
public class CreateRoleDto
{
    /// <summary>
    /// 角色名稱 (唯一)
    /// </summary>
    [Required(ErrorMessage = "角色名稱為必填")]
    [StringLength(50, ErrorMessage = "角色名稱不能超過50字元")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 角色顯示名稱
    /// </summary>
    [Required(ErrorMessage = "角色顯示名稱為必填")]
    [StringLength(100, ErrorMessage = "角色顯示名稱不能超過100字元")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 角色描述
    /// </summary>
    [StringLength(500, ErrorMessage = "角色描述不能超過500字元")]
    public string? Description { get; set; }

    /// <summary>
    /// 角色優先級
    /// </summary>
    [Range(1, 1000, ErrorMessage = "角色優先級必須在1-1000之間")]
    public int Priority { get; set; } = 100;

    /// <summary>
    /// 權限ID列表
    /// </summary>
    public List<int> PermissionIds { get; set; } = new List<int>();
}

/// <summary>
/// 更新角色 DTO
/// </summary>
public class UpdateRoleDto
{
    /// <summary>
    /// 角色顯示名稱
    /// </summary>
    [Required(ErrorMessage = "角色顯示名稱為必填")]
    [StringLength(100, ErrorMessage = "角色顯示名稱不能超過100字元")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 角色描述
    /// </summary>
    [StringLength(500, ErrorMessage = "角色描述不能超過500字元")]
    public string? Description { get; set; }

    /// <summary>
    /// 角色優先級
    /// </summary>
    [Range(1, 1000, ErrorMessage = "角色優先級必須在1-1000之間")]
    public int Priority { get; set; } = 100;

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 權限ID列表
    /// </summary>
    public List<int> PermissionIds { get; set; } = new List<int>();
}

/// <summary>
/// 角色回應 DTO
/// </summary>
public class RoleResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int? CreatedById { get; set; }
    public int? UpdatedById { get; set; }
    public int UserCount { get; set; }
    public int PermissionCount { get; set; }
    public List<PermissionResponseDto> Permissions { get; set; } = new List<PermissionResponseDto>();
}

#endregion

#region Permission DTOs

/// <summary>
/// 建立權限 DTO
/// </summary>
public class CreatePermissionDto
{
    /// <summary>
    /// 權限名稱
    /// </summary>
    [Required(ErrorMessage = "權限名稱為必填")]
    [StringLength(100, ErrorMessage = "權限名稱不能超過100字元")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 權限顯示名稱
    /// </summary>
    [Required(ErrorMessage = "權限顯示名稱為必填")]
    [StringLength(100, ErrorMessage = "權限顯示名稱不能超過100字元")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 權限描述
    /// </summary>
    [StringLength(500, ErrorMessage = "權限描述不能超過500字元")]
    public string? Description { get; set; }

    /// <summary>
    /// 權限分類
    /// </summary>
    [Required(ErrorMessage = "權限分類為必填")]
    [StringLength(50, ErrorMessage = "權限分類不能超過50字元")]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// 權限動作
    /// </summary>
    [Required(ErrorMessage = "權限動作為必填")]
    public PermissionAction Action { get; set; }

    /// <summary>
    /// 權限資源
    /// </summary>
    [Required(ErrorMessage = "權限資源為必填")]
    [StringLength(50, ErrorMessage = "權限資源不能超過50字元")]
    public string Resource { get; set; } = string.Empty;
}

/// <summary>
/// 更新權限 DTO
/// </summary>
public class UpdatePermissionDto
{
    /// <summary>
    /// 權限顯示名稱
    /// </summary>
    [Required(ErrorMessage = "權限顯示名稱為必填")]
    [StringLength(100, ErrorMessage = "權限顯示名稱不能超過100字元")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 權限描述
    /// </summary>
    [StringLength(500, ErrorMessage = "權限描述不能超過500字元")]
    public string? Description { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// 權限回應 DTO
/// </summary>
public class PermissionResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public PermissionAction Action { get; set; }
    public string Resource { get; set; } = string.Empty;
    public bool IsSystemPermission { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

#endregion

#region UserRole DTOs

/// <summary>
/// 分配使用者角色 DTO
/// </summary>
public class AssignUserRoleDto
{
    /// <summary>
    /// 使用者ID
    /// </summary>
    [Required(ErrorMessage = "使用者ID為必填")]
    public int UserId { get; set; }

    /// <summary>
    /// 角色ID列表
    /// </summary>
    [Required(ErrorMessage = "角色ID為必填")]
    public List<int> RoleIds { get; set; } = new List<int>();

    /// <summary>
    /// 主要角色ID (可選)
    /// </summary>
    public int? PrimaryRoleId { get; set; }

    /// <summary>
    /// 分配原因
    /// </summary>
    [StringLength(500, ErrorMessage = "分配原因不能超過500字元")]
    public string? AssignmentReason { get; set; }

    /// <summary>
    /// 角色生效時間
    /// </summary>
    public DateTime? ValidFrom { get; set; }

    /// <summary>
    /// 角色到期時間
    /// </summary>
    public DateTime? ValidTo { get; set; }
}

/// <summary>
/// 使用者角色回應 DTO
/// </summary>
public class UserRoleResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string RoleDisplayName { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? AssignedById { get; set; }
    public string? AssignedByName { get; set; }
    public string? AssignmentReason { get; set; }
    public bool IsExpired => ValidTo.HasValue && ValidTo.Value < DateTime.UtcNow;
}

#endregion

#region Authorization DTOs

/// <summary>
/// 權限檢查請求 DTO
/// </summary>
public class PermissionCheckDto
{
    /// <summary>
    /// 使用者ID
    /// </summary>
    [Required(ErrorMessage = "使用者ID為必填")]
    public int UserId { get; set; }

    /// <summary>
    /// 權限名稱
    /// </summary>
    [Required(ErrorMessage = "權限名稱為必填")]
    public string Permission { get; set; } = string.Empty;

    /// <summary>
    /// 資源ID (可選，用於資源級別的權限檢查)
    /// </summary>
    public int? ResourceId { get; set; }

    /// <summary>
    /// 額外的檢查條件 (JSON格式)
    /// </summary>
    public string? AdditionalConditions { get; set; }
}

/// <summary>
/// 權限檢查回應 DTO
/// </summary>
public class PermissionCheckResponseDto
{
    public bool HasPermission { get; set; }
    public string Permission { get; set; } = string.Empty;
    public List<string> GrantedBy { get; set; } = new List<string>(); // 由哪些角色授予
    public List<string> DeniedReasons { get; set; } = new List<string>(); // 拒絕原因
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 使用者權限摘要 DTO
/// </summary>
public class UserPermissionSummaryDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public List<RoleResponseDto> Roles { get; set; } = new List<RoleResponseDto>();
    public List<PermissionResponseDto> AllPermissions { get; set; } = new List<PermissionResponseDto>();
    public Dictionary<string, List<string>> PermissionsByCategory { get; set; } = new Dictionary<string, List<string>>();
    public DateTime LastUpdated { get; set; }
}

#endregion