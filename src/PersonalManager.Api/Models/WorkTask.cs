using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PersonalManager.Api.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WorkTaskStatus
{
    Pending,
    Planning,
    InProgress,
    Testing,
    Completed,
    OnHold,
    Cancelled
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WorkTaskPriority
{
    Low,
    Medium,
    High,
    Urgent
}

public class WorkTask
{
    public int Id { get; set; }
    public int UserId { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [StringLength(100)]
    public string Project { get; set; } = string.Empty;

    public WorkTaskPriority Priority { get; set; } = WorkTaskPriority.Medium;
    public WorkTaskStatus Status { get; set; } = WorkTaskStatus.Pending;

    public double EstimatedHours { get; set; }
    public double ActualHours { get; set; }

    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }

    public string Tags { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
