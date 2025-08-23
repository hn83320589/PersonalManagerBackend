using PersonalManagerAPI.Models;

namespace PersonalManagerAPI.DTOs.Skill;

/// <summary>
/// 技能回應 DTO
/// </summary>
public class SkillResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public SkillLevel Level { get; set; }
    public string? Description { get; set; }
    public bool IsPublic { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}