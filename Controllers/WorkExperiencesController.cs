using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Services;

namespace PersonalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkExperiencesController : ControllerBase
{
    private readonly IWorkExperienceService _service;
    public WorkExperiencesController(IWorkExperienceService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(ApiResponse<List<WorkExperienceResponse>>.Ok(await _service.GetAllAsync()));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item != null ? Ok(ApiResponse<WorkExperienceResponse>.Ok(item)) : NotFound(ApiResponse.Fail("Work experience not found"));
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(int userId)
        => Ok(ApiResponse<List<WorkExperienceResponse>>.Ok(await _service.GetByUserIdAsync(userId)));

    [HttpGet("user/{userId}/public")]
    public async Task<IActionResult> GetPublicByUserId(int userId)
        => Ok(ApiResponse<List<WorkExperienceResponse>>.Ok(await _service.GetPublicByUserIdAsync(userId)));

    [Authorize] [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkExperienceDto dto)
        => Ok(ApiResponse<WorkExperienceResponse>.Ok(await _service.CreateAsync(dto), "Work experience created"));

    [Authorize] [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWorkExperienceDto dto)
    {
        var item = await _service.UpdateAsync(id, dto);
        return item != null ? Ok(ApiResponse<WorkExperienceResponse>.Ok(item, "Work experience updated")) : NotFound(ApiResponse.Fail("Work experience not found"));
    }

    [Authorize] [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => await _service.DeleteAsync(id) ? Ok(ApiResponse.Ok("Work experience deleted")) : NotFound(ApiResponse.Fail("Work experience not found"));
}
