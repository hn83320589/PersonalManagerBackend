using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Services;

namespace PersonalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TimeEntriesController : BaseApiController
{
    private readonly ITimeEntryService _service;
    public TimeEntriesController(ITimeEntryService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized(ApiResponse.Fail("Unauthorized"));
        return Ok(ApiResponse<List<TimeEntryResponse>>.Ok(await _service.GetByUserIdAsync(currentUserId.Value)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized(ApiResponse.Fail("Unauthorized"));
        var item = await _service.GetByIdAsync(id);
        if (item == null) return NotFound(ApiResponse.Fail("Time entry not found"));
        if (item.UserId != currentUserId.Value) return StatusCode(403, ApiResponse.Fail("Forbidden"));
        return Ok(ApiResponse<TimeEntryResponse>.Ok(item));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTimeEntryDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized(ApiResponse.Fail("Unauthorized"));
        dto.UserId = currentUserId.Value;
        return Ok(ApiResponse<TimeEntryResponse>.Ok(await _service.CreateAsync(dto), "Time entry created"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTimeEntryDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized(ApiResponse.Fail("Unauthorized"));
        var existing = await _service.GetByIdAsync(id);
        if (existing == null) return NotFound(ApiResponse.Fail("Time entry not found"));
        if (existing.UserId != currentUserId.Value) return StatusCode(403, ApiResponse.Fail("Forbidden"));
        var item = await _service.UpdateAsync(id, dto);
        return item != null ? Ok(ApiResponse<TimeEntryResponse>.Ok(item, "Time entry updated")) : NotFound(ApiResponse.Fail("Time entry not found"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized(ApiResponse.Fail("Unauthorized"));
        var existing = await _service.GetByIdAsync(id);
        if (existing == null) return NotFound(ApiResponse.Fail("Time entry not found"));
        if (existing.UserId != currentUserId.Value) return StatusCode(403, ApiResponse.Fail("Forbidden"));
        return await _service.DeleteAsync(id) ? Ok(ApiResponse.Ok("Time entry deleted")) : NotFound(ApiResponse.Fail("Time entry not found"));
    }
}
