using Microsoft.AspNetCore.Mvc;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.DTOs.CalendarEvent;
using PersonalManagerAPI.Services.Interfaces;

namespace PersonalManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CalendarEventsController : BaseController
{
    private readonly ICalendarEventService _calendarEventService;

    public CalendarEventsController(ICalendarEventService calendarEventService)
    {
        _calendarEventService = calendarEventService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CalendarEventResponseDto>>>> GetCalendarEvents([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var events = await _calendarEventService.GetAllCalendarEventsAsync(skip, take);
        return Ok(ApiResponse<IEnumerable<CalendarEventResponseDto>>.SuccessResult(events, "成功取得行事曆事件列表"));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CalendarEventResponseDto>>> GetCalendarEvent(int id)
    {
        var calendarEvent = await _calendarEventService.GetCalendarEventByIdAsync(id);
        
        if (calendarEvent == null)
        {
            return NotFound(ApiResponse<CalendarEventResponseDto>.ErrorResult("找不到指定的行事曆事件"));
        }

        return Ok(ApiResponse<CalendarEventResponseDto>.SuccessResult(calendarEvent, "成功取得行事曆事件資料"));
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CalendarEventResponseDto>>>> GetCalendarEventsByUserId(int userId)
    {
        var events = await _calendarEventService.GetCalendarEventsByUserIdAsync(userId);
        return Ok(ApiResponse<IEnumerable<CalendarEventResponseDto>>.SuccessResult(events, "成功取得使用者行事曆事件列表"));
    }

    [HttpGet("public")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CalendarEventResponseDto>>>> GetPublicCalendarEvents([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var events = await _calendarEventService.GetPublicCalendarEventsAsync(skip, take);
        return Ok(ApiResponse<IEnumerable<CalendarEventResponseDto>>.SuccessResult(events, "成功取得公開行事曆事件列表"));
    }

    [HttpGet("date-range")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CalendarEventResponseDto>>>> GetCalendarEventsByDateRange(
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate, 
        [FromQuery] bool publicOnly = true)
    {
        var events = await _calendarEventService.GetCalendarEventsByDateRangeAsync(startDate, endDate, publicOnly);
        return Ok(ApiResponse<IEnumerable<CalendarEventResponseDto>>.SuccessResult(events, "成功取得指定日期範圍的行事曆事件"));
    }

    [HttpGet("month/{year}/{month}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CalendarEventResponseDto>>>> GetCalendarEventsByMonth(int year, int month, [FromQuery] bool publicOnly = true)
    {
        if (month < 1 || month > 12)
        {
            return BadRequest(ApiResponse<IEnumerable<CalendarEventResponseDto>>.ErrorResult("月份必須在1-12之間"));
        }

        var events = await _calendarEventService.GetCalendarEventsByMonthAsync(year, month, publicOnly);
        return Ok(ApiResponse<IEnumerable<CalendarEventResponseDto>>.SuccessResult(events, $"成功取得{year}年{month}月的行事曆事件"));
    }

    [HttpGet("week")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CalendarEventResponseDto>>>> GetCalendarEventsByWeek([FromQuery] DateTime startOfWeek, [FromQuery] bool publicOnly = true)
    {
        var events = await _calendarEventService.GetCalendarEventsByWeekAsync(startOfWeek, publicOnly);
        return Ok(ApiResponse<IEnumerable<CalendarEventResponseDto>>.SuccessResult(events, "成功取得本週行事曆事件"));
    }

    [HttpGet("today")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CalendarEventResponseDto>>>> GetTodayCalendarEvents([FromQuery] bool publicOnly = true)
    {
        var events = await _calendarEventService.GetTodayCalendarEventsAsync(publicOnly);
        return Ok(ApiResponse<IEnumerable<CalendarEventResponseDto>>.SuccessResult(events, "成功取得今日行事曆事件"));
    }

    [HttpGet("upcoming")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CalendarEventResponseDto>>>> GetUpcomingEvents([FromQuery] int days = 7, [FromQuery] bool publicOnly = true)
    {
        var events = await _calendarEventService.GetUpcomingEventsAsync(days, publicOnly);
        return Ok(ApiResponse<IEnumerable<CalendarEventResponseDto>>.SuccessResult(events, $"成功取得未來{days}天的行事曆事件"));
    }

    [HttpGet("type/{type}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CalendarEventResponseDto>>>> SearchByType(string type, [FromQuery] bool publicOnly = true)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            return BadRequest(ApiResponse<IEnumerable<CalendarEventResponseDto>>.ErrorResult("事件類型不能為空"));
        }

        var events = await _calendarEventService.SearchByTypeAsync(type, publicOnly);
        return Ok(ApiResponse<IEnumerable<CalendarEventResponseDto>>.SuccessResult(events, $"成功取得{type}類型的行事曆事件"));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CalendarEventResponseDto>>>> SearchCalendarEvents([FromQuery] string keyword, [FromQuery] bool publicOnly = true)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return BadRequest(ApiResponse<IEnumerable<CalendarEventResponseDto>>.ErrorResult("搜尋關鍵字不能為空"));
        }

        var events = await _calendarEventService.SearchCalendarEventsAsync(keyword, publicOnly);
        return Ok(ApiResponse<IEnumerable<CalendarEventResponseDto>>.SuccessResult(events, "搜尋完成"));
    }

    [HttpGet("types")]
    public async Task<ActionResult<ApiResponse<IEnumerable<string>>>> GetEventTypes([FromQuery] bool publicOnly = true)
    {
        var types = await _calendarEventService.GetEventTypesAsync(publicOnly);
        return Ok(ApiResponse<IEnumerable<string>>.SuccessResult(types, "成功取得事件類型列表"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CalendarEventResponseDto>>> CreateCalendarEvent([FromBody] CreateCalendarEventDto createCalendarEventDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
            return BadRequest(ApiResponse<CalendarEventResponseDto>.ErrorResult("資料驗證失敗", errors));
        }

        var calendarEvent = await _calendarEventService.CreateCalendarEventAsync(createCalendarEventDto);

        return CreatedAtAction(nameof(GetCalendarEvent), new { id = calendarEvent.Id }, 
            ApiResponse<CalendarEventResponseDto>.SuccessResult(calendarEvent, "行事曆事件建立成功"));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CalendarEventResponseDto>>> UpdateCalendarEvent(int id, [FromBody] UpdateCalendarEventDto updateCalendarEventDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
            return BadRequest(ApiResponse<CalendarEventResponseDto>.ErrorResult("資料驗證失敗", errors));
        }

        var calendarEvent = await _calendarEventService.UpdateCalendarEventAsync(id, updateCalendarEventDto);
        
        if (calendarEvent == null)
        {
            return NotFound(ApiResponse<CalendarEventResponseDto>.ErrorResult("找不到指定的行事曆事件"));
        }

        return Ok(ApiResponse<CalendarEventResponseDto>.SuccessResult(calendarEvent, "行事曆事件更新成功"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteCalendarEvent(int id)
    {
        var result = await _calendarEventService.DeleteCalendarEventAsync(id);
        
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResult("找不到指定的行事曆事件"));
        }

        return Ok(ApiResponse.SuccessResult("行事曆事件刪除成功"));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<object>>> GetCalendarEventStats()
    {
        var stats = await _calendarEventService.GetCalendarEventStatsAsync();
        return Ok(ApiResponse<object>.SuccessResult(stats, "成功取得行事曆事件統計"));
    }

    [HttpGet("check-conflict")]
    public async Task<ActionResult<ApiResponse<bool>>> CheckTimeConflict(
        [FromQuery] int userId, 
        [FromQuery] DateTime startTime, 
        [FromQuery] DateTime endTime, 
        [FromQuery] int? excludeEventId = null)
    {
        var hasConflict = await _calendarEventService.CheckTimeConflictAsync(userId, startTime, endTime, excludeEventId);
        return Ok(ApiResponse<bool>.SuccessResult(hasConflict, hasConflict ? "檢測到時間衝突" : "無時間衝突"));
    }
}