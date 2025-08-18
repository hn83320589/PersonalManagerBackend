using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.Models;

public enum TaskStatus
{
    Pending,
    Planning,
    InProgress,
    Testing,
    Completed,
    OnHold,
    Cancelled
}

public enum TaskPriority
{
    Low,
    Medium,
    High,
    Urgent
}

public class WorkTask
{
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public TaskStatus Status { get; set; } = TaskStatus.Pending;
    
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? DueDate { get; set; }
    
    public DateTime? CompletedAt { get; set; }
    
    public DateTime? CompletedDate { get; set; }
    
    public decimal? EstimatedHours { get; set; }
    
    public decimal? ActualHours { get; set; }
    
    [StringLength(100)]
    public string? ProjectId { get; set; }
    
    [StringLength(100)]
    public string? Project { get; set; }
    
    public string? Tags { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public User User { get; set; } = null!;
}