namespace PersonalManagerAPI.DTOs.Education;

/// <summary>
/// 學歷回應 DTO
/// </summary>
public class EducationResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string School { get; set; } = string.Empty;
    public string? Degree { get; set; }
    public string? FieldOfStudy { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Description { get; set; }
    public bool IsPublic { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}