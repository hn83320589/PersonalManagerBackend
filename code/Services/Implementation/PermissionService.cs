using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Models;
using Microsoft.Extensions.Logging;

namespace PersonalManagerAPI.Services;

/// <summary>
/// 權限管理服務實作
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly JsonDataService _dataService;
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(JsonDataService dataService, ILogger<PermissionService> logger)
    {
        _dataService = dataService;
        _logger = logger;
    }

    public async Task<ServiceResult<IEnumerable<PermissionResponseDto>>> GetAllPermissionsAsync()
    {
        try
        {
            var permissions = await _dataService.GetAllAsync<Permission>();
            var response = permissions.Select(MapToResponseDto);
            return ServiceResult<IEnumerable<PermissionResponseDto>>.Success(response, "成功取得權限列表");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得權限列表時發生錯誤");
            return ServiceResult<IEnumerable<PermissionResponseDto>>.Failure("取得權限列表失敗", ex.Message);
        }
    }

    public async Task<ServiceResult<PermissionResponseDto>> GetPermissionByIdAsync(int id)
    {
        try
        {
            var permission = await _dataService.GetByIdAsync<Permission>(id);
            if (permission == null)
            {
                return ServiceResult<PermissionResponseDto>.Failure("找不到指定的權限");
            }

            var response = MapToResponseDto(permission);
            return ServiceResult<PermissionResponseDto>.Success(response, "成功取得權限資訊");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得權限 {PermissionId} 時發生錯誤", id);
            return ServiceResult<PermissionResponseDto>.Failure("取得權限資訊失敗", ex.Message);
        }
    }

    public async Task<ServiceResult<PermissionResponseDto>> GetPermissionByNameAsync(string name)
    {
        try
        {
            var permissions = await _dataService.GetAllAsync<Permission>();
            var permission = permissions.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (permission == null)
            {
                return ServiceResult<PermissionResponseDto>.Failure("找不到指定的權限");
            }

            var response = MapToResponseDto(permission);
            return ServiceResult<PermissionResponseDto>.Success(response, "成功取得權限資訊");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根據名稱取得權限 {PermissionName} 時發生錯誤", name);
            return ServiceResult<PermissionResponseDto>.Failure("取得權限資訊失敗", ex.Message);
        }
    }

    public async Task<ServiceResult<IEnumerable<PermissionResponseDto>>> GetPermissionsByCategoryAsync(string category)
    {
        try
        {
            var permissions = await _dataService.GetAllAsync<Permission>();
            var filteredPermissions = permissions
                .Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .Select(MapToResponseDto);

            return ServiceResult<IEnumerable<PermissionResponseDto>>.Success(
                filteredPermissions,
                $"成功取得分類 '{category}' 的權限列表"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得分類 {Category} 的權限時發生錯誤", category);
            return ServiceResult<IEnumerable<PermissionResponseDto>>.Failure("取得權限列表失敗", ex.Message);
        }
    }

    public async Task<ServiceResult<IEnumerable<PermissionResponseDto>>> GetPermissionsByResourceAsync(string resource)
    {
        try
        {
            var permissions = await _dataService.GetAllAsync<Permission>();
            var filteredPermissions = permissions
                .Where(p => p.Resource.Equals(resource, StringComparison.OrdinalIgnoreCase))
                .Select(MapToResponseDto);

            return ServiceResult<IEnumerable<PermissionResponseDto>>.Success(
                filteredPermissions,
                $"成功取得資源 '{resource}' 的權限列表"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得資源 {Resource} 的權限時發生錯誤", resource);
            return ServiceResult<IEnumerable<PermissionResponseDto>>.Failure("取得權限列表失敗", ex.Message);
        }
    }

    public async Task<ServiceResult<PermissionResponseDto>> CreatePermissionAsync(CreatePermissionDto dto)
    {
        try
        {
            // 檢查權限名稱是否已存在
            if (await PermissionNameExistsAsync(dto.Name))
            {
                return ServiceResult<PermissionResponseDto>.Failure("權限名稱已存在");
            }

            var permissions = await _dataService.GetAllAsync<Permission>();
            var newId = permissions.Any() ? permissions.Max(p => p.Id) + 1 : 1;

            var permission = new Permission
            {
                Id = newId,
                Name = dto.Name,
                DisplayName = dto.DisplayName,
                Description = dto.Description,
                Category = dto.Category,
                Action = dto.Action,
                Resource = dto.Resource,
                IsActive = dto.IsActive,
                IsSystemPermission = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _dataService.CreateAsync(permission);
            _logger.LogInformation("成功建立權限: {PermissionName} (ID: {PermissionId})", permission.Name, permission.Id);

            var response = MapToResponseDto(permission);
            return ServiceResult<PermissionResponseDto>.Success(response, "成功建立權限");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "建立權限時發生錯誤");
            return ServiceResult<PermissionResponseDto>.Failure("建立權限失敗", ex.Message);
        }
    }

    public async Task<ServiceResult<PermissionResponseDto>> UpdatePermissionAsync(int id, UpdatePermissionDto dto)
    {
        try
        {
            var permission = await _dataService.GetByIdAsync<Permission>(id);
            if (permission == null)
            {
                return ServiceResult<PermissionResponseDto>.Failure("找不到指定的權限");
            }

            // 檢查是否為系統權限
            if (permission.IsSystemPermission)
            {
                return ServiceResult<PermissionResponseDto>.Failure("系統權限無法修改");
            }

            // 更新欄位
            if (!string.IsNullOrEmpty(dto.DisplayName))
                permission.DisplayName = dto.DisplayName;
            if (dto.Description != null)
                permission.Description = dto.Description;
            if (dto.IsActive.HasValue)
                permission.IsActive = dto.IsActive.Value;

            permission.UpdatedAt = DateTime.UtcNow;

            await _dataService.UpdateAsync(permission);
            _logger.LogInformation("成功更新權限: {PermissionName} (ID: {PermissionId})", permission.Name, permission.Id);

            var response = MapToResponseDto(permission);
            return ServiceResult<PermissionResponseDto>.Success(response, "成功更新權限");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新權限 {PermissionId} 時發生錯誤", id);
            return ServiceResult<PermissionResponseDto>.Failure("更新權限失敗", ex.Message);
        }
    }

    public async Task<ServiceResult<bool>> DeletePermissionAsync(int id)
    {
        try
        {
            var permission = await _dataService.GetByIdAsync<Permission>(id);
            if (permission == null)
            {
                return ServiceResult<bool>.Failure("找不到指定的權限");
            }

            // 檢查是否為系統權限
            if (permission.IsSystemPermission)
            {
                return ServiceResult<bool>.Failure("系統權限無法刪除");
            }

            // 檢查是否有角色使用此權限
            var rolePermissions = await _dataService.GetAllAsync<RolePermission>();
            if (rolePermissions.Any(rp => rp.PermissionId == id && rp.IsActive))
            {
                return ServiceResult<bool>.Failure("此權限仍被角色使用，無法刪除");
            }

            // 刪除權限的角色對應
            var rolePermissionsToDelete = rolePermissions.Where(rp => rp.PermissionId == id).ToList();
            foreach (var rp in rolePermissionsToDelete)
            {
                await _dataService.DeleteAsync<RolePermission>(rp.Id);
            }

            // 刪除權限
            await _dataService.DeleteAsync<Permission>(id);
            _logger.LogInformation("成功刪除權限: {PermissionName} (ID: {PermissionId})", permission.Name, permission.Id);

            return ServiceResult<bool>.Success(true, "成功刪除權限");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刪除權限 {PermissionId} 時發生錯誤", id);
            return ServiceResult<bool>.Failure("刪除權限失敗", ex.Message);
        }
    }

    public async Task<bool> PermissionNameExistsAsync(string name, int? excludeId = null)
    {
        var permissions = await _dataService.GetAllAsync<Permission>();
        return permissions.Any(p =>
            p.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
            (!excludeId.HasValue || p.Id != excludeId.Value));
    }

    public async Task<ServiceResult<IEnumerable<string>>> GetAllCategoriesAsync()
    {
        try
        {
            var permissions = await _dataService.GetAllAsync<Permission>();
            var categories = permissions
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            return ServiceResult<IEnumerable<string>>.Success(categories, "成功取得權限分類列表");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得權限分類列表時發生錯誤");
            return ServiceResult<IEnumerable<string>>.Failure("取得權限分類列表失敗", ex.Message);
        }
    }

    public async Task<ServiceResult<IEnumerable<string>>> GetAllResourcesAsync()
    {
        try
        {
            var permissions = await _dataService.GetAllAsync<Permission>();
            var resources = permissions
                .Select(p => p.Resource)
                .Distinct()
                .OrderBy(r => r)
                .ToList();

            return ServiceResult<IEnumerable<string>>.Success(resources, "成功取得資源類型列表");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得資源類型列表時發生錯誤");
            return ServiceResult<IEnumerable<string>>.Failure("取得資源類型列表失敗", ex.Message);
        }
    }

    public async Task<ServiceResult<bool>> TogglePermissionStatusAsync(int id, bool isActive)
    {
        try
        {
            var permission = await _dataService.GetByIdAsync<Permission>(id);
            if (permission == null)
            {
                return ServiceResult<bool>.Failure("找不到指定的權限");
            }

            permission.IsActive = isActive;
            permission.UpdatedAt = DateTime.UtcNow;

            await _dataService.UpdateAsync(permission);
            _logger.LogInformation("成功{Action}權限: {PermissionName} (ID: {PermissionId})",
                isActive ? "啟用" : "停用", permission.Name, permission.Id);

            return ServiceResult<bool>.Success(true, $"成功{(isActive ? "啟用" : "停用")}權限");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "切換權限 {PermissionId} 狀態時發生錯誤", id);
            return ServiceResult<bool>.Failure("切換權限狀態失敗", ex.Message);
        }
    }

    private PermissionResponseDto MapToResponseDto(Permission permission)
    {
        return new PermissionResponseDto
        {
            Id = permission.Id,
            Name = permission.Name,
            DisplayName = permission.DisplayName,
            Description = permission.Description,
            Category = permission.Category,
            Action = permission.Action,
            ActionName = permission.Action.ToString(),
            Resource = permission.Resource,
            IsSystemPermission = permission.IsSystemPermission,
            IsActive = permission.IsActive,
            CreatedAt = permission.CreatedAt,
            UpdatedAt = permission.UpdatedAt
        };
    }
}
