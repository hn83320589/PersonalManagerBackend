using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Services;

namespace PersonalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CalendarEventsController : ControllerBase
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

    [Authorize] [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCalendarEventDto dto)
        => Ok(ApiResponse<CalendarEventResponse>.Ok(await _service.CreateAsync(dto), "Event created"));

    [Authorize] [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCalendarEventDto dto)
    {
        var item = await _service.UpdateAsync(id, dto);
        return item != null ? Ok(ApiResponse<CalendarEventResponse>.Ok(item, "Event updated")) : NotFound(ApiResponse.Fail("Event not found"));
    }

    [Authorize] [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => await _service.DeleteAsync(id) ? Ok(ApiResponse.Ok("Event deleted")) : NotFound(ApiResponse.Fail("Event not found"));
}
