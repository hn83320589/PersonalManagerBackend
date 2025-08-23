using System.ComponentModel.DataAnnotations;
using PersonalManagerAPI.Models;

namespace PersonalManagerAPI.DTOs.Skill;

/// <summary>
/// 更新技能 DTO
/// </summary>
public class UpdateSkillDto
{
    [StringLength(100, MinimumLength = 2, ErrorMessage = "技能名稱長度須介於2-100字元")]
    public string? Name { get; set; }

    [StringLength(50, ErrorMessage = "分類最長50字元")]
    public string? Category { get; set; }

    public SkillLevel? Level { get; set; }

    [StringLength(1000, ErrorMessage = "描述最長1000字元")]
    public string? Description { get; set; }

    public bool? IsPublic { get; set; }

    public int? SortOrder { get; set; }
}