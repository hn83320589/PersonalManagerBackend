using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Models;

namespace PersonalManagerAPI.Services;

/// <summary>
/// 權限管理服務介面
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// 取得所有權限
    /// </summary>
    Task<ServiceResult<IEnumerable<PermissionResponseDto>>> GetAllPermissionsAsync();

    /// <summary>
    /// 根據ID取得權限
    /// </summary>
    Task<ServiceResult<PermissionResponseDto>> GetPermissionByIdAsync(int id);

    /// <summary>
    /// 根據名稱取得權限
    /// </summary>
    Task<ServiceResult<PermissionResponseDto>> GetPermissionByNameAsync(string name);

    /// <summary>
    /// 根據分類取得權限
    /// </summary>
    Task<ServiceResult<IEnumerable<PermissionResponseDto>>> GetPermissionsByCategoryAsync(string category);

    /// <summary>
    /// 根據資源取得權限
    /// </summary>
    Task<ServiceResult<IEnumerable<PermissionResponseDto>>> GetPermissionsByResourceAsync(string resource);

    /// <summary>
    /// 建立新權限
    /// </summary>
    Task<ServiceResult<PermissionResponseDto>> CreatePermissionAsync(CreatePermissionDto dto);

    /// <summary>
    /// 更新權限
    /// </summary>
    Task<ServiceResult<PermissionResponseDto>> UpdatePermissionAsync(int id, UpdatePermissionDto dto);

    /// <summary>
    /// 刪除權限
    /// </summary>
    Task<ServiceResult<bool>> DeletePermissionAsync(int id);

    /// <summary>
    /// 檢查權限名稱是否已存在
    /// </summary>
    Task<bool> PermissionNameExistsAsync(string name, int? excludeId = null);

    /// <summary>
    /// 取得所有權限分類
    /// </summary>
    Task<ServiceResult<IEnumerable<string>>> GetAllCategoriesAsync();

    /// <summary>
    /// 取得所有資源類型
    /// </summary>
    Task<ServiceResult<IEnumerable<string>>> GetAllResourcesAsync();

    /// <summary>
    /// 啟用/停用權限
    /// </summary>
    Task<ServiceResult<bool>> TogglePermissionStatusAsync(int id, bool isActive);
}
