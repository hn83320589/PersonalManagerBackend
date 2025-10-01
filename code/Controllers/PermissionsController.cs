using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Services;

namespace PersonalManagerAPI.Controllers;

/// <summary>
/// 權限管理API控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
// [Authorize(Roles = "Admin")] // 生產環境啟用，管理員權限
public class PermissionsController : BaseController
{
    private readonly IPermissionService _permissionService;
    private readonly ILogger<PermissionsController> _logger;

    public PermissionsController(IPermissionService permissionService, ILogger<PermissionsController> logger)
    {
        _permissionService = permissionService;
        _logger = logger;
    }

    /// <summary>
    /// 取得所有權限
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllPermissions()
    {
        var result = await _permissionService.GetAllPermissionsAsync();
        return CreateResponse(result);
    }

    /// <summary>
    /// 根據ID取得權限
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPermissionById(int id)
    {
        var result = await _permissionService.GetPermissionByIdAsync(id);
        return CreateResponse(result);
    }

    /// <summary>
    /// 根據名稱取得權限
    /// </summary>
    [HttpGet("by-name/{name}")]
    public async Task<IActionResult> GetPermissionByName(string name)
    {
        var result = await _permissionService.GetPermissionByNameAsync(name);
        return CreateResponse(result);
    }

    /// <summary>
    /// 根據分類取得權限
    /// </summary>
    [HttpGet("by-category/{category}")]
    public async Task<IActionResult> GetPermissionsByCategory(string category)
    {
        var result = await _permissionService.GetPermissionsByCategoryAsync(category);
        return CreateResponse(result);
    }

    /// <summary>
    /// 根據資源取得權限
    /// </summary>
    [HttpGet("by-resource/{resource}")]
    public async Task<IActionResult> GetPermissionsByResource(string resource)
    {
        var result = await _permissionService.GetPermissionsByResourceAsync(resource);
        return CreateResponse(result);
    }

    /// <summary>
    /// 建立新權限
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(CreateErrorResponse("資料驗證失敗", GetModelStateErrors()));
        }

        var result = await _permissionService.CreatePermissionAsync(dto);
        return CreateResponse(result);
    }

    /// <summary>
    /// 更新權限
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePermission(int id, [FromBody] UpdatePermissionDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(CreateErrorResponse("資料驗證失敗", GetModelStateErrors()));
        }

        var result = await _permissionService.UpdatePermissionAsync(id, dto);
        return CreateResponse(result);
    }

    /// <summary>
    /// 刪除權限
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePermission(int id)
    {
        var result = await _permissionService.DeletePermissionAsync(id);
        return CreateResponse(result);
    }

    /// <summary>
    /// 檢查權限名稱是否存在
    /// </summary>
    [HttpGet("check-name/{name}")]
    public async Task<IActionResult> CheckPermissionName(string name, [FromQuery] int? excludeId = null)
    {
        var exists = await _permissionService.PermissionNameExistsAsync(name, excludeId);
        return Ok(CreateSuccessResponse(new { exists }, "檢查完成"));
    }

    /// <summary>
    /// 取得所有權限分類
    /// </summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetAllCategories()
    {
        var result = await _permissionService.GetAllCategoriesAsync();
        return CreateResponse(result);
    }

    /// <summary>
    /// 取得所有資源類型
    /// </summary>
    [HttpGet("resources")]
    public async Task<IActionResult> GetAllResources()
    {
        var result = await _permissionService.GetAllResourcesAsync();
        return CreateResponse(result);
    }

    /// <summary>
    /// 啟用/停用權限
    /// </summary>
    [HttpPatch("{id}/toggle-status")]
    public async Task<IActionResult> TogglePermissionStatus(int id, [FromBody] bool isActive)
    {
        var result = await _permissionService.TogglePermissionStatusAsync(id, isActive);
        return CreateResponse(result);
    }
}
