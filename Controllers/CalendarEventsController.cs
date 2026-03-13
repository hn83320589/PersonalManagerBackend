using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Services;

namespace PersonalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CalendarEventsController : BaseApiController
{
    private readonly ICalendarEventService _service;
    public CalendarEventsController(ICalendarEventService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(ApiResponse<List<CalendarEventResponse>>.Ok(await _service.GetAllAsync()));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item != null ? Ok(ApiResponse<CalendarEventResponse>.Ok(item)) : NotFound(ApiResponse.Fail("Calendar event not found"));
    }

    [Authorize]
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(int userId)
        => Ok(ApiResponse<List<CalendarEventResponse>>.Ok(await _service.GetByUserIdAsync(userId)));

    [HttpGet("user/{userId}/public")]
    public async Task<IActionResult> GetPublicByUserId(int userId)
        => Ok(ApiResponse<List<CalendarEventResponse>>.Ok(await _service.GetPublicByUserIdAsync(userId)));

    [Authorize]
    [HttpGet("user/{userId}/range")]
    public async Task<IActionResult> GetByDateRange(int userId, [FromQuery] DateTime start, [FromQuery] DateTime end)
        => Ok(ApiResponse<List<CalendarEventResponse>>.Ok(await _service.GetByDateRangeAsync(userId, start, end)));

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCalendarEventDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized(ApiResponse.Fail("Unauthorized"));
        dto.UserId = currentUserId.Value;
        return Ok(ApiResponse<CalendarEventResponse>.Ok(await _service.CreateAsync(dto), "Event created"));
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCalendarEventDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized(ApiResponse.Fail("Unauthorized"));
        var existing = await _service.GetByIdAsync(id);
        if (existing == null) return NotFound(ApiResponse.Fail("Event not found"));
        if (existing.UserId != currentUserId.Value) return StatusCode(403, ApiResponse.Fail("Forbidden"));
        var item = await _service.UpdateAsync(id, dto);
        return item != null ? Ok(ApiResponse<CalendarEventResponse>.Ok(item, "Event updated")) : NotFound(ApiResponse.Fail("Event not found"));
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized(ApiResponse.Fail("Unauthorized"));
        var existing = await _service.GetByIdAsync(id);
        if (existing == null) return NotFound(ApiResponse.Fail("Event not found"));
        if (existing.UserId != currentUserId.Value) return StatusCode(403, ApiResponse.Fail("Forbidden"));
        return await _service.DeleteAsync(id) ? Ok(ApiResponse.Ok("Event deleted")) : NotFound(ApiResponse.Fail("Event not found"));
    }
}
