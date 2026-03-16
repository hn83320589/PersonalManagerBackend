using System.ComponentModel.DataAnnotations;

namespace PersonalManager.Api.Models;

public class TimeEntry
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? WorkTaskId { get; set; }

    [Required, StringLength(200)]
    public string Task { get; set; } = string.Empty;

    [StringLength(100)]
    public string Project { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Date { get; set; } = string.Empty;  // ISO date "2026-03-16"

    [StringLength(10)]
    public string? StartTime { get; set; }

    [StringLength(10)]
    public string? EndTime { get; set; }

    public int Duration { get; set; }  // minutes

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
