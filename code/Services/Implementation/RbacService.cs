using AutoMapper;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services.Interfaces;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PersonalManagerAPI.Services.Implementation;

/// <summary>
/// RBAC 權限控制服務實作
/// </summary>
public class RbacService : IRbacService
{
    private readonly JsonDataService _dataService;
    private readonly IMapper _mapper;
    private readonly ILogger<RbacService> _logger;

    public RbacService(JsonDataService dataService, IMapper mapper, ILogger<RbacService> logger)
    {
        _dataService = dataService;
        _mapper = mapper;
        _logger = logger;
    }

    #region Role Management

    public async Task<RoleResponseDto> CreateRoleAsync(CreateRoleDto createRoleDto, int createdById)
    {
        _logger.LogInformation("正在建立角色: {RoleName}", createRoleDto.Name);

        var roles = await GetAllRolesDataAsync();

        // 檢查角色名稱是否已存在
        if (await RoleNameExistsAsync(createRoleDto.Name))
        {
            throw new InvalidOperationException($"角色名稱 '{createRoleDto.Name}' 已存在");
        }

        var role = new Role
        {
            Id = await GetNextRoleIdAsync(),
            Name = createRoleDto.Name,
            DisplayName = createRoleDto.DisplayName,
            Description = createRoleDto.Description,
            Priority = createRoleDto.Priority,
            CreatedById = createdById,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        roles.Add(role);
        await SaveRolesAsync(roles);

        // 如果指定了權限，則分配權限
        if (createRoleDto.PermissionIds.Any())
        {
            await AssignRolePermissionsAsync(role.Id, createRoleDto.PermissionIds, createdById);
        }

        _logger.LogInformation("角色建立成功: {RoleId} - {RoleName}", role.Id, role.Name);

        var response = _mapper.Map<RoleResponseDto>(role);
        response.Permissions = await GetRolePermissionsAsync(role.Id);
        return response;
    }

    public async Task<RoleResponseDto?> UpdateRoleAsync(int roleId, UpdateRoleDto updateRoleDto, int updatedById)
    {
        _logger.LogInformation("正在更新角色: {RoleId}", roleId);

        var roles = await GetAllRolesDataAsync();
        var role = roles.FirstOrDefault(r => r.Id == roleId);

        if (role == null)
        {
            _logger.LogWarning("找不到角色: {RoleId}", roleId);
            return null;
        }

        if (role.IsSystemRole)
        {
            throw new InvalidOperationException("系統預設角色不能修改");
        }

        role.DisplayName = updateRoleDto.DisplayName;
        role.Description = updateRoleDto.Description;
        role.Priority = updateRoleDto.Priority;
        role.IsActive = updateRoleDto.IsActive;
        role.UpdatedById = updatedById;
        role.UpdatedAt = DateTime.UtcNow;

        await SaveRolesAsync(roles);

        // 更新角色權限
        await RemoveAllRolePermissionsAsync(roleId);
        if (updateRoleDto.PermissionIds.Any())
        {
            await AssignRolePermissionsAsync(roleId, updateRoleDto.PermissionIds, updatedById);
        }

        _logger.LogInformation("角色更新成功: {RoleId}", roleId);

        var response = _mapper.Map<RoleResponseDto>(role);
        response.Permissions = await GetRolePermissionsAsync(roleId);
        return response;
    }

    public async Task<bool> DeleteRoleAsync(int roleId, int deletedById)
    {
        _logger.LogInformation("正在刪除角色: {RoleId}", roleId);

        var roles = await GetAllRolesDataAsync();
        var role = roles.FirstOrDefault(r => r.Id == roleId);

        if (role == null)
        {
            _logger.LogWarning("找不到角色: {RoleId}", roleId);
            return false;
        }

        if (role.IsSystemRole)
        {
            throw new InvalidOperationException("系統預設角色不能刪除");
        }

        // 檢查是否有使用者使用此角色
        var userRoles = await GetAllUserRolesDataAsync();
        var hasUsers = userRoles.Any(ur => ur.RoleId == roleId && ur.IsActive);
        if (hasUsers)
        {
            throw new InvalidOperationException("角色正在被使用，無法刪除");
        }

        roles.Remove(role);
        await SaveRolesAsync(roles);

        // 清理相關的角色權限
        await RemoveAllRolePermissionsAsync(roleId);

        _logger.LogInformation("角色刪除成功: {RoleId}", roleId);
        return true;
    }

    public async Task<RoleResponseDto?> GetRoleByIdAsync(int roleId)
    {
        var roles = await GetAllRolesDataAsync();
        var role = roles.FirstOrDefault(r => r.Id == roleId);

        if (role == null)
            return null;

        var response = _mapper.Map<RoleResponseDto>(role);
        response.Permissions = await GetRolePermissionsAsync(roleId);
        response.UserCount = await GetRoleUserCountAsync(roleId);
        response.PermissionCount = response.Permissions.Count;

        return response;
    }

    public async Task<RoleResponseDto?> GetRoleByNameAsync(string roleName)
    {
        var roles = await GetAllRolesDataAsync();
        var role = roles.FirstOrDefault(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));

        if (role == null)
            return null;

        var response = _mapper.Map<RoleResponseDto>(role);
        response.Permissions = await GetRolePermissionsAsync(role.Id);
        response.UserCount = await GetRoleUserCountAsync(role.Id);
        response.PermissionCount = response.Permissions.Count;

        return response;
    }

    public async Task<List<RoleResponseDto>> GetAllRolesAsync(bool includeInactive = false)
    {
        var roles = await GetAllRolesDataAsync();
        
        if (!includeInactive)
        {
            roles = roles.Where(r => r.IsActive).ToList();
        }

        var response = new List<RoleResponseDto>();
        foreach (var role in roles.OrderBy(r => r.Priority).ThenBy(r => r.Name))
        {
            var roleDto = _mapper.Map<RoleResponseDto>(role);
            roleDto.UserCount = await GetRoleUserCountAsync(role.Id);
            roleDto.PermissionCount = (await GetRolePermissionsAsync(role.Id)).Count;
            response.Add(roleDto);
        }

        return response;
    }

    public async Task<List<RoleResponseDto>> SearchRolesAsync(string? searchTerm = null, bool? isActive = null, int skip = 0, int take = 20)
    {
        var roles = await GetAllRolesDataAsync();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            roles = roles.Where(r => 
                r.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                r.DisplayName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                (r.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true)
            ).ToList();
        }

        if (isActive.HasValue)
        {
            roles = roles.Where(r => r.IsActive == isActive.Value).ToList();
        }

        var pagedRoles = roles
            .OrderBy(r => r.Priority)
            .ThenBy(r => r.Name)
            .Skip(skip)
            .Take(take)
            .ToList();

        var response = new List<RoleResponseDto>();
        foreach (var role in pagedRoles)
        {
            var roleDto = _mapper.Map<RoleResponseDto>(role);
            roleDto.UserCount = await GetRoleUserCountAsync(role.Id);
            roleDto.PermissionCount = (await GetRolePermissionsAsync(role.Id)).Count;
            response.Add(roleDto);
        }

        return response;
    }

    public async Task<bool> RoleNameExistsAsync(string roleName, int? excludeRoleId = null)
    {
        var roles = await GetAllRolesDataAsync();
        return roles.Any(r => 
            r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase) && 
            (!excludeRoleId.HasValue || r.Id != excludeRoleId.Value));
    }

    #endregion

    #region Permission Management

    public async Task<PermissionResponseDto> CreatePermissionAsync(CreatePermissionDto createPermissionDto)
    {
        _logger.LogInformation("正在建立權限: {PermissionName}", createPermissionDto.Name);

        var permissions = await GetAllPermissionsDataAsync();

        // 檢查權限名稱是否已存在
        if (await PermissionNameExistsAsync(createPermissionDto.Name))
        {
            throw new InvalidOperationException($"權限名稱 '{createPermissionDto.Name}' 已存在");
        }

        var permission = new Permission
        {
            Id = await GetNextPermissionIdAsync(),
            Name = createPermissionDto.Name,
            DisplayName = createPermissionDto.DisplayName,
            Description = createPermissionDto.Description,
            Category = createPermissionDto.Category,
            Action = createPermissionDto.Action,
            Resource = createPermissionDto.Resource,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        permissions.Add(permission);
        await SavePermissionsAsync(permissions);

        _logger.LogInformation("權限建立成功: {PermissionId} - {PermissionName}", permission.Id, permission.Name);

        return _mapper.Map<PermissionResponseDto>(permission);
    }

    public async Task<PermissionResponseDto?> UpdatePermissionAsync(int permissionId, UpdatePermissionDto updatePermissionDto)
    {
        _logger.LogInformation("正在更新權限: {PermissionId}", permissionId);

        var permissions = await GetAllPermissionsDataAsync();
        var permission = permissions.FirstOrDefault(p => p.Id == permissionId);

        if (permission == null)
        {
            _logger.LogWarning("找不到權限: {PermissionId}", permissionId);
            return null;
        }

        if (permission.IsSystemPermission)
        {
            throw new InvalidOperationException("系統預設權限不能修改");
        }

        permission.DisplayName = updatePermissionDto.DisplayName;
        permission.Description = updatePermissionDto.Description;
        permission.IsActive = updatePermissionDto.IsActive;
        permission.UpdatedAt = DateTime.UtcNow;

        await SavePermissionsAsync(permissions);

        _logger.LogInformation("權限更新成功: {PermissionId}", permissionId);

        return _mapper.Map<PermissionResponseDto>(permission);
    }

    public async Task<bool> DeletePermissionAsync(int permissionId)
    {
        _logger.LogInformation("正在刪除權限: {PermissionId}", permissionId);

        var permissions = await GetAllPermissionsDataAsync();
        var permission = permissions.FirstOrDefault(p => p.Id == permissionId);

        if (permission == null)
        {
            _logger.LogWarning("找不到權限: {PermissionId}", permissionId);
            return false;
        }

        if (permission.IsSystemPermission)
        {
            throw new InvalidOperationException("系統預設權限不能刪除");
        }

        // 檢查是否有角色使用此權限
        var rolePermissions = await GetAllRolePermissionsDataAsync();
        var hasRoles = rolePermissions.Any(rp => rp.PermissionId == permissionId && rp.IsActive);
        if (hasRoles)
        {
            throw new InvalidOperationException("權限正在被使用，無法刪除");
        }

        permissions.Remove(permission);
        await SavePermissionsAsync(permissions);

        _logger.LogInformation("權限刪除成功: {PermissionId}", permissionId);
        return true;
    }

    public async Task<PermissionResponseDto?> GetPermissionByIdAsync(int permissionId)
    {
        var permissions = await GetAllPermissionsDataAsync();
        var permission = permissions.FirstOrDefault(p => p.Id == permissionId);

        return permission != null ? _mapper.Map<PermissionResponseDto>(permission) : null;
    }

    public async Task<PermissionResponseDto?> GetPermissionByNameAsync(string permissionName)
    {
        var permissions = await GetAllPermissionsDataAsync();
        var permission = permissions.FirstOrDefault(p => p.Name.Equals(permissionName, StringComparison.OrdinalIgnoreCase));

        return permission != null ? _mapper.Map<PermissionResponseDto>(permission) : null;
    }

    public async Task<List<PermissionResponseDto>> GetAllPermissionsAsync(bool includeInactive = false)
    {
        var permissions = await GetAllPermissionsDataAsync();

        if (!includeInactive)
        {
            permissions = permissions.Where(p => p.IsActive).ToList();
        }

        return permissions
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Resource)
            .ThenBy(p => p.Action)
            .Select(p => _mapper.Map<PermissionResponseDto>(p))
            .ToList();
    }

    public async Task<Dictionary<string, List<PermissionResponseDto>>> GetPermissionsByCategoryAsync(bool includeInactive = false)
    {
        var permissions = await GetAllPermissionsAsync(includeInactive);

        return permissions
            .GroupBy(p => p.Category)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(p => p.Resource).ThenBy(p => p.Action).ToList()
            );
    }

    public async Task<List<PermissionResponseDto>> SearchPermissionsAsync(string? searchTerm = null, string? category = null, PermissionAction? action = null, bool? isActive = null)
    {
        var permissions = await GetAllPermissionsDataAsync();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            permissions = permissions.Where(p =>
                p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.DisplayName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.Resource.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                (p.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true)
            ).ToList();
        }

        if (!string.IsNullOrEmpty(category))
        {
            permissions = permissions.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (action.HasValue)
        {
            permissions = permissions.Where(p => p.Action == action.Value).ToList();
        }

        if (isActive.HasValue)
        {
            permissions = permissions.Where(p => p.IsActive == isActive.Value).ToList();
        }

        return permissions
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Resource)
            .ThenBy(p => p.Action)
            .Select(p => _mapper.Map<PermissionResponseDto>(p))
            .ToList();
    }

    public async Task<bool> PermissionNameExistsAsync(string permissionName, int? excludePermissionId = null)
    {
        var permissions = await GetAllPermissionsDataAsync();
        return permissions.Any(p =>
            p.Name.Equals(permissionName, StringComparison.OrdinalIgnoreCase) &&
            (!excludePermissionId.HasValue || p.Id != excludePermissionId.Value));
    }

    public async Task InitializeDefaultPermissionsAsync()
    {
        _logger.LogInformation("正在初始化系統預設權限");

        var existingPermissions = await GetAllPermissionsDataAsync();
        var permissionsToAdd = new List<Permission>();

        var defaultPermissions = GetDefaultSystemPermissions();

        foreach (var defaultPerm in defaultPermissions)
        {
            if (!existingPermissions.Any(p => p.Name == defaultPerm.Name))
            {
                defaultPerm.Id = await GetNextPermissionIdAsync();
                permissionsToAdd.Add(defaultPerm);
            }
        }

        if (permissionsToAdd.Any())
        {
            existingPermissions.AddRange(permissionsToAdd);
            await SavePermissionsAsync(existingPermissions);
            _logger.LogInformation("已新增 {Count} 個預設權限", permissionsToAdd.Count);
        }
        else
        {
            _logger.LogInformation("所有預設權限已存在，無需新增");
        }
    }

    #endregion

    #region User Role Management

    public async Task<List<UserRoleResponseDto>> AssignUserRolesAsync(AssignUserRoleDto assignUserRoleDto, int assignedById)
    {
        _logger.LogInformation("正在為使用者 {UserId} 分配角色", assignUserRoleDto.UserId);

        var userRoles = await GetAllUserRolesDataAsync();
        var result = new List<UserRoleResponseDto>();

        // 移除現有的角色 (可選)
        var existingUserRoles = userRoles.Where(ur => ur.UserId == assignUserRoleDto.UserId && ur.IsActive).ToList();
        foreach (var existingRole in existingUserRoles)
        {
            existingRole.IsActive = false;
            existingRole.UpdatedAt = DateTime.UtcNow;
            existingRole.UpdatedById = assignedById;
        }

        // 分配新角色
        foreach (var roleId in assignUserRoleDto.RoleIds)
        {
            var userRole = new UserRole
            {
                Id = await GetNextUserRoleIdAsync(),
                UserId = assignUserRoleDto.UserId,
                RoleId = roleId,
                IsPrimary = assignUserRoleDto.PrimaryRoleId == roleId,
                ValidFrom = assignUserRoleDto.ValidFrom,
                ValidTo = assignUserRoleDto.ValidTo,
                AssignedById = assignedById,
                AssignmentReason = assignUserRoleDto.AssignmentReason,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            userRoles.Add(userRole);
        }

        await SaveUserRolesAsync(userRoles);

        // 取得回應資料
        foreach (var roleId in assignUserRoleDto.RoleIds)
        {
            var userRoleResponse = await GetUserRoleResponseAsync(assignUserRoleDto.UserId, roleId);
            if (userRoleResponse != null)
            {
                result.Add(userRoleResponse);
            }
        }

        _logger.LogInformation("使用者角色分配完成: UserId={UserId}, RoleCount={Count}", assignUserRoleDto.UserId, assignUserRoleDto.RoleIds.Count);
        return result;
    }

    public async Task<bool> RemoveUserRoleAsync(int userId, int roleId, int updatedById)
    {
        _logger.LogInformation("正在移除使用者角色: UserId={UserId}, RoleId={RoleId}", userId, roleId);

        var userRoles = await GetAllUserRolesDataAsync();
        var userRole = userRoles.FirstOrDefault(ur => ur.UserId == userId && ur.RoleId == roleId && ur.IsActive);

        if (userRole == null)
        {
            _logger.LogWarning("找不到使用者角色: UserId={UserId}, RoleId={RoleId}", userId, roleId);
            return false;
        }

        userRole.IsActive = false;
        userRole.UpdatedById = updatedById;
        userRole.UpdatedAt = DateTime.UtcNow;

        await SaveUserRolesAsync(userRoles);

        _logger.LogInformation("使用者角色移除成功: UserId={UserId}, RoleId={RoleId}", userId, roleId);
        return true;
    }

    public async Task<bool> RemoveAllUserRolesAsync(int userId, int updatedById)
    {
        _logger.LogInformation("正在移除使用者所有角色: UserId={UserId}", userId);

        var userRoles = await GetAllUserRolesDataAsync();
        var activeUserRoles = userRoles.Where(ur => ur.UserId == userId && ur.IsActive).ToList();

        foreach (var userRole in activeUserRoles)
        {
            userRole.IsActive = false;
            userRole.UpdatedById = updatedById;
            userRole.UpdatedAt = DateTime.UtcNow;
        }

        await SaveUserRolesAsync(userRoles);

        _logger.LogInformation("使用者所有角色移除完成: UserId={UserId}, Count={Count}", userId, activeUserRoles.Count);
        return activeUserRoles.Any();
    }

    public async Task<bool> UpdateUserPrimaryRoleAsync(int userId, int roleId, int updatedById)
    {
        _logger.LogInformation("正在更新使用者主要角色: UserId={UserId}, RoleId={RoleId}", userId, roleId);

        var userRoles = await GetAllUserRolesDataAsync();
        var userActiveRoles = userRoles.Where(ur => ur.UserId == userId && ur.IsActive).ToList();

        // 檢查使用者是否有此角色
        var targetRole = userActiveRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (targetRole == null)
        {
            _logger.LogWarning("使用者沒有此角色，無法設為主要角色: UserId={UserId}, RoleId={RoleId}", userId, roleId);
            return false;
        }

        // 重設所有角色的主要狀態
        foreach (var userRole in userActiveRoles)
        {
            userRole.IsPrimary = userRole.RoleId == roleId;
            userRole.UpdatedById = updatedById;
            userRole.UpdatedAt = DateTime.UtcNow;
        }

        await SaveUserRolesAsync(userRoles);

        _logger.LogInformation("使用者主要角色更新完成: UserId={UserId}, RoleId={RoleId}", userId, roleId);
        return true;
    }

    public async Task<List<UserRoleResponseDto>> GetUserRolesAsync(int userId, bool includeInactive = false)
    {
        var userRoles = await GetAllUserRolesDataAsync();
        var users = await _dataService.GetAllAsync<User>();
        var roles = await GetAllRolesDataAsync();

        var query = userRoles.Where(ur => ur.UserId == userId);

        if (!includeInactive)
        {
            query = query.Where(ur => ur.IsActive);
        }

        var result = new List<UserRoleResponseDto>();

        foreach (var userRole in query.OrderBy(ur => ur.IsPrimary ? 0 : 1).ThenBy(ur => ur.CreatedAt))
        {
            var user = users.FirstOrDefault(u => u.Id == userRole.UserId);
            var role = roles.FirstOrDefault(r => r.Id == userRole.RoleId);
            var assignedBy = users.FirstOrDefault(u => u.Id == userRole.AssignedById);

            if (user != null && role != null)
            {
                result.Add(new UserRoleResponseDto
                {
                    Id = userRole.Id,
                    UserId = userRole.UserId,
                    Username = user.Username,
                    UserEmail = user.Email,
                    UserFullName = user.FullName ?? $"{user.FirstName} {user.LastName}".Trim(),
                    RoleId = userRole.RoleId,
                    RoleName = role.Name,
                    RoleDisplayName = role.DisplayName,
                    IsPrimary = userRole.IsPrimary,
                    IsActive = userRole.IsActive,
                    ValidFrom = userRole.ValidFrom,
                    ValidTo = userRole.ValidTo,
                    CreatedAt = userRole.CreatedAt,
                    AssignedById = userRole.AssignedById,
                    AssignedByName = assignedBy?.FullName ?? assignedBy?.Username,
                    AssignmentReason = userRole.AssignmentReason
                });
            }
        }

        return result;
    }

    public async Task<RoleResponseDto?> GetUserPrimaryRoleAsync(int userId)
    {
        var userRoles = await GetAllUserRolesDataAsync();
        var primaryUserRole = userRoles.FirstOrDefault(ur => 
            ur.UserId == userId && ur.IsActive && ur.IsPrimary);

        if (primaryUserRole == null)
        {
            // 如果沒有設定主要角色，取第一個活躍角色
            primaryUserRole = userRoles.FirstOrDefault(ur => 
                ur.UserId == userId && ur.IsActive);
        }

        if (primaryUserRole == null)
            return null;

        return await GetRoleByIdAsync(primaryUserRole.RoleId);
    }

    public async Task<List<UserRoleResponseDto>> GetRoleUsersAsync(int roleId, bool includeInactive = false)
    {
        var userRoles = await GetAllUserRolesDataAsync();
        var users = await _dataService.GetAllAsync<User>();
        var roles = await GetAllRolesDataAsync();

        var query = userRoles.Where(ur => ur.RoleId == roleId);

        if (!includeInactive)
        {
            query = query.Where(ur => ur.IsActive);
        }

        var result = new List<UserRoleResponseDto>();

        foreach (var userRole in query.OrderBy(ur => ur.CreatedAt))
        {
            var user = users.FirstOrDefault(u => u.Id == userRole.UserId);
            var role = roles.FirstOrDefault(r => r.Id == userRole.RoleId);
            var assignedBy = users.FirstOrDefault(u => u.Id == userRole.AssignedById);

            if (user != null && role != null)
            {
                result.Add(new UserRoleResponseDto
                {
                    Id = userRole.Id,
                    UserId = userRole.UserId,
                    Username = user.Username,
                    UserEmail = user.Email,
                    UserFullName = user.FullName ?? $"{user.FirstName} {user.LastName}".Trim(),
                    RoleId = userRole.RoleId,
                    RoleName = role.Name,
                    RoleDisplayName = role.DisplayName,
                    IsPrimary = userRole.IsPrimary,
                    IsActive = userRole.IsActive,
                    ValidFrom = userRole.ValidFrom,
                    ValidTo = userRole.ValidTo,
                    CreatedAt = userRole.CreatedAt,
                    AssignedById = userRole.AssignedById,
                    AssignedByName = assignedBy?.FullName ?? assignedBy?.Username,
                    AssignmentReason = userRole.AssignmentReason
                });
            }
        }

        return result;
    }

    #endregion

    #region Permission Checking

    public async Task<bool> CheckPermissionAsync(int userId, string permissionName)
    {
        try
        {
            var userPermissions = await GetUserAllPermissionsAsync(userId);
            return userPermissions.Any(p => p.Name.Equals(permissionName, StringComparison.OrdinalIgnoreCase) && p.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "檢查使用者權限時發生錯誤: UserId={UserId}, Permission={Permission}", userId, permissionName);
            return false;
        }
    }

    public async Task<bool> CheckAnyPermissionAsync(int userId, params string[] permissionNames)
    {
        try
        {
            var userPermissions = await GetUserAllPermissionsAsync(userId);
            var userPermissionNames = userPermissions.Where(p => p.IsActive).Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

            return permissionNames.Any(perm => userPermissionNames.Contains(perm));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "檢查使用者任一權限時發生錯誤: UserId={UserId}", userId);
            return false;
        }
    }

    public async Task<bool> CheckAllPermissionsAsync(int userId, params string[] permissionNames)
    {
        try
        {
            var userPermissions = await GetUserAllPermissionsAsync(userId);
            var userPermissionNames = userPermissions.Where(p => p.IsActive).Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

            return permissionNames.All(perm => userPermissionNames.Contains(perm));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "檢查使用者所有權限時發生錯誤: UserId={UserId}", userId);
            return false;
        }
    }

    public async Task<UserPermissionSummaryDto> GetUserPermissionSummaryAsync(int userId)
    {
        var users = await _dataService.GetAllAsync<User>();
        var user = users.FirstOrDefault(u => u.Id == userId);

        if (user == null)
        {
            throw new ArgumentException($"找不到使用者: {userId}");
        }

        var userRoles = await GetUserRolesAsync(userId);
        var roles = new List<RoleResponseDto>();
        var allPermissions = new List<PermissionResponseDto>();

        foreach (var userRole in userRoles.Where(ur => ur.IsActive))
        {
            var role = await GetRoleByIdAsync(userRole.RoleId);
            if (role != null)
            {
                roles.Add(role);
                allPermissions.AddRange(role.Permissions);
            }
        }

        // 去除重複的權限
        var uniquePermissions = allPermissions
            .GroupBy(p => p.Id)
            .Select(g => g.First())
            .Where(p => p.IsActive)
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToList();

        // 按分類分組權限
        var permissionsByCategory = uniquePermissions
            .GroupBy(p => p.Category)
            .ToDictionary(
                g => g.Key,
                g => g.Select(p => p.Name).ToList()
            );

        return new UserPermissionSummaryDto
        {
            UserId = userId,
            Username = user.Username,
            Roles = roles,
            AllPermissions = uniquePermissions,
            PermissionsByCategory = permissionsByCategory,
            LastUpdated = DateTime.UtcNow
        };
    }

    public async Task<PermissionCheckResponseDto> CheckPermissionDetailedAsync(PermissionCheckDto checkRequest)
    {
        var response = new PermissionCheckResponseDto
        {
            Permission = checkRequest.Permission,
            CheckedAt = DateTime.UtcNow
        };

        try
        {
            var userRoles = await GetUserRolesAsync(checkRequest.UserId);
            var activeRoles = userRoles.Where(ur => ur.IsActive).ToList();

            if (!activeRoles.Any())
            {
                response.HasPermission = false;
                response.DeniedReasons.Add("使用者沒有分配任何角色");
                return response;
            }

            foreach (var userRole in activeRoles)
            {
                var rolePermissions = await GetRolePermissionsAsync(userRole.RoleId);
                var hasPermission = rolePermissions.Any(p => 
                    p.Name.Equals(checkRequest.Permission, StringComparison.OrdinalIgnoreCase) && p.IsActive);

                if (hasPermission)
                {
                    response.HasPermission = true;
                    response.GrantedBy.Add(userRole.RoleDisplayName);
                }
            }

            if (!response.HasPermission)
            {
                response.DeniedReasons.Add($"使用者的角色 ({string.Join(", ", activeRoles.Select(r => r.RoleDisplayName))}) 都沒有權限: {checkRequest.Permission}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "詳細權限檢查時發生錯誤: {UserId}, {Permission}", checkRequest.UserId, checkRequest.Permission);
            response.HasPermission = false;
            response.DeniedReasons.Add($"權限檢查過程發生錯誤: {ex.Message}");
        }

        return response;
    }

    #endregion

    #region Role Permission Management

    public async Task<bool> AssignRolePermissionsAsync(int roleId, List<int> permissionIds, int assignedById)
    {
        _logger.LogInformation("正在為角色 {RoleId} 分配權限", roleId);

        var rolePermissions = await GetAllRolePermissionsDataAsync();

        // 移除現有權限
        var existingPermissions = rolePermissions.Where(rp => rp.RoleId == roleId && rp.IsActive).ToList();
        foreach (var existingPermission in existingPermissions)
        {
            existingPermission.IsActive = false;
        }

        // 新增權限
        foreach (var permissionId in permissionIds)
        {
            var rolePermission = new RolePermission
            {
                Id = await GetNextRolePermissionIdAsync(),
                RoleId = roleId,
                PermissionId = permissionId,
                CreatedById = assignedById,
                CreatedAt = DateTime.UtcNow
            };

            rolePermissions.Add(rolePermission);
        }

        await SaveRolePermissionsAsync(rolePermissions);

        _logger.LogInformation("角色權限分配完成: RoleId={RoleId}, PermissionCount={Count}", roleId, permissionIds.Count);
        return true;
    }

    public async Task<bool> RemoveRolePermissionAsync(int roleId, int permissionId)
    {
        _logger.LogInformation("正在移除角色權限: RoleId={RoleId}, PermissionId={PermissionId}", roleId, permissionId);

        var rolePermissions = await GetAllRolePermissionsDataAsync();
        var rolePermission = rolePermissions.FirstOrDefault(rp => 
            rp.RoleId == roleId && rp.PermissionId == permissionId && rp.IsActive);

        if (rolePermission == null)
        {
            _logger.LogWarning("找不到角色權限: RoleId={RoleId}, PermissionId={PermissionId}", roleId, permissionId);
            return false;
        }

        rolePermission.IsActive = false;
        await SaveRolePermissionsAsync(rolePermissions);

        _logger.LogInformation("角色權限移除成功: RoleId={RoleId}, PermissionId={PermissionId}", roleId, permissionId);
        return true;
    }

    public async Task<bool> RemoveAllRolePermissionsAsync(int roleId)
    {
        _logger.LogInformation("正在移除角色所有權限: RoleId={RoleId}", roleId);

        var rolePermissions = await GetAllRolePermissionsDataAsync();
        var activePermissions = rolePermissions.Where(rp => rp.RoleId == roleId && rp.IsActive).ToList();

        foreach (var permission in activePermissions)
        {
            permission.IsActive = false;
        }

        await SaveRolePermissionsAsync(rolePermissions);

        _logger.LogInformation("角色所有權限移除完成: RoleId={RoleId}, Count={Count}", roleId, activePermissions.Count);
        return activePermissions.Any();
    }

    public async Task<List<PermissionResponseDto>> GetRolePermissionsAsync(int roleId, bool includeInactive = false)
    {
        var rolePermissions = await GetAllRolePermissionsDataAsync();
        var permissions = await GetAllPermissionsDataAsync();

        var query = rolePermissions.Where(rp => rp.RoleId == roleId);

        if (!includeInactive)
        {
            query = query.Where(rp => rp.IsActive);
        }

        var result = new List<PermissionResponseDto>();

        foreach (var rolePermission in query)
        {
            var permission = permissions.FirstOrDefault(p => p.Id == rolePermission.PermissionId);
            if (permission != null && (includeInactive || permission.IsActive))
            {
                result.Add(_mapper.Map<PermissionResponseDto>(permission));
            }
        }

        return result.OrderBy(p => p.Category).ThenBy(p => p.Name).ToList();
    }

    #endregion

    #region Utility & Statistics

    public async Task<object> GetRoleStatisticsAsync()
    {
        var roles = await GetAllRolesDataAsync();
        var userRoles = await GetAllUserRolesDataAsync();

        return new
        {
            TotalRoles = roles.Count,
            ActiveRoles = roles.Count(r => r.IsActive),
            SystemRoles = roles.Count(r => r.IsSystemRole),
            RoleUsageCounts = roles.Select(r => new
            {
                RoleId = r.Id,
                RoleName = r.Name,
                UserCount = userRoles.Count(ur => ur.RoleId == r.Id && ur.IsActive)
            }).OrderByDescending(x => x.UserCount).ToList()
        };
    }

    public async Task<object> GetPermissionStatisticsAsync()
    {
        var permissions = await GetAllPermissionsDataAsync();
        var rolePermissions = await GetAllRolePermissionsDataAsync();

        var permissionsByCategory = permissions
            .GroupBy(p => p.Category)
            .ToDictionary(g => g.Key, g => g.Count());

        var permissionsByAction = permissions
            .GroupBy(p => p.Action)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        return new
        {
            TotalPermissions = permissions.Count,
            ActivePermissions = permissions.Count(p => p.IsActive),
            SystemPermissions = permissions.Count(p => p.IsSystemPermission),
            PermissionsByCategory = permissionsByCategory,
            PermissionsByAction = permissionsByAction,
            PermissionUsageCounts = permissions.Select(p => new
            {
                PermissionId = p.Id,
                PermissionName = p.Name,
                RoleCount = rolePermissions.Count(rp => rp.PermissionId == p.Id && rp.IsActive)
            }).OrderByDescending(x => x.RoleCount).Take(10).ToList()
        };
    }

    public async Task<object> GetUserRoleStatisticsAsync()
    {
        var userRoles = await GetAllUserRolesDataAsync();
        var users = await _dataService.GetAllAsync<User>();
        var roles = await GetAllRolesDataAsync();

        var activeUserRoles = userRoles.Where(ur => ur.IsActive).ToList();

        return new
        {
            TotalUserRoles = userRoles.Count,
            ActiveUserRoles = activeUserRoles.Count,
            UsersWithRoles = activeUserRoles.Select(ur => ur.UserId).Distinct().Count(),
            UsersWithoutRoles = users.Count(u => u.IsActive) - activeUserRoles.Select(ur => ur.UserId).Distinct().Count(),
            AverageRolesPerUser = activeUserRoles.GroupBy(ur => ur.UserId).Average(g => g.Count()),
            ExpiredRoles = userRoles.Count(ur => ur.ValidTo.HasValue && ur.ValidTo.Value < DateTime.UtcNow),
            RoleDistribution = roles.Select(r => new
            {
                RoleId = r.Id,
                RoleName = r.Name,
                UserCount = activeUserRoles.Count(ur => ur.RoleId == r.Id)
            }).OrderByDescending(x => x.UserCount).ToList()
        };
    }

    public async Task<int> CleanupExpiredUserRolesAsync()
    {
        _logger.LogInformation("正在清理過期的使用者角色");

        var userRoles = await GetAllUserRolesDataAsync();
        var expiredRoles = userRoles.Where(ur => 
            ur.IsActive && 
            ur.ValidTo.HasValue && 
            ur.ValidTo.Value < DateTime.UtcNow).ToList();

        foreach (var expiredRole in expiredRoles)
        {
            expiredRole.IsActive = false;
            expiredRole.UpdatedAt = DateTime.UtcNow;
        }

        if (expiredRoles.Any())
        {
            await SaveUserRolesAsync(userRoles);
        }

        _logger.LogInformation("清理過期使用者角色完成: {Count} 個角色已清理", expiredRoles.Count);
        return expiredRoles.Count;
    }

    public bool IsValidPermissionName(string permissionName)
    {
        if (string.IsNullOrWhiteSpace(permissionName))
            return false;

        // 權限名稱格式: resource.action (例如: users.create, posts.read)
        var pattern = @"^[a-z]+\.[a-z]+$";
        return Regex.IsMatch(permissionName, pattern);
    }

    public string GeneratePermissionName(string resource, PermissionAction action)
    {
        return $"{resource.ToLower()}.{action.ToString().ToLower()}";
    }

    #endregion

    #region Private Helper Methods

    private async Task<List<Role>> GetAllRolesDataAsync()
    {
        return await _dataService.ReadJsonAsync<Role>("roles.json");
    }

    private async Task SaveRolesAsync(List<Role> roles)
    {
        await _dataService.WriteJsonAsync("roles.json", roles);
    }

    private async Task<List<Permission>> GetAllPermissionsDataAsync()
    {
        return await _dataService.ReadJsonAsync<Permission>("permissions.json");
    }

    private async Task SavePermissionsAsync(List<Permission> permissions)
    {
        await _dataService.WriteJsonAsync("permissions.json", permissions);
    }

    private async Task<List<RolePermission>> GetAllRolePermissionsDataAsync()
    {
        return await _dataService.ReadJsonAsync<RolePermission>("rolePermissions.json");
    }

    private async Task SaveRolePermissionsAsync(List<RolePermission> rolePermissions)
    {
        await _dataService.WriteJsonAsync("rolePermissions.json", rolePermissions);
    }

    private async Task<List<UserRole>> GetAllUserRolesDataAsync()
    {
        return await _dataService.ReadJsonAsync<UserRole>("userRoles.json");
    }

    private async Task SaveUserRolesAsync(List<UserRole> userRoles)
    {
        await _dataService.WriteJsonAsync("userRoles.json", userRoles);
    }

    private async Task<int> GetNextRoleIdAsync()
    {
        var roles = await GetAllRolesDataAsync();
        return roles.Count > 0 ? roles.Max(r => r.Id) + 1 : 1;
    }

    private async Task<int> GetNextPermissionIdAsync()
    {
        var permissions = await GetAllPermissionsDataAsync();
        return permissions.Count > 0 ? permissions.Max(p => p.Id) + 1 : 1;
    }

    private async Task<int> GetNextRolePermissionIdAsync()
    {
        var rolePermissions = await GetAllRolePermissionsDataAsync();
        return rolePermissions.Count > 0 ? rolePermissions.Max(rp => rp.Id) + 1 : 1;
    }

    private async Task<int> GetNextUserRoleIdAsync()
    {
        var userRoles = await GetAllUserRolesDataAsync();
        return userRoles.Count > 0 ? userRoles.Max(ur => ur.Id) + 1 : 1;
    }

    private async Task<int> GetRoleUserCountAsync(int roleId)
    {
        var userRoles = await GetAllUserRolesDataAsync();
        return userRoles.Count(ur => ur.RoleId == roleId && ur.IsActive);
    }

    private async Task<List<Permission>> GetUserAllPermissionsAsync(int userId)
    {
        var userRoles = await GetAllUserRolesDataAsync();
        var rolePermissions = await GetAllRolePermissionsDataAsync();
        var permissions = await GetAllPermissionsDataAsync();

        var userActiveRoleIds = userRoles
            .Where(ur => ur.UserId == userId && ur.IsActive)
            .Select(ur => ur.RoleId)
            .ToList();

        var userPermissionIds = rolePermissions
            .Where(rp => userActiveRoleIds.Contains(rp.RoleId) && rp.IsActive)
            .Select(rp => rp.PermissionId)
            .Distinct()
            .ToList();

        return permissions.Where(p => userPermissionIds.Contains(p.Id)).ToList();
    }

    private async Task<UserRoleResponseDto?> GetUserRoleResponseAsync(int userId, int roleId)
    {
        var userRoles = await GetUserRolesAsync(userId);
        return userRoles.FirstOrDefault(ur => ur.RoleId == roleId);
    }

    private List<Permission> GetDefaultSystemPermissions()
    {
        var permissions = new List<Permission>();
        var id = 1;

        var resources = new[]
        {
            "users", "roles", "permissions", "profiles", "educations", "workexperiences",
            "skills", "portfolios", "calendarevents", "todoitems", "worktasks",
            "blogposts", "guestbook", "contactmethods", "files", "system"
        };

        var actions = new[]
        {
            PermissionAction.Create, PermissionAction.Read, PermissionAction.Update, 
            PermissionAction.Delete, PermissionAction.Manage
        };

        foreach (var resource in resources)
        {
            foreach (var action in actions)
            {
                permissions.Add(new Permission
                {
                    Id = id++,
                    Name = GeneratePermissionName(resource, action),
                    DisplayName = $"{GetResourceDisplayName(resource)} - {GetActionDisplayName(action)}",
                    Description = $"允許{GetActionDisplayName(action)}{GetResourceDisplayName(resource)}",
                    Category = GetResourceCategory(resource),
                    Action = action,
                    Resource = resource,
                    IsSystemPermission = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        return permissions;
    }

    private string GetResourceDisplayName(string resource)
    {
        return resource switch
        {
            "users" => "使用者",
            "roles" => "角色",
            "permissions" => "權限",
            "profiles" => "個人資料",
            "educations" => "學歷",
            "workexperiences" => "工作經歷",
            "skills" => "技能",
            "portfolios" => "作品集",
            "calendarevents" => "行事曆事件",
            "todoitems" => "待辦事項",
            "worktasks" => "工作任務",
            "blogposts" => "部落格文章",
            "guestbook" => "留言板",
            "contactmethods" => "聯絡方式",
            "files" => "檔案",
            "system" => "系統",
            _ => resource
        };
    }

    private string GetActionDisplayName(PermissionAction action)
    {
        return action switch
        {
            PermissionAction.Create => "建立",
            PermissionAction.Read => "查看",
            PermissionAction.Update => "修改",
            PermissionAction.Delete => "刪除",
            PermissionAction.Manage => "管理",
            PermissionAction.Execute => "執行",
            PermissionAction.Export => "匯出",
            PermissionAction.Import => "匯入",
            PermissionAction.Publish => "發佈",
            PermissionAction.Approve => "審核",
            _ => action.ToString()
        };
    }

    private string GetResourceCategory(string resource)
    {
        return resource switch
        {
            "users" or "roles" or "permissions" => "使用者管理",
            "profiles" or "educations" or "workexperiences" or "skills" or "portfolios" or "contactmethods" => "個人資料",
            "calendarevents" or "todoitems" or "worktasks" => "任務管理",
            "blogposts" or "guestbook" => "內容管理",
            "files" => "檔案管理",
            "system" => "系統管理",
            _ => "其他"
        };
    }

    #endregion
}