using System.ComponentModel.DataAnnotations;

namespace PersonalManager.Api.Models;

public class CalendarEvent
{
    public int Id { get; set; }
    public int UserId { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public bool IsAllDay { get; set; }
    public bool IsPublic { get; set; }

    [StringLength(20)]
    public string Color { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
