namespace PersonalManagerAPI.DTOs.WorkTask;

public class WorkTaskResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CompletedDate { get; set; }
    public decimal? EstimatedHours { get; set; }
    public decimal? ActualHours { get; set; }
    public string? ProjectId { get; set; }
    public string? Project { get; set; }
    public string? Tags { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // 計算屬性
    public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.Now && Status != "Completed";
    public bool IsActive => Status == "InProgress" || Status == "Planning";
    public int? DaysUntilDue => DueDate.HasValue ? (int?)(DueDate.Value - DateTime.Now).TotalDays : null;
    public decimal? ProgressPercentage => EstimatedHours.HasValue && ActualHours.HasValue && EstimatedHours.Value > 0 
        ? Math.Min(100, (ActualHours.Value / EstimatedHours.Value) * 100) : null;
    public decimal? TimeVariance => EstimatedHours.HasValue && ActualHours.HasValue 
        ? ActualHours.Value - EstimatedHours.Value : null;
}