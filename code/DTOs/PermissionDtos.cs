using System.ComponentModel.DataAnnotations;
using PersonalManagerAPI.Models;

namespace PersonalManagerAPI.DTOs;

/// <summary>
/// 建立權限DTO
/// </summary>
public class CreatePermissionDto
{
    /// <summary>
    /// 權限名稱 (唯一標識符)
    /// </summary>
    [Required(ErrorMessage = "權限名稱為必填")]
    [StringLength(100, ErrorMessage = "權限名稱長度不可超過100個字元")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 權限顯示名稱
    /// </summary>
    [Required(ErrorMessage = "權限顯示名稱為必填")]
    [StringLength(100, ErrorMessage = "顯示名稱長度不可超過100個字元")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 權限描述
    /// </summary>
    [StringLength(500, ErrorMessage = "描述長度不可超過500個字元")]
    public string? Description { get; set; }

    /// <summary>
    /// 權限分類/模組
    /// </summary>
    [Required(ErrorMessage = "權限分類為必填")]
    [StringLength(50, ErrorMessage = "分類長度不可超過50個字元")]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// 權限動作類型
    /// </summary>
    [Required(ErrorMessage = "權限動作為必填")]
    public PermissionAction Action { get; set; }

    /// <summary>
    /// 權限資源
    /// </summary>
    [Required(ErrorMessage = "權限資源為必填")]
    [StringLength(50, ErrorMessage = "資源長度不可超過50個字元")]
    public string Resource { get; set; } = string.Empty;

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// 更新權限DTO
/// </summary>
public class UpdatePermissionDto
{
    /// <summary>
    /// 權限顯示名稱
    /// </summary>
    [StringLength(100, ErrorMessage = "顯示名稱長度不可超過100個字元")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// 權限描述
    /// </summary>
    [StringLength(500, ErrorMessage = "描述長度不可超過500個字元")]
    public string? Description { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool? IsActive { get; set; }
}

/// <summary>
/// 權限回應DTO
/// </summary>
public class PermissionResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public PermissionAction Action { get; set; }
    public string ActionName { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public bool IsSystemPermission { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
