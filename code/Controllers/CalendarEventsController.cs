using Microsoft.AspNetCore.Mvc;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services;

namespace PersonalManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CalendarEventsController : BaseController
{
    private readonly JsonDataService _dataService;

    public CalendarEventsController(JsonDataService dataService)
    {
        _dataService = dataService;
    }

    /// <summary>
    /// 取得所有公開行事曆事件
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCalendarEvents([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var events = await _dataService.GetCalendarEventsAsync();
            var query = events.Where(e => e.IsPublic);

            if (startDate.HasValue)
            {
                query = query.Where(e => e.StartTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.StartTime <= endDate.Value);
            }

            var filteredEvents = query.OrderBy(e => e.StartTime).ToList();
                
            return Ok(ApiResponse<List<CalendarEvent>>.SuccessResult(filteredEvents, "成功取得行事曆事件列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<CalendarEvent>>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 取得指定行事曆事件
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCalendarEvent(int id)
    {
        try
        {
            var events = await _dataService.GetCalendarEventsAsync();
            var calendarEvent = events.FirstOrDefault(e => e.Id == id);
            
            if (calendarEvent == null)
            {
                return NotFound(ApiResponse<CalendarEvent>.ErrorResult("找不到指定的行事曆事件"));
            }

            return Ok(ApiResponse<CalendarEvent>.SuccessResult(calendarEvent, "成功取得行事曆事件資料"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CalendarEvent>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 取得指定使用者的行事曆事件
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetCalendarEventsByUser(int userId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var events = await _dataService.GetCalendarEventsAsync();
            var query = events.Where(e => e.UserId == userId);

            if (startDate.HasValue)
            {
                query = query.Where(e => e.StartTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.StartTime <= endDate.Value);
            }

            var userEvents = query.OrderBy(e => e.StartTime).ToList();
                
            return Ok(ApiResponse<List<CalendarEvent>>.SuccessResult(userEvents, "成功取得使用者行事曆事件列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<CalendarEvent>>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 取得今日事件
    /// </summary>
    [HttpGet("today")]
    public async Task<IActionResult> GetTodayEvents()
    {
        try
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            
            var events = await _dataService.GetCalendarEventsAsync();
            var todayEvents = events
                .Where(e => e.IsPublic && e.StartTime >= today && e.StartTime < tomorrow)
                .OrderBy(e => e.StartTime)
                .ToList();
                
            return Ok(ApiResponse<List<CalendarEvent>>.SuccessResult(todayEvents, "成功取得今日事件列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<CalendarEvent>>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 建立行事曆事件
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCalendarEvent([FromBody] CalendarEvent calendarEvent)
    {
        try
        {
            var events = await _dataService.GetCalendarEventsAsync();
            
            calendarEvent.Id = events.Count > 0 ? events.Max(e => e.Id) + 1 : 1;
            calendarEvent.CreatedAt = DateTime.UtcNow;
            calendarEvent.UpdatedAt = DateTime.UtcNow;
            
            events.Add(calendarEvent);
            await _dataService.SaveCalendarEventsAsync(events);

            return Ok(ApiResponse<CalendarEvent>.SuccessResult(calendarEvent, "行事曆事件建立成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CalendarEvent>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 更新行事曆事件
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCalendarEvent(int id, [FromBody] CalendarEvent updatedEvent)
    {
        try
        {
            var events = await _dataService.GetCalendarEventsAsync();
            var calendarEvent = events.FirstOrDefault(e => e.Id == id);
            
            if (calendarEvent == null)
            {
                return NotFound(ApiResponse<CalendarEvent>.ErrorResult("找不到指定的行事曆事件"));
            }

            calendarEvent.Title = updatedEvent.Title;
            calendarEvent.Description = updatedEvent.Description;
            calendarEvent.StartTime = updatedEvent.StartTime;
            calendarEvent.EndTime = updatedEvent.EndTime;
            calendarEvent.Location = updatedEvent.Location;
            calendarEvent.IsAllDay = updatedEvent.IsAllDay;
            calendarEvent.IsPublic = updatedEvent.IsPublic;
            calendarEvent.Color = updatedEvent.Color;
            calendarEvent.UpdatedAt = DateTime.UtcNow;

            await _dataService.SaveCalendarEventsAsync(events);

            return Ok(ApiResponse<CalendarEvent>.SuccessResult(calendarEvent, "行事曆事件更新成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CalendarEvent>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 刪除行事曆事件
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCalendarEvent(int id)
    {
        try
        {
            var events = await _dataService.GetCalendarEventsAsync();
            var calendarEvent = events.FirstOrDefault(e => e.Id == id);
            
            if (calendarEvent == null)
            {
                return NotFound(ApiResponse.ErrorResult("找不到指定的行事曆事件"));
            }

            events.Remove(calendarEvent);
            await _dataService.SaveCalendarEventsAsync(events);

            return Ok(ApiResponse.SuccessResult("行事曆事件刪除成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }
}