using System.ComponentModel.DataAnnotations;
using PersonalManagerAPI.Models;

namespace PersonalManagerAPI.DTOs.Skill;

/// <summary>
/// 建立技能 DTO
/// </summary>
public class CreateSkillDto
{
    [Required(ErrorMessage = "使用者ID為必填")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "技能名稱為必填")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "技能名稱長度須介於2-100字元")]
    public string Name { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "分類最長50字元")]
    public string? Category { get; set; }

    [Required(ErrorMessage = "技能等級為必填")]
    public SkillLevel Level { get; set; } = SkillLevel.Intermediate;

    [StringLength(1000, ErrorMessage = "描述最長1000字元")]
    public string? Description { get; set; }

    public bool IsPublic { get; set; } = true;

    public int SortOrder { get; set; } = 0;
}