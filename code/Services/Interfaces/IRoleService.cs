using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.DTOs.User;

namespace PersonalManagerAPI.Services;

/// <summary>
/// 角色管理服務介面
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// 取得所有角色
    /// </summary>
    Task<ServiceResult<IEnumerable<RoleResponseDto>>> GetAllRolesAsync();

    /// <summary>
    /// 根據ID取得角色
    /// </summary>
    Task<ServiceResult<RoleResponseDto>> GetRoleByIdAsync(int id);

    /// <summary>
    /// 根據名稱取得角色
    /// </summary>
    Task<ServiceResult<RoleResponseDto>> GetRoleByNameAsync(string name);

    /// <summary>
    /// 建立新角色
    /// </summary>
    Task<ServiceResult<RoleResponseDto>> CreateRoleAsync(CreateRoleDto dto, int? createdById = null);

    /// <summary>
    /// 更新角色
    /// </summary>
    Task<ServiceResult<RoleResponseDto>> UpdateRoleAsync(int id, UpdateRoleDto dto, int? updatedById = null);

    /// <summary>
    /// 刪除角色
    /// </summary>
    Task<ServiceResult<bool>> DeleteRoleAsync(int id);

    /// <summary>
    /// 取得角色的權限列表
    /// </summary>
    Task<ServiceResult<IEnumerable<PermissionResponseDto>>> GetRolePermissionsAsync(int roleId);

    /// <summary>
    /// 為角色分配權限
    /// </summary>
    Task<ServiceResult<bool>> AssignPermissionsToRoleAsync(int roleId, AssignPermissionsDto dto, int? assignedById = null);

    /// <summary>
    /// 移除角色的權限
    /// </summary>
    Task<ServiceResult<bool>> RemovePermissionFromRoleAsync(int roleId, int permissionId);

    /// <summary>
    /// 取得角色的所有使用者
    /// </summary>
    Task<ServiceResult<IEnumerable<UserResponseDto>>> GetRoleUsersAsync(int roleId);

    /// <summary>
    /// 檢查角色名稱是否已存在
    /// </summary>
    Task<bool> RoleNameExistsAsync(string name, int? excludeId = null);

    /// <summary>
    /// 啟用/停用角色
    /// </summary>
    Task<ServiceResult<bool>> ToggleRoleStatusAsync(int id, bool isActive);
}
