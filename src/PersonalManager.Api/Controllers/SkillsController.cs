using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Services;

namespace PersonalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillsController : ControllerBase
{
    private readonly ISkillService _service;
    public SkillsController(ISkillService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(ApiResponse<List<SkillResponse>>.Ok(await _service.GetAllAsync()));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item != null ? Ok(ApiResponse<SkillResponse>.Ok(item)) : NotFound(ApiResponse.Fail("Skill not found"));
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(int userId)
        => Ok(ApiResponse<List<SkillResponse>>.Ok(await _service.GetByUserIdAsync(userId)));

    [HttpGet("user/{userId}/public")]
    public async Task<IActionResult> GetPublicByUserId(int userId)
        => Ok(ApiResponse<List<SkillResponse>>.Ok(await _service.GetPublicByUserIdAsync(userId)));

    [HttpGet("user/{userId}/category/{category}")]
    public async Task<IActionResult> GetByCategory(int userId, string category)
        => Ok(ApiResponse<List<SkillResponse>>.Ok(await _service.GetByCategoryAsync(userId, category)));

    [Authorize] [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSkillDto dto)
        => Ok(ApiResponse<SkillResponse>.Ok(await _service.CreateAsync(dto), "Skill created"));

    [Authorize] [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSkillDto dto)
    {
        var item = await _service.UpdateAsync(id, dto);
        return item != null ? Ok(ApiResponse<SkillResponse>.Ok(item, "Skill updated")) : NotFound(ApiResponse.Fail("Skill not found"));
    }

    [Authorize] [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => await _service.DeleteAsync(id) ? Ok(ApiResponse.Ok("Skill deleted")) : NotFound(ApiResponse.Fail("Skill not found"));
}
