namespace PersonalManagerAPI.DTOs.WorkExperience;

/// <summary>
/// 工作經歷回應 DTO
/// </summary>
public class WorkExperienceResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Company { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Department { get; set; }
    public string? Location { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Salary { get; set; }
    public string? SalaryCurrency { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsPublic { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}