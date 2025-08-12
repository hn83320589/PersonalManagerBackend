using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.Models;

public enum EventType
{
    Personal,
    Work,
    Meeting,
    Reminder,
    Other
}

public class CalendarEvent
{
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime StartTime { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    public DateTime? EndTime { get; set; }
    
    [StringLength(20)]
    public string? Color { get; set; }
    
    public bool IsAllDay { get; set; } = false;
    
    [StringLength(255)]
    public string? Location { get; set; }
    
    public bool IsPublic { get; set; } = false;
    
    public EventType EventType { get; set; } = EventType.Personal;
    
    [StringLength(255)]
    public string? GoogleEventId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public User User { get; set; } = null!;
}