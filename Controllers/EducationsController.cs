using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Services;

namespace PersonalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EducationsController : ControllerBase
{
    private readonly IEducationService _service;
    public EducationsController(IEducationService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(ApiResponse<List<EducationResponse>>.Ok(await _service.GetAllAsync()));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item != null ? Ok(ApiResponse<EducationResponse>.Ok(item)) : NotFound(ApiResponse.Fail("Education not found"));
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(int userId)
        => Ok(ApiResponse<List<EducationResponse>>.Ok(await _service.GetByUserIdAsync(userId)));

    [HttpGet("user/{userId}/public")]
    public async Task<IActionResult> GetPublicByUserId(int userId)
        => Ok(ApiResponse<List<EducationResponse>>.Ok(await _service.GetPublicByUserIdAsync(userId)));

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEducationDto dto)
        => Ok(ApiResponse<EducationResponse>.Ok(await _service.CreateAsync(dto), "Education created"));

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEducationDto dto)
    {
        var item = await _service.UpdateAsync(id, dto);
        return item != null ? Ok(ApiResponse<EducationResponse>.Ok(item, "Education updated")) : NotFound(ApiResponse.Fail("Education not found"));
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => await _service.DeleteAsync(id) ? Ok(ApiResponse.Ok("Education deleted")) : NotFound(ApiResponse.Fail("Education not found"));
}
