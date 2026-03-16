using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Models;
using PersonalManager.Api.Services;

namespace PersonalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : BaseApiController
{
    private readonly IProjectService _service;
    public ProjectsController(IProjectService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized(ApiResponse.Fail("Unauthorized"));
        return Ok(ApiResponse<List<ProjectResponse>>.Ok(await _service.GetByUserIdAsync(currentUserId.Value)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized(ApiResponse.Fail("Unauthorized"));
        var item = await _service.GetByIdAsync(id);
        if (item == null) return NotFound(ApiResponse.Fail("Project not found"));
        if (item.UserId != currentUserId.Value) return StatusCode(403, ApiResponse.Fail("Forbidden"));
        return Ok(ApiResponse<ProjectResponse>.Ok(item));
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(int userId)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized(ApiResponse.Fail("Unauthorized"));
        if (userId != currentUserId.Value) return StatusCode(403, ApiResponse.Fail("Forbidden"));
        return Ok(ApiResponse<List<ProjectResponse>>.Ok(await _service.GetByUserIdAsync(userId)));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized(ApiResponse.Fail("Unauthorized"));
        dto.UserId = currentUserId.Value;
        return Ok(ApiResponse<ProjectResponse>.Ok(await _service.CreateAsync(dto), "Project created"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProjectDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized(ApiResponse.Fail("Unauthorized"));
        var existing = await _service.GetByIdAsync(id);
        if (existing == null) return NotFound(ApiResponse.Fail("Project not found"));
        if (existing.UserId != currentUserId.Value) return StatusCode(403, ApiResponse.Fail("Forbidden"));
        var item = await _service.UpdateAsync(id, dto);
        return item != null ? Ok(ApiResponse<ProjectResponse>.Ok(item, "Project updated")) : NotFound(ApiResponse.Fail("Project not found"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized(ApiResponse.Fail("Unauthorized"));
        var existing = await _service.GetByIdAsync(id);
        if (existing == null) return NotFound(ApiResponse.Fail("Project not found"));
        if (existing.UserId != currentUserId.Value) return StatusCode(403, ApiResponse.Fail("Forbidden"));
        return await _service.DeleteAsync(id) ? Ok(ApiResponse.Ok("Project deleted")) : NotFound(ApiResponse.Fail("Project not found"));
    }
}
