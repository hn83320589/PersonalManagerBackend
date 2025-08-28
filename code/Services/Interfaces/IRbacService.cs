using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Models;

namespace PersonalManagerAPI.Services.Interfaces;

/// <summary>
/// RBAC 權限控制服務介面
/// </summary>
public interface IRbacService
{
    #region Role Management

    /// <summary>
    /// 建立角色
    /// </summary>
    Task<RoleResponseDto> CreateRoleAsync(CreateRoleDto createRoleDto, int createdById);

    /// <summary>
    /// 更新角色
    /// </summary>
    Task<RoleResponseDto?> UpdateRoleAsync(int roleId, UpdateRoleDto updateRoleDto, int updatedById);

    /// <summary>
    /// 刪除角色
    /// </summary>
    Task<bool> DeleteRoleAsync(int roleId, int deletedById);

    /// <summary>
    /// 取得角色詳情
    /// </summary>
    Task<RoleResponseDto?> GetRoleByIdAsync(int roleId);

    /// <summary>
    /// 取得角色詳情 (依名稱)
    /// </summary>
    Task<RoleResponseDto?> GetRoleByNameAsync(string roleName);

    /// <summary>
    /// 取得所有角色
    /// </summary>
    Task<List<RoleResponseDto>> GetAllRolesAsync(bool includeInactive = false);

    /// <summary>
    /// 搜尋角色
    /// </summary>
    Task<List<RoleResponseDto>> SearchRolesAsync(string? searchTerm = null, bool? isActive = null, int skip = 0, int take = 20);

    /// <summary>
    /// 檢查角色名稱是否已存在
    /// </summary>
    Task<bool> RoleNameExistsAsync(string roleName, int? excludeRoleId = null);

    #endregion

    #region Permission Management

    /// <summary>
    /// 建立權限
    /// </summary>
    Task<PermissionResponseDto> CreatePermissionAsync(CreatePermissionDto createPermissionDto);

    /// <summary>
    /// 更新權限
    /// </summary>
    Task<PermissionResponseDto?> UpdatePermissionAsync(int permissionId, UpdatePermissionDto updatePermissionDto);

    /// <summary>
    /// 刪除權限
    /// </summary>
    Task<bool> DeletePermissionAsync(int permissionId);

    /// <summary>
    /// 取得權限詳情
    /// </summary>
    Task<PermissionResponseDto?> GetPermissionByIdAsync(int permissionId);

    /// <summary>
    /// 取得權限詳情 (依名稱)
    /// </summary>
    Task<PermissionResponseDto?> GetPermissionByNameAsync(string permissionName);

    /// <summary>
    /// 取得所有權限
    /// </summary>
    Task<List<PermissionResponseDto>> GetAllPermissionsAsync(bool includeInactive = false);

    /// <summary>
    /// 依分類取得權限
    /// </summary>
    Task<Dictionary<string, List<PermissionResponseDto>>> GetPermissionsByCategoryAsync(bool includeInactive = false);

    /// <summary>
    /// 搜尋權限
    /// </summary>
    Task<List<PermissionResponseDto>> SearchPermissionsAsync(string? searchTerm = null, string? category = null, PermissionAction? action = null, bool? isActive = null);

    /// <summary>
    /// 檢查權限名稱是否已存在
    /// </summary>
    Task<bool> PermissionNameExistsAsync(string permissionName, int? excludePermissionId = null);

    /// <summary>
    /// 初始化系統預設權限
    /// </summary>
    Task InitializeDefaultPermissionsAsync();

    #endregion

    #region User Role Management

    /// <summary>
    /// 分配使用者角色
    /// </summary>
    Task<List<UserRoleResponseDto>> AssignUserRolesAsync(AssignUserRoleDto assignUserRoleDto, int assignedById);

    /// <summary>
    /// 移除使用者角色
    /// </summary>
    Task<bool> RemoveUserRoleAsync(int userId, int roleId, int updatedById);

    /// <summary>
    /// 移除使用者所有角色
    /// </summary>
    Task<bool> RemoveAllUserRolesAsync(int userId, int updatedById);

    /// <summary>
    /// 更新使用者主要角色
    /// </summary>
    Task<bool> UpdateUserPrimaryRoleAsync(int userId, int roleId, int updatedById);

    /// <summary>
    /// 取得使用者角色
    /// </summary>
    Task<List<UserRoleResponseDto>> GetUserRolesAsync(int userId, bool includeInactive = false);

    /// <summary>
    /// 取得使用者主要角色
    /// </summary>
    Task<RoleResponseDto?> GetUserPrimaryRoleAsync(int userId);

    /// <summary>
    /// 取得角色下的所有使用者
    /// </summary>
    Task<List<UserRoleResponseDto>> GetRoleUsersAsync(int roleId, bool includeInactive = false);

    #endregion

    #region Permission Checking

    /// <summary>
    /// 檢查使用者是否有特定權限
    /// </summary>
    Task<bool> CheckPermissionAsync(int userId, string permissionName);

    /// <summary>
    /// 檢查使用者是否有任一權限
    /// </summary>
    Task<bool> CheckAnyPermissionAsync(int userId, params string[] permissionNames);

    /// <summary>
    /// 檢查使用者是否有所有權限
    /// </summary>
    Task<bool> CheckAllPermissionsAsync(int userId, params string[] permissionNames);

    /// <summary>
    /// 取得使用者權限摘要
    /// </summary>
    Task<UserPermissionSummaryDto> GetUserPermissionSummaryAsync(int userId);

    /// <summary>
    /// 詳細權限檢查 (含原因)
    /// </summary>
    Task<PermissionCheckResponseDto> CheckPermissionDetailedAsync(PermissionCheckDto checkRequest);

    #endregion

    #region Role Permission Management

    /// <summary>
    /// 為角色分配權限
    /// </summary>
    Task<bool> AssignRolePermissionsAsync(int roleId, List<int> permissionIds, int assignedById);

    /// <summary>
    /// 移除角色權限
    /// </summary>
    Task<bool> RemoveRolePermissionAsync(int roleId, int permissionId);

    /// <summary>
    /// 移除角色所有權限
    /// </summary>
    Task<bool> RemoveAllRolePermissionsAsync(int roleId);

    /// <summary>
    /// 取得角色權限
    /// </summary>
    Task<List<PermissionResponseDto>> GetRolePermissionsAsync(int roleId, bool includeInactive = false);

    #endregion

    #region Utility & Statistics

    /// <summary>
    /// 取得角色統計
    /// </summary>
    Task<object> GetRoleStatisticsAsync();

    /// <summary>
    /// 取得權限統計
    /// </summary>
    Task<object> GetPermissionStatisticsAsync();

    /// <summary>
    /// 取得使用者角色統計
    /// </summary>
    Task<object> GetUserRoleStatisticsAsync();

    /// <summary>
    /// 清理過期的使用者角色
    /// </summary>
    Task<int> CleanupExpiredUserRolesAsync();

    /// <summary>
    /// 驗證權限名稱格式
    /// </summary>
    bool IsValidPermissionName(string permissionName);

    /// <summary>
    /// 生成權限名稱
    /// </summary>
    string GeneratePermissionName(string resource, PermissionAction action);

    #endregion
}