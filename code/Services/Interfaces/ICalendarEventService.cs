using PersonalManagerAPI.DTOs.CalendarEvent;
using PersonalManagerAPI.Models;

namespace PersonalManagerAPI.Services.Interfaces;

/// <summary>
/// 行事曆事件服務介面
/// </summary>
public interface ICalendarEventService
{
    /// <summary>
    /// 取得所有行事曆事件（分頁）
    /// </summary>
    Task<IEnumerable<CalendarEventResponseDto>> GetAllCalendarEventsAsync(int skip = 0, int take = 50);

    /// <summary>
    /// 根據ID取得行事曆事件
    /// </summary>
    Task<CalendarEventResponseDto?> GetCalendarEventByIdAsync(int id);

    /// <summary>
    /// 根據使用者ID取得行事曆事件列表
    /// </summary>
    Task<IEnumerable<CalendarEventResponseDto>> GetCalendarEventsByUserIdAsync(int userId);

    /// <summary>
    /// 取得公開的行事曆事件（分頁）
    /// </summary>
    Task<IEnumerable<CalendarEventResponseDto>> GetPublicCalendarEventsAsync(int skip = 0, int take = 50);

    /// <summary>
    /// 根據日期範圍查詢行事曆事件
    /// </summary>
    Task<IEnumerable<CalendarEventResponseDto>> GetCalendarEventsByDateRangeAsync(DateTime startDate, DateTime endDate, bool publicOnly = true);

    /// <summary>
    /// 根據月份查詢行事曆事件
    /// </summary>
    Task<IEnumerable<CalendarEventResponseDto>> GetCalendarEventsByMonthAsync(int year, int month, bool publicOnly = true);

    /// <summary>
    /// 根據週期查詢行事曆事件
    /// </summary>
    Task<IEnumerable<CalendarEventResponseDto>> GetCalendarEventsByWeekAsync(DateTime startOfWeek, bool publicOnly = true);

    /// <summary>
    /// 根據今日查詢行事曆事件
    /// </summary>
    Task<IEnumerable<CalendarEventResponseDto>> GetTodayCalendarEventsAsync(bool publicOnly = true);

    /// <summary>
    /// 根據類型搜尋行事曆事件
    /// </summary>
    Task<IEnumerable<CalendarEventResponseDto>> SearchByTypeAsync(string type, bool publicOnly = true);

    /// <summary>
    /// 根據關鍵字搜尋行事曆事件（標題、描述）
    /// </summary>
    Task<IEnumerable<CalendarEventResponseDto>> SearchCalendarEventsAsync(string keyword, bool publicOnly = true);

    /// <summary>
    /// 取得即將到來的事件
    /// </summary>
    Task<IEnumerable<CalendarEventResponseDto>> GetUpcomingEventsAsync(int days = 7, bool publicOnly = true);

    /// <summary>
    /// 建立新的行事曆事件
    /// </summary>
    Task<CalendarEventResponseDto> CreateCalendarEventAsync(CreateCalendarEventDto createCalendarEventDto);

    /// <summary>
    /// 更新行事曆事件
    /// </summary>
    Task<CalendarEventResponseDto?> UpdateCalendarEventAsync(int id, UpdateCalendarEventDto updateCalendarEventDto);

    /// <summary>
    /// 刪除行事曆事件
    /// </summary>
    Task<bool> DeleteCalendarEventAsync(int id);

    /// <summary>
    /// 取得行事曆事件統計資訊
    /// </summary>
    Task<object> GetCalendarEventStatsAsync();

    /// <summary>
    /// 取得所有事件類型
    /// </summary>
    Task<IEnumerable<string>> GetEventTypesAsync(bool publicOnly = true);

    /// <summary>
    /// 驗證事件時間
    /// </summary>
    bool ValidateEventTimes(DateTime startTime, DateTime endTime);

    /// <summary>
    /// 檢查時間衝突
    /// </summary>
    Task<bool> CheckTimeConflictAsync(int userId, DateTime startTime, DateTime endTime, int? excludeEventId = null);
}