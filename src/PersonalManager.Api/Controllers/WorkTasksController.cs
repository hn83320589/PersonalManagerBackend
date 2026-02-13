using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Models;
using PersonalManager.Api.Services;

namespace PersonalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkTasksController : ControllerBase
{
    private readonly IWorkTaskService _service;
    public WorkTasksController(IWorkTaskService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(ApiResponse<List<WorkTaskResponse>>.Ok(await _service.GetAllAsync()));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item != null ? Ok(ApiResponse<WorkTaskResponse>.Ok(item)) : NotFound(ApiResponse.Fail("Work task not found"));
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(int userId)
        => Ok(ApiResponse<List<WorkTaskResponse>>.Ok(await _service.GetByUserIdAsync(userId)));

    [HttpGet("user/{userId}/project/{project}")]
    public async Task<IActionResult> GetByProject(int userId, string project)
        => Ok(ApiResponse<List<WorkTaskResponse>>.Ok(await _service.GetByProjectAsync(userId, project)));

    [HttpGet("user/{userId}/status/{status}")]
    public async Task<IActionResult> GetByStatus(int userId, WorkTaskStatus status)
        => Ok(ApiResponse<List<WorkTaskResponse>>.Ok(await _service.GetByStatusAsync(userId, status)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkTaskDto dto)
        => Ok(ApiResponse<WorkTaskResponse>.Ok(await _service.CreateAsync(dto), "Work task created"));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWorkTaskDto dto)
    {
        var item = await _service.UpdateAsync(id, dto);
        return item != null ? Ok(ApiResponse<WorkTaskResponse>.Ok(item, "Work task updated")) : NotFound(ApiResponse.Fail("Work task not found"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => await _service.DeleteAsync(id) ? Ok(ApiResponse.Ok("Work task deleted")) : NotFound(ApiResponse.Fail("Work task not found"));
}
