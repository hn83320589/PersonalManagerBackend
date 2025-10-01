using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.DTOs;

/// <summary>
/// 建立角色DTO
/// </summary>
public class CreateRoleDto
{
    /// <summary>
    /// 角色名稱 (唯一)
    /// </summary>
    [Required(ErrorMessage = "角色名稱為必填")]
    [StringLength(50, ErrorMessage = "角色名稱長度不可超過50個字元")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 角色顯示名稱
    /// </summary>
    [Required(ErrorMessage = "角色顯示名稱為必填")]
    [StringLength(100, ErrorMessage = "顯示名稱長度不可超過100個字元")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 角色描述
    /// </summary>
    [StringLength(500, ErrorMessage = "描述長度不可超過500個字元")]
    public string? Description { get; set; }

    /// <summary>
    /// 角色優先級
    /// </summary>
    [Range(1, 1000, ErrorMessage = "優先級必須在1-1000之間")]
    public int Priority { get; set; } = 100;

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// 更新角色DTO
/// </summary>
public class UpdateRoleDto
{
    /// <summary>
    /// 角色顯示名稱
    /// </summary>
    [StringLength(100, ErrorMessage = "顯示名稱長度不可超過100個字元")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// 角色描述
    /// </summary>
    [StringLength(500, ErrorMessage = "描述長度不可超過500個字元")]
    public string? Description { get; set; }

    /// <summary>
    /// 角色優先級
    /// </summary>
    [Range(1, 1000, ErrorMessage = "優先級必須在1-1000之間")]
    public int? Priority { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool? IsActive { get; set; }
}

/// <summary>
/// 角色回應DTO
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
    public int PermissionCount { get; set; }
    public int UserCount { get; set; }
}

/// <summary>
/// 分配權限DTO
/// </summary>
public class AssignPermissionsDto
{
    /// <summary>
    /// 權限ID列表
    /// </summary>
    [Required(ErrorMessage = "權限ID列表為必填")]
    public List<int> PermissionIds { get; set; } = new();

    /// <summary>
    /// 備註
    /// </summary>
    [StringLength(500, ErrorMessage = "備註長度不可超過500個字元")]
    public string? Notes { get; set; }
}

/// <summary>
/// 分配角色DTO
/// </summary>
public class AssignRolesToUserDto
{
    /// <summary>
    /// 角色ID列表
    /// </summary>
    [Required(ErrorMessage = "角色ID列表為必填")]
    public List<int> RoleIds { get; set; } = new();

    /// <summary>
    /// 主要角色ID
    /// </summary>
    public int? PrimaryRoleId { get; set; }

    /// <summary>
    /// 分配原因
    /// </summary>
    [StringLength(500, ErrorMessage = "分配原因長度不可超過500個字元")]
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
