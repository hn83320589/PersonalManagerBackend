using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PersonalManager.Api.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TodoPriority
{
    Low,
    Medium,
    High
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TodoStatus
{
    Pending,
    InProgress,
    Completed
}

public class TodoItem
{
    public int Id { get; set; }
    public int UserId { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public TodoPriority Priority { get; set; } = TodoPriority.Medium;
    public TodoStatus Status { get; set; } = TodoStatus.Pending;

    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
