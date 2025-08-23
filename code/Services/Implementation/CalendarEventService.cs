using PersonalManagerAPI.DTOs.CalendarEvent;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services;
using PersonalManagerAPI.Services.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace PersonalManagerAPI.Services.Implementation;

/// <summary>
/// 行事曆事件服務實作
/// </summary>
public class CalendarEventService : ICalendarEventService
{
    private readonly JsonDataService _dataService;
    private readonly IMapper _mapper;
    private readonly ILogger<CalendarEventService> _logger;

    // 預定義的事件類型
    private readonly HashSet<string> _validEventTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Meeting", "Appointment", "Reminder", "Task", "Personal", "Work",
        "Travel", "Holiday", "Birthday", "Anniversary", "Conference", "Training",
        "Social", "Health", "Education", "Entertainment", "Other"
    };

    public CalendarEventService(JsonDataService dataService, IMapper mapper, ILogger<CalendarEventService> logger)
    {
        _dataService = dataService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<CalendarEventResponseDto>> GetAllCalendarEventsAsync(int skip = 0, int take = 50)
    {
        try
        {
            var events = await _dataService.GetCalendarEventsAsync();
            var pagedEvents = events
                .OrderBy(e => e.StartTime)
                .Skip(skip)
                .Take(take);
            
            return _mapper.Map<IEnumerable<CalendarEventResponseDto>>(pagedEvents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all calendar events");
            throw;
        }
    }

    public async Task<CalendarEventResponseDto?> GetCalendarEventByIdAsync(int id)
    {
        try
        {
            var events = await _dataService.GetCalendarEventsAsync();
            var calendarEvent = events.FirstOrDefault(e => e.Id == id);
            return _mapper.Map<CalendarEventResponseDto?>(calendarEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting calendar event with ID {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<CalendarEventResponseDto>> GetCalendarEventsByUserIdAsync(int userId)
    {
        try
        {
            var events = await _dataService.GetCalendarEventsAsync();
            var userEvents = events
                .Where(e => e.UserId == userId)
                .OrderBy(e => e.StartTime);
            
            return _mapper.Map<IEnumerable<CalendarEventResponseDto>>(userEvents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting calendar events for user ID {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<CalendarEventResponseDto>> GetPublicCalendarEventsAsync(int skip = 0, int take = 50)
    {
        try
        {
            var events = await _dataService.GetCalendarEventsAsync();
            var publicEvents = events
                .Where(e => e.IsPublic)
                .OrderBy(e => e.StartTime)
                .Skip(skip)
                .Take(take);
            
            return _mapper.Map<IEnumerable<CalendarEventResponseDto>>(publicEvents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting public calendar events");
            throw;
        }
    }

    public async Task<IEnumerable<CalendarEventResponseDto>> GetCalendarEventsByDateRangeAsync(DateTime startDate, DateTime endDate, bool publicOnly = true)
    {
        try
        {
            var events = await _dataService.GetCalendarEventsAsync();
            var filteredEvents = events.Where(e =>
                (!publicOnly || e.IsPublic) &&
                e.StartTime <= endDate && e.EndTime >= startDate
            ).OrderBy(e => e.StartTime);
            
            return _mapper.Map<IEnumerable<CalendarEventResponseDto>>(filteredEvents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting calendar events by date range");
            throw;
        }
    }

    public async Task<IEnumerable<CalendarEventResponseDto>> GetCalendarEventsByMonthAsync(int year, int month, bool publicOnly = true)
    {
        try
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            
            return await GetCalendarEventsByDateRangeAsync(startDate, endDate, publicOnly);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting calendar events for month {Year}-{Month}", year, month);
            throw;
        }
    }

    public async Task<IEnumerable<CalendarEventResponseDto>> GetCalendarEventsByWeekAsync(DateTime startOfWeek, bool publicOnly = true)
    {
        try
        {
            var endOfWeek = startOfWeek.AddDays(6);
            return await GetCalendarEventsByDateRangeAsync(startOfWeek, endOfWeek, publicOnly);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting calendar events for week starting {StartOfWeek}", startOfWeek);
            throw;
        }
    }

    public async Task<IEnumerable<CalendarEventResponseDto>> GetTodayCalendarEventsAsync(bool publicOnly = true)
    {
        try
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            
            return await GetCalendarEventsByDateRangeAsync(today, tomorrow, publicOnly);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting today's calendar events");
            throw;
        }
    }

    public async Task<IEnumerable<CalendarEventResponseDto>> SearchByTypeAsync(string type, bool publicOnly = true)
    {
        try
        {
            var events = await _dataService.GetCalendarEventsAsync();
            var searchResults = events
                .Where(e => (!publicOnly || e.IsPublic) && 
                           !string.IsNullOrEmpty(e.Type) &&
                           e.Type.Contains(type, StringComparison.OrdinalIgnoreCase))
                .OrderBy(e => e.StartTime);
            
            return _mapper.Map<IEnumerable<CalendarEventResponseDto>>(searchResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching calendar events by type {Type}", type);
            throw;
        }
    }

    public async Task<IEnumerable<CalendarEventResponseDto>> SearchCalendarEventsAsync(string keyword, bool publicOnly = true)
    {
        try
        {
            var events = await _dataService.GetCalendarEventsAsync();
            var searchResults = events.Where(e =>
                (!publicOnly || e.IsPublic) &&
                (e.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                 (!string.IsNullOrEmpty(e.Description) && e.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                 (!string.IsNullOrEmpty(e.Location) && e.Location.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                 (!string.IsNullOrEmpty(e.Type) && e.Type.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            ).OrderBy(e => e.StartTime);
            
            return _mapper.Map<IEnumerable<CalendarEventResponseDto>>(searchResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching calendar events with keyword {Keyword}", keyword);
            throw;
        }
    }

    public async Task<IEnumerable<CalendarEventResponseDto>> GetUpcomingEventsAsync(int days = 7, bool publicOnly = true)
    {
        try
        {
            var now = DateTime.Now;
            var futureDate = now.AddDays(days);
            
            var events = await _dataService.GetCalendarEventsAsync();
            var upcomingEvents = events
                .Where(e => (!publicOnly || e.IsPublic) && e.StartTime >= now && e.StartTime <= futureDate)
                .OrderBy(e => e.StartTime);
            
            return _mapper.Map<IEnumerable<CalendarEventResponseDto>>(upcomingEvents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting upcoming calendar events");
            throw;
        }
    }

    public async Task<CalendarEventResponseDto> CreateCalendarEventAsync(CreateCalendarEventDto createCalendarEventDto)
    {
        try
        {
            // 驗證時間
            if (!ValidateEventTimes(createCalendarEventDto.StartTime, createCalendarEventDto.EndTime))
            {
                throw new InvalidOperationException("事件時間不正確：結束時間不能早於開始時間");
            }

            // 檢查時間衝突
            var hasConflict = await CheckTimeConflictAsync(
                createCalendarEventDto.UserId, 
                createCalendarEventDto.StartTime, 
                createCalendarEventDto.EndTime);

            if (hasConflict)
            {
                _logger.LogWarning("Time conflict detected for user {UserId} at {StartTime}-{EndTime}", 
                    createCalendarEventDto.UserId, createCalendarEventDto.StartTime, createCalendarEventDto.EndTime);
                // 記錄警告但仍允許創建，由使用者決定
            }

            // 驗證事件類型
            if (!string.IsNullOrEmpty(createCalendarEventDto.Type) && 
                !_validEventTypes.Contains(createCalendarEventDto.Type))
            {
                _logger.LogWarning("Invalid event type provided: {Type}", createCalendarEventDto.Type);
            }

            var events = await _dataService.GetCalendarEventsAsync();
            var calendarEvent = _mapper.Map<Models.CalendarEvent>(createCalendarEventDto);
            
            // 生成新ID
            calendarEvent.Id = events.Any() ? events.Max(e => e.Id) + 1 : 1;
            
            // 設定時間戳記
            calendarEvent.CreatedAt = DateTime.UtcNow;
            calendarEvent.UpdatedAt = DateTime.UtcNow;

            events.Add(calendarEvent);
            await _dataService.SaveCalendarEventsAsync(events);

            _logger.LogInformation("Calendar event created with ID {Id} for user {UserId}", calendarEvent.Id, calendarEvent.UserId);
            return _mapper.Map<CalendarEventResponseDto>(calendarEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating calendar event");
            throw;
        }
    }

    public async Task<CalendarEventResponseDto?> UpdateCalendarEventAsync(int id, UpdateCalendarEventDto updateCalendarEventDto)
    {
        try
        {
            var events = await _dataService.GetCalendarEventsAsync();
            var existingEvent = events.FirstOrDefault(e => e.Id == id);
            
            if (existingEvent == null)
            {
                _logger.LogWarning("Calendar event with ID {Id} not found for update", id);
                return null;
            }

            // 驗證時間（如果有提供）
            var startTime = updateCalendarEventDto.StartTime ?? existingEvent.StartTime;
            var endTime = updateCalendarEventDto.EndTime ?? existingEvent.EndTime;
            
            if (!ValidateEventTimes(startTime, endTime))
            {
                throw new InvalidOperationException("事件時間不正確：結束時間不能早於開始時間");
            }

            // 檢查時間衝突（如果時間有變更）
            if (updateCalendarEventDto.StartTime.HasValue || updateCalendarEventDto.EndTime.HasValue)
            {
                var hasConflict = await CheckTimeConflictAsync(
                    existingEvent.UserId, 
                    startTime, 
                    endTime, 
                    id);

                if (hasConflict)
                {
                    _logger.LogWarning("Time conflict detected during update for event {Id}", id);
                }
            }

            // 驗證事件類型（如果有提供）
            if (!string.IsNullOrEmpty(updateCalendarEventDto.Type) && 
                !_validEventTypes.Contains(updateCalendarEventDto.Type))
            {
                _logger.LogWarning("Invalid event type provided for update: {Type}", updateCalendarEventDto.Type);
            }

            _mapper.Map(updateCalendarEventDto, existingEvent);
            existingEvent.UpdatedAt = DateTime.UtcNow;

            await _dataService.SaveCalendarEventsAsync(events);
            _logger.LogInformation("Calendar event with ID {Id} updated", id);
            
            return _mapper.Map<CalendarEventResponseDto>(existingEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating calendar event with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteCalendarEventAsync(int id)
    {
        try
        {
            var events = await _dataService.GetCalendarEventsAsync();
            var calendarEvent = events.FirstOrDefault(e => e.Id == id);
            
            if (calendarEvent == null)
            {
                _logger.LogWarning("Calendar event with ID {Id} not found for deletion", id);
                return false;
            }

            events.Remove(calendarEvent);
            await _dataService.SaveCalendarEventsAsync(events);
            _logger.LogInformation("Calendar event with ID {Id} deleted", id);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting calendar event with ID {Id}", id);
            throw;
        }
    }

    public async Task<object> GetCalendarEventStatsAsync()
    {
        try
        {
            var events = await _dataService.GetCalendarEventsAsync();
            var now = DateTime.Now;
            var today = DateTime.Today;
            var thisMonth = new DateTime(now.Year, now.Month, 1);

            var stats = new
            {
                TotalEvents = events.Count(),
                PublicEvents = events.Count(e => e.IsPublic),
                PrivateEvents = events.Count(e => !e.IsPublic),
                
                AllDayEvents = events.Count(e => e.IsAllDay),
                RecurringEvents = events.Count(e => e.IsRecurring),
                EventsWithReminders = events.Count(e => e.HasReminder),
                EventsWithLocation = events.Count(e => !string.IsNullOrEmpty(e.Location)),
                
                TodayEvents = events.Count(e => e.StartTime.Date == today),
                ThisWeekEvents = events.Count(e => e.StartTime.Date >= today && e.StartTime.Date < today.AddDays(7)),
                ThisMonthEvents = events.Count(e => e.StartTime >= thisMonth && e.StartTime < thisMonth.AddMonths(1)),
                
                PastEvents = events.Count(e => e.EndTime < now),
                UpcomingEvents = events.Count(e => e.StartTime > now),
                OngoingEvents = events.Count(e => e.StartTime <= now && e.EndTime >= now),
                
                TodayCreated = events.Count(e => e.CreatedAt.Date == today),
                ThisMonthCreated = events.Count(e => e.CreatedAt >= thisMonth),
                RecentlyUpdated = events.Count(e => e.UpdatedAt >= today.AddDays(-7)),
                
                EventsByType = events
                    .Where(e => !string.IsNullOrEmpty(e.Type))
                    .GroupBy(e => e.Type)
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10),
                
                EventsByMonth = events
                    .GroupBy(e => new { Year = e.StartTime.Year, Month = e.StartTime.Month })
                    .Select(g => new { 
                        YearMonth = $"{g.Key.Year}-{g.Key.Month:D2}", 
                        Count = g.Count() 
                    })
                    .OrderBy(x => x.YearMonth),
                
                EventsByDayOfWeek = events
                    .GroupBy(e => e.StartTime.DayOfWeek)
                    .Select(g => new { DayOfWeek = g.Key.ToString(), Count = g.Count() })
                    .OrderBy(x => (int)Enum.Parse<DayOfWeek>(x.DayOfWeek)),
                
                EventsByHour = events
                    .Where(e => !e.IsAllDay)
                    .GroupBy(e => e.StartTime.Hour)
                    .Select(g => new { Hour = g.Key, Count = g.Count() })
                    .OrderBy(x => x.Hour),
                
                AverageEventDuration = events
                    .Where(e => !e.IsAllDay)
                    .Select(e => (e.EndTime - e.StartTime).TotalMinutes)
                    .DefaultIfEmpty(0)
                    .Average(),
                
                UniqueEventTypes = events
                    .Where(e => !string.IsNullOrEmpty(e.Type))
                    .Select(e => e.Type)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count()
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting calendar event statistics");
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetEventTypesAsync(bool publicOnly = true)
    {
        try
        {
            var events = await _dataService.GetCalendarEventsAsync();
            var types = events
                .Where(e => !publicOnly || e.IsPublic)
                .Where(e => !string.IsNullOrEmpty(e.Type))
                .Select(e => e.Type!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(t => t);
            
            return types;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting event types");
            throw;
        }
    }

    public bool ValidateEventTimes(DateTime startTime, DateTime endTime)
    {
        try
        {
            // 結束時間不能早於開始時間
            if (endTime < startTime)
                return false;

            // 事件不能超過24小時（除非是全天事件）
            if ((endTime - startTime).TotalHours > 24)
                return false;

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while validating event times");
            return false;
        }
    }

    public async Task<bool> CheckTimeConflictAsync(int userId, DateTime startTime, DateTime endTime, int? excludeEventId = null)
    {
        try
        {
            var events = await _dataService.GetCalendarEventsAsync();
            var userEvents = events.Where(e => 
                e.UserId == userId && 
                (excludeEventId == null || e.Id != excludeEventId.Value));

            // 檢查是否有時間重疊的事件
            var hasConflict = userEvents.Any(e =>
                startTime < e.EndTime && endTime > e.StartTime);

            return hasConflict;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking time conflict");
            return false;
        }
    }
}