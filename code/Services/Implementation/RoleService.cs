using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.DTOs.User;
using PersonalManagerAPI.Models;
using Microsoft.Extensions.Logging;

namespace PersonalManagerAPI.Services;

/// <summary>
/// 角色管理服務實作
/// </summary>
public class RoleService : IRoleService
{
    private readonly JsonDataService _dataService;
    private readonly ILogger<RoleService> _logger;

    public RoleService(JsonDataService dataService, ILogger<RoleService> logger)
    {
        _dataService = dataService;
        _logger = logger;
    }

    public async Task<ServiceResult<IEnumerable<RoleResponseDto>>> GetAllRolesAsync()
    {
        try
        {
            var roles = await _dataService.GetAllAsync<Role>();
            var permissions = await _dataService.GetAllAsync<RolePermission>();
            var userRoles = await _dataService.GetAllAsync<UserRole>();

            var response = roles.Select(r => MapToResponseDto(r, permissions, userRoles));
            return ServiceResult<IEnumerable<RoleResponseDto>>.Success(response, "成功取得角色列表");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得角色列表時發生錯誤");
            return ServiceResult<IEnumerable<RoleResponseDto>>.Failure("取得角色列表失敗", ex.Message);
        }
    }

    public async Task<ServiceResult<RoleResponseDto>> GetRoleByIdAsync(int id)
    {
        try
        {
            var role = await _dataService.GetByIdAsync<Role>(id);
            if (role == null)
            {
                return ServiceResult<RoleResponseDto>.Failure("找不到指定的角色");
            }

            var permissions = await _dataService.GetAllAsync<RolePermission>();
            var userRoles = await _dataService.GetAllAsync<UserRole>();

            var response = MapToResponseDto(role, permissions, userRoles);
            return ServiceResult<RoleResponseDto>.Success(response, "成功取得角色資訊");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得角色 {RoleId} 時發生錯誤", id);
            return ServiceResult<RoleResponseDto>.Failure("取得角色資訊失敗", ex.Message);
        }
    }

    public async Task<ServiceResult<RoleResponseDto>> GetRoleByNameAsync(string name)
    {
        try
        {
            var roles = await _dataService.GetAllAsync<Role>();
            var role = roles.FirstOrDefault(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (role == null)
            {
                return ServiceResult<RoleResponseDto>.Failure("找不到指定的角色");
            }

            var permissions = await _dataService.GetAllAsync<RolePermission>();
            var userRoles = await _dataService.GetAllAsync<UserRole>();

            var response = MapToResponseDto(role, permissions, userRoles);
            return ServiceResult<RoleResponseDto>.Success(response, "成功取得角色資訊");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根據名稱取得角色 {RoleName} 時發生錯誤", name);
            return ServiceResult<RoleResponseDto>.Failure("取得角色資訊失敗", ex.Message);
        }
    }

    public async Task<ServiceResult<RoleResponseDto>> CreateRoleAsync(CreateRoleDto dto, int? createdById = null)
    {
        try
        {
            // 檢查角色名稱是否已存在
            if (await RoleNameExistsAsync(dto.Name))
            {
                return ServiceResult<RoleResponseDto>.Failure("角色名稱已存在");
            }

            var roles = await _dataService.GetAllAsync<Role>();
            var newId = roles.Any() ? roles.Max(r => r.Id) + 1 : 1;

            var role = new Role
            {
                Id = newId,
                Name = dto.Name,
                DisplayName = dto.DisplayName,
                Description = dto.Description,
                Priority = dto.Priority,
                IsActive = dto.IsActive,
                CreatedById = createdById,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _dataService.CreateAsync(role);
            _logger.LogInformation("成功建立角色: {RoleName} (ID: {RoleId})", role.Name, role.Id);

            var permissions = await _dataService.GetAllAsync<RolePermission>();
            var userRoles = await _dataService.GetAllAsync<UserRole>();

            var response = MapToResponseDto(role, permissions, userRoles);
            return ServiceResult<RoleResponseDto>.Success(response, "成功建立角色");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "建立角色時發生錯誤");
            return ServiceResult<RoleResponseDto>.Failure("建立角色失敗", ex.Message);
        }
    }

    public async Task<ServiceResult<RoleResponseDto>> UpdateRoleAsync(int id, UpdateRoleDto dto, int? updatedById = null)
    {
        try
        {
            var role = await _dataService.GetByIdAsync<Role>(id);
            if (role == null)
            {
                return ServiceResult<RoleResponseDto>.Failure("找不到指定的角色");
            }

            // 檢查是否為系統角色
            if (role.IsSystemRole)
            {
                return ServiceResult<RoleResponseDto>.Failure("系統角色無法修改");
            }

            // 更新欄位
            if (!string.IsNullOrEmpty(dto.DisplayName))
                role.DisplayName = dto.DisplayName;
            if (dto.Description != null)
                role.Description = dto.Description;
            if (dto.Priority.HasValue)
                role.Priority = dto.Priority.Value;
            if (dto.IsActive.HasValue)
                role.IsActive = dto.IsActive.Value;

            role.UpdatedById = updatedById;
            role.UpdatedAt = DateTime.UtcNow;

            await _dataService.UpdateAsync(role);
            _logger.LogInformation("成功更新角色: {RoleName} (ID: {RoleId})", role.Name, role.Id);

            var permissions = await _dataService.GetAllAsync<RolePermission>();
            var userRoles = await _dataService.GetAllAsync<UserRole>();

            var response = MapToResponseDto(role, permissions, userRoles);
            return ServiceResult<RoleResponseDto>.Success(response, "成功更新角色");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新角色 {RoleId} 時發生錯誤", id);
            return ServiceResult<RoleResponseDto>.Failure("更新角色失敗", ex.Message);
        }
    }

    public async Task<ServiceResult<bool>> DeleteRoleAsync(int id)
    {
        try
        {
            var role = await _dataService.GetByIdAsync<Role>(id);
            if (role == null)
            {
                return ServiceResult<bool>.Failure("找不到指定的角色");
            }

            // 檢查是否為系統角色
            if (role.IsSystemRole)
            {
                return ServiceResult<bool>.Failure("系統角色無法刪除");
            }

            // 檢查是否有使用者使用此角色
            var userRoles = await _dataService.GetAllAsync<UserRole>();
            if (userRoles.Any(ur => ur.RoleId == id && ur.IsActive))
            {
                return ServiceResult<bool>.Failure("此角色仍有使用者使用，無法刪除");
            }

            // 刪除角色的權限對應
            var rolePermissions = await _dataService.GetAllAsync<RolePermission>();
            var rolePermissionsToDelete = rolePermissions.Where(rp => rp.RoleId == id).ToList();
            foreach (var rp in rolePermissionsToDelete)
            {
                await _dataService.DeleteAsync<RolePermission>(rp.Id);
            }

            // 刪除角色
            await _dataService.DeleteAsync<Role>(id);
            _logger.LogInformation("成功刪除角色: {RoleName} (ID: {RoleId})", role.Name, role.Id);

            return ServiceResult<bool>.Success(true, "成功刪除角色");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刪除角色 {RoleId} 時發生錯誤", id);
            return ServiceResult<bool>.Failure("刪除角色失敗", ex.Message);
        }
    }

    public async Task<ServiceResult<IEnumerable<PermissionResponseDto>>> GetRolePermissionsAsync(int roleId)
    {
        try
        {
            var role = await _dataService.GetByIdAsync<Role>(roleId);
            if (role == null)
            {
                return ServiceResult<IEnumerable<PermissionResponseDto>>.Failure("找不到指定的角色");
            }

            var rolePermissions = await _dataService.GetAllAsync<RolePermission>();
            var permissions = await _dataService.GetAllAsync<Permission>();

            var rolePermissionIds = rolePermissions
                .Where(rp => rp.RoleId == roleId && rp.IsActive)
                .Select(rp => rp.PermissionId)
                .ToList();

            var rolePermissionsList = permissions
                .Where(p => rolePermissionIds.Contains(p.Id))
                .Select(p => new PermissionResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    DisplayName = p.DisplayName,
                    Description = p.Description,
                    Category = p.Category,
                    Action = p.Action,
                    ActionName = p.Action.ToString(),
                    Resource = p.Resource,
                    IsSystemPermission = p.IsSystemPermission,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                });

            return ServiceResult<IEnumerable<PermissionResponseDto>>.Success(rolePermissionsList, "成功取得角色權限列表");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得角色 {RoleId} 權限列表時發生錯誤", roleId);
            return ServiceResult<IEnumerable<PermissionResponseDto>>.Failure("取得角色權限列表失敗", ex.Message);
        }
    }

    public async Task<ServiceResult<bool>> AssignPermissionsToRoleAsync(int roleId, AssignPermissionsDto dto, int? assignedById = null)
    {
        try
        {
            var role = await _dataService.GetByIdAsync<Role>(roleId);
            if (role == null)
            {
                return ServiceResult<bool>.Failure("找不到指定的角色");
            }

            var permissions = await _dataService.GetAllAsync<Permission>();
            var validPermissionIds = permissions.Select(p => p.Id).ToList();

            // 驗證所有權限ID都有效
            var invalidIds = dto.PermissionIds.Except(validPermissionIds).ToList();
            if (invalidIds.Any())
            {
                return ServiceResult<bool>.Failure($"無效的權限ID: {string.Join(", ", invalidIds)}");
            }

            var rolePermissions = await _dataService.GetAllAsync<RolePermission>();
            var existingRolePermissions = rolePermissions.Where(rp => rp.RoleId == roleId).ToList();
            var maxId = rolePermissions.Any() ? rolePermissions.Max(rp => rp.Id) : 0;

            // 為每個權限建立對應關係
            foreach (var permissionId in dto.PermissionIds)
            {
                var existing = existingRolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);

                if (existing != null)
                {
                    // 如果已存在但未啟用，則啟用它
                    if (!existing.IsActive)
                    {
                        existing.IsActive = true;
                        await _dataService.UpdateAsync(existing);
                    }
                }
                else
                {
                    // 建立新的權限對應
                    var rolePermission = new RolePermission
                    {
                        Id = ++maxId,
                        RoleId = roleId,
                        PermissionId = permissionId,
                        IsActive = true,
                        CreatedById = assignedById,
                        CreatedAt = DateTime.UtcNow,
                        Notes = dto.Notes
                    };
                    await _dataService.CreateAsync(rolePermission);
                }
            }

            _logger.LogInformation("成功為角色 {RoleId} 分配 {Count} 個權限", roleId, dto.PermissionIds.Count);
            return ServiceResult<bool>.Success(true, "成功分配權限");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "為角色 {RoleId} 分配權限時發生錯誤", roleId);
            return ServiceResult<bool>.Failure("分配權限失敗", ex.Message);
        }
    }

    public async Task<ServiceResult<bool>> RemovePermissionFromRoleAsync(int roleId, int permissionId)
    {
        try
        {
            var rolePermissions = await _dataService.GetAllAsync<RolePermission>();
            var rolePermission = rolePermissions.FirstOrDefault(rp =>
                rp.RoleId == roleId && rp.PermissionId == permissionId);

            if (rolePermission == null)
            {
                return ServiceResult<bool>.Failure("找不到指定的權限對應");
            }

            await _dataService.DeleteAsync<RolePermission>(rolePermission.Id);
            _logger.LogInformation("成功從角色 {RoleId} 移除權限 {PermissionId}", roleId, permissionId);

            return ServiceResult<bool>.Success(true, "成功移除權限");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "從角色 {RoleId} 移除權限 {PermissionId} 時發生錯誤", roleId, permissionId);
            return ServiceResult<bool>.Failure("移除權限失敗", ex.Message);
        }
    }

    public async Task<ServiceResult<IEnumerable<UserResponseDto>>> GetRoleUsersAsync(int roleId)
    {
        try
        {
            var role = await _dataService.GetByIdAsync<Role>(roleId);
            if (role == null)
            {
                return ServiceResult<IEnumerable<UserResponseDto>>.Failure("找不到指定的角色");
            }

            var userRoles = await _dataService.GetAllAsync<UserRole>();
            var users = await _dataService.GetAllAsync<User>();

            var userIds = userRoles
                .Where(ur => ur.RoleId == roleId && ur.IsActive)
                .Select(ur => ur.UserId)
                .ToList();

            var roleUsers = users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    FullName = u.FullName,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                });

            return ServiceResult<IEnumerable<UserResponseDto>>.Success(roleUsers, "成功取得角色使用者列表");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得角色 {RoleId} 使用者列表時發生錯誤", roleId);
            return ServiceResult<IEnumerable<UserResponseDto>>.Failure("取得角色使用者列表失敗", ex.Message);
        }
    }

    public async Task<bool> RoleNameExistsAsync(string name, int? excludeId = null)
    {
        var roles = await _dataService.GetAllAsync<Role>();
        return roles.Any(r =>
            r.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
            (!excludeId.HasValue || r.Id != excludeId.Value));
    }

    public async Task<ServiceResult<bool>> ToggleRoleStatusAsync(int id, bool isActive)
    {
        try
        {
            var role = await _dataService.GetByIdAsync<Role>(id);
            if (role == null)
            {
                return ServiceResult<bool>.Failure("找不到指定的角色");
            }

            role.IsActive = isActive;
            role.UpdatedAt = DateTime.UtcNow;

            await _dataService.UpdateAsync(role);
            _logger.LogInformation("成功{Action}角色: {RoleName} (ID: {RoleId})",
                isActive ? "啟用" : "停用", role.Name, role.Id);

            return ServiceResult<bool>.Success(true, $"成功{(isActive ? "啟用" : "停用")}角色");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "切換角色 {RoleId} 狀態時發生錯誤", id);
            return ServiceResult<bool>.Failure("切換角色狀態失敗", ex.Message);
        }
    }

    private RoleResponseDto MapToResponseDto(Role role, IEnumerable<RolePermission> allRolePermissions, IEnumerable<UserRole> allUserRoles)
    {
        var permissionCount = allRolePermissions.Count(rp => rp.RoleId == role.Id && rp.IsActive);
        var userCount = allUserRoles.Count(ur => ur.RoleId == role.Id && ur.IsActive);

        return new RoleResponseDto
        {
            Id = role.Id,
            Name = role.Name,
            DisplayName = role.DisplayName,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole,
            Priority = role.Priority,
            IsActive = role.IsActive,
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt,
            CreatedById = role.CreatedById,
            UpdatedById = role.UpdatedById,
            PermissionCount = permissionCount,
            UserCount = userCount
        };
    }
}
