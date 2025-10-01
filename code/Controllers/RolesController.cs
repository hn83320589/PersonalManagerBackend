using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Services;

namespace PersonalManagerAPI.Controllers;

/// <summary>
/// 角色管理API控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
// [Authorize(Roles = "Admin")] // 生產環境啟用，管理員權限
public class RolesController : BaseController
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RolesController> _logger;

    public RolesController(IRoleService roleService, ILogger<RolesController> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    /// <summary>
    /// 取得所有角色
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllRoles()
    {
        var result = await _roleService.GetAllRolesAsync();
        return CreateResponse(result);
    }

    /// <summary>
    /// 根據ID取得角色
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoleById(int id)
    {
        var result = await _roleService.GetRoleByIdAsync(id);
        return CreateResponse(result);
    }

    /// <summary>
    /// 根據名稱取得角色
    /// </summary>
    [HttpGet("by-name/{name}")]
    public async Task<IActionResult> GetRoleByName(string name)
    {
        var result = await _roleService.GetRoleByNameAsync(name);
        return CreateResponse(result);
    }

    /// <summary>
    /// 建立新角色
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(CreateErrorResponse("資料驗證失敗", GetModelStateErrors()));
        }

        // 從 JWT Claims 中獲取使用者ID (生產環境實作)
        int? createdById = null; // TODO: 從 HttpContext.User.Claims 中取得

        var result = await _roleService.CreateRoleAsync(dto, createdById);
        return CreateResponse(result);
    }

    /// <summary>
    /// 更新角色
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(CreateErrorResponse("資料驗證失敗", GetModelStateErrors()));
        }

        int? updatedById = null; // TODO: 從 HttpContext.User.Claims 中取得

        var result = await _roleService.UpdateRoleAsync(id, dto, updatedById);
        return CreateResponse(result);
    }

    /// <summary>
    /// 刪除角色
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        var result = await _roleService.DeleteRoleAsync(id);
        return CreateResponse(result);
    }

    /// <summary>
    /// 取得角色的所有權限
    /// </summary>
    [HttpGet("{roleId}/permissions")]
    public async Task<IActionResult> GetRolePermissions(int roleId)
    {
        var result = await _roleService.GetRolePermissionsAsync(roleId);
        return CreateResponse(result);
    }

    /// <summary>
    /// 為角色分配權限
    /// </summary>
    [HttpPost("{roleId}/permissions")]
    public async Task<IActionResult> AssignPermissionsToRole(int roleId, [FromBody] AssignPermissionsDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(CreateErrorResponse("資料驗證失敗", GetModelStateErrors()));
        }

        int? assignedById = null; // TODO: 從 HttpContext.User.Claims 中取得

        var result = await _roleService.AssignPermissionsToRoleAsync(roleId, dto, assignedById);
        return CreateResponse(result);
    }

    /// <summary>
    /// 從角色移除權限
    /// </summary>
    [HttpDelete("{roleId}/permissions/{permissionId}")]
    public async Task<IActionResult> RemovePermissionFromRole(int roleId, int permissionId)
    {
        var result = await _roleService.RemovePermissionFromRoleAsync(roleId, permissionId);
        return CreateResponse(result);
    }

    /// <summary>
    /// 取得角色的所有使用者
    /// </summary>
    [HttpGet("{roleId}/users")]
    public async Task<IActionResult> GetRoleUsers(int roleId)
    {
        var result = await _roleService.GetRoleUsersAsync(roleId);
        return CreateResponse(result);
    }

    /// <summary>
    /// 檢查角色名稱是否存在
    /// </summary>
    [HttpGet("check-name/{name}")]
    public async Task<IActionResult> CheckRoleName(string name, [FromQuery] int? excludeId = null)
    {
        var exists = await _roleService.RoleNameExistsAsync(name, excludeId);
        return Ok(CreateSuccessResponse(new { exists }, "檢查完成"));
    }

    /// <summary>
    /// 啟用/停用角色
    /// </summary>
    [HttpPatch("{id}/toggle-status")]
    public async Task<IActionResult> ToggleRoleStatus(int id, [FromBody] bool isActive)
    {
        var result = await _roleService.ToggleRoleStatusAsync(id, isActive);
        return CreateResponse(result);
    }
}
