namespace PersonalManagerAPI.DTOs.CalendarEvent;

/// <summary>
/// 行事曆事件回應 DTO
/// </summary>
public class CalendarEventResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string? Type { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAllDay { get; set; }
    public bool IsRecurring { get; set; }
    public string? RecurrencePattern { get; set; }
    public string? Color { get; set; }
    public bool IsPublic { get; set; }
    public bool HasReminder { get; set; }
    public int? ReminderMinutes { get; set; }
    public string? ExternalUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}