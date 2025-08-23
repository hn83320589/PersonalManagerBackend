namespace PersonalManagerAPI.DTOs.TodoItem;

public class TodoItemResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public string Priority { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? Category { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // 計算屬性
    public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.Now && !IsCompleted;
    public bool IsDueSoon => DueDate.HasValue && DueDate.Value <= DateTime.Now.AddDays(3) && DueDate.Value >= DateTime.Now && !IsCompleted;
    public int? DaysUntilDue => DueDate.HasValue ? (int?)(DueDate.Value - DateTime.Now).TotalDays : null;
}