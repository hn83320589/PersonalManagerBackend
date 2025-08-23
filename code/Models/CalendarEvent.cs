using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.Models;

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
    public DateTime StartTime { get; set; }
    
    [Required]
    public DateTime EndTime { get; set; }
    
    [StringLength(20)]
    public string? Color { get; set; }
    
    public bool IsAllDay { get; set; } = false;
    
    [StringLength(255)]
    public string? Location { get; set; }
    
    public bool IsPublic { get; set; } = false;
    
    [StringLength(50)]
    public string? Type { get; set; }
    
    public bool IsRecurring { get; set; } = false;
    
    [StringLength(100)]
    public string? RecurrencePattern { get; set; }
    
    public bool HasReminder { get; set; } = false;
    
    public int? ReminderMinutes { get; set; }
    
    [StringLength(500)]
    public string? ExternalUrl { get; set; }
    
    [StringLength(255)]
    public string? GoogleEventId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public User User { get; set; } = null!;
}