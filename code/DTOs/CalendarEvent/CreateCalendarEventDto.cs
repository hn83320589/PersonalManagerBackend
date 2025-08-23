using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.DTOs.CalendarEvent;

/// <summary>
/// 建立行事曆事件請求 DTO
/// </summary>
public class CreateCalendarEventDto
{
    [Required(ErrorMessage = "使用者ID為必填")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "事件標題為必填")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "事件標題長度必須在1-200字元之間")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "事件描述最長2000字元")]
    public string? Description { get; set; }

    [StringLength(100, ErrorMessage = "事件位置最長100字元")]
    public string? Location { get; set; }

    [StringLength(50, ErrorMessage = "事件類型最長50字元")]
    public string? Type { get; set; }

    [Required(ErrorMessage = "開始時間為必填")]
    public DateTime StartTime { get; set; }

    [Required(ErrorMessage = "結束時間為必填")]
    public DateTime EndTime { get; set; }

    public bool IsAllDay { get; set; } = false;

    public bool IsRecurring { get; set; } = false;

    [StringLength(50, ErrorMessage = "重複模式最長50字元")]
    public string? RecurrencePattern { get; set; }

    [StringLength(20, ErrorMessage = "顏色代碼最長20字元")]
    public string? Color { get; set; } = "#007bff";

    public bool IsPublic { get; set; } = false;

    public bool HasReminder { get; set; } = false;

    [Range(0, 10080, ErrorMessage = "提醒時間必須在0-10080分鐘之間")] // 最多一週
    public int? ReminderMinutes { get; set; }

    [StringLength(500, ErrorMessage = "外部URL最長500字元")]
    public string? ExternalUrl { get; set; }
}