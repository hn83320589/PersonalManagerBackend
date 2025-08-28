using Microsoft.AspNetCore.Mvc;
using PersonalManagerAPI.Controllers;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Services.Interfaces;
using PersonalManagerAPI.Attributes;

namespace PersonalManagerAPI.Controllers;

/// <summary>
/// RBAC 系統統一管理 API
/// </summary>
[ApiController]
[Route("api/rbac")]
public class RbacController : BaseController
{
    private readonly IRbacService _rbacService;

    public RbacController(IRbacService rbacService)
    {
        _rbacService = rbacService;
    }

    #region Role Management

    /// <summary>
    /// 獲取所有角色
    /// </summary>
    [HttpGet("roles")]
    [RequirePermission("Role.Read")]
    public async Task<ActionResult<ApiResponse<List<RoleResponseDto>>>> GetAllRoles([FromQuery] bool includeInactive = false)
    {
        try
        {
            var roles = await _rbacService.GetAllRolesAsync(includeInactive);
            return Ok(ApiResponse<List<RoleResponseDto>>.Success(roles, "成功獲取角色列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<RoleResponseDto>>.Failure($"獲取角色列表失敗: {ex.Message}"));
        }
    }

    /// <summary>
    /// 根據ID獲取角色
    /// </summary>
    [HttpGet("roles/{id}")]
    [RequirePermission("Role.Read")]
    public async Task<ActionResult<ApiResponse<RoleResponseDto>>> GetRoleById(int id)
    {
        try
        {
            var role = await _rbacService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound(ApiResponse<RoleResponseDto>.Failure("角色不存在"));
            }
            return Ok(ApiResponse<RoleResponseDto>.Success(role, "成功獲取角色詳情"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RoleResponseDto>.Failure($"獲取角色詳情失敗: {ex.Message}"));
        }
    }

    /// <summary>
    /// 建立新角色
    /// </summary>
    [HttpPost("roles")]
    [RequirePermission("Role.Create")]
    public async Task<ActionResult<ApiResponse<RoleResponseDto>>> CreateRole([FromBody] CreateRoleDto createRoleDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<RoleResponseDto>.Failure("資料驗證失敗", GetModelErrors()));
            }

            // 暫時使用固定的 createdById，在實際應用中應從 JWT Token 中獲取
            const int tempUserId = 1;
            var role = await _rbacService.CreateRoleAsync(createRoleDto, tempUserId);
            
            return CreatedAtAction(
                nameof(GetRoleById), 
                new { id = role.Id }, 
                ApiResponse<RoleResponseDto>.Success(role, "角色建立成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RoleResponseDto>.Failure($"建立角色失敗: {ex.Message}"));
        }
    }

    #endregion

    #region Permission Management

    /// <summary>
    /// 獲取所有權限
    /// </summary>
    [HttpGet("permissions")]
    [RequirePermission("Permission.Read")]
    public async Task<ActionResult<ApiResponse<List<PermissionResponseDto>>>> GetAllPermissions([FromQuery] bool includeInactive = false)
    {
        try
        {
            var permissions = await _rbacService.GetAllPermissionsAsync(includeInactive);
            return Ok(ApiResponse<List<PermissionResponseDto>>.Success(permissions, "成功獲取權限列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<PermissionResponseDto>>.Failure($"獲取權限列表失敗: {ex.Message}"));
        }
    }

    /// <summary>
    /// 根據ID獲取權限
    /// </summary>
    [HttpGet("permissions/{id}")]
    [RequirePermission("Permission.Read")]
    public async Task<ActionResult<ApiResponse<PermissionResponseDto>>> GetPermissionById(int id)
    {
        try
        {
            var permission = await _rbacService.GetPermissionByIdAsync(id);
            if (permission == null)
            {
                return NotFound(ApiResponse<PermissionResponseDto>.Failure("權限不存在"));
            }
            return Ok(ApiResponse<PermissionResponseDto>.Success(permission, "成功獲取權限詳情"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PermissionResponseDto>.Failure($"獲取權限詳情失敗: {ex.Message}"));
        }
    }

    /// <summary>
    /// 建立新權限
    /// </summary>
    [HttpPost("permissions")]
    [RequirePermission("Permission.Create")]
    public async Task<ActionResult<ApiResponse<PermissionResponseDto>>> CreatePermission([FromBody] CreatePermissionDto createPermissionDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<PermissionResponseDto>.Failure("資料驗證失敗", GetModelErrors()));
            }

            var permission = await _rbacService.CreatePermissionAsync(createPermissionDto);
            
            return CreatedAtAction(
                nameof(GetPermissionById), 
                new { id = permission.Id }, 
                ApiResponse<PermissionResponseDto>.Success(permission, "權限建立成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PermissionResponseDto>.Failure($"建立權限失敗: {ex.Message}"));
        }
    }

    #endregion

    #region Basic User Role Management

    /// <summary>
    /// 獲取使用者的角色列表
    /// </summary>
    [HttpGet("users/{userId}/roles")]
    [RequirePermission("User.Read")]
    public async Task<ActionResult<ApiResponse<List<UserRoleResponseDto>>>> GetUserRoles(int userId)
    {
        try
        {
            var userRoles = await _rbacService.GetUserRolesAsync(userId);
            return Ok(ApiResponse<List<UserRoleResponseDto>>.Success(userRoles, $"成功獲取使用者角色，共 {userRoles.Count} 個角色"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<UserRoleResponseDto>>.Failure($"獲取使用者角色失敗: {ex.Message}"));
        }
    }

    #endregion

    #region Statistics and System Management

    /// <summary>
    /// 獲取RBAC系統統計資訊
    /// </summary>
    [HttpGet("statistics")]
    [RequirePermission("System.Read")]
    public async Task<ActionResult<ApiResponse<object>>> GetSystemStatistics()
    {
        try
        {
            var roleStats = await _rbacService.GetRoleStatisticsAsync();
            var permissionStats = await _rbacService.GetPermissionStatisticsAsync();

            var stats = new
            {
                RoleStatistics = roleStats,
                PermissionStatistics = permissionStats,
                GeneratedAt = DateTime.UtcNow
            };

            return Ok(ApiResponse<object>.Success(stats, "成功獲取系統統計資訊"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Failure($"獲取系統統計失敗: {ex.Message}"));
        }
    }

    /// <summary>
    /// 系統健康檢查
    /// </summary>
    [HttpGet("health")]
    public ActionResult<ApiResponse<object>> HealthCheck()
    {
        try
        {
            var health = new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Services = new
                {
                    RbacService = "Online",
                    DataAccess = "Online"
                }
            };

            return Ok(ApiResponse<object>.Success(health, "系統健康狀態正常"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Failure($"健康檢查失敗: {ex.Message}"));
        }
    }

    #endregion
}