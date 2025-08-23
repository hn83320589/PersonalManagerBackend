using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.DTOs.Education;

/// <summary>
/// 建立學歷 DTO
/// </summary>
public class CreateEducationDto
{
    [Required(ErrorMessage = "使用者ID為必填")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "學校名稱為必填")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "學校名稱長度須介於2-200字元")]
    public string School { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "學位最長100字元")]
    public string? Degree { get; set; }

    [StringLength(100, ErrorMessage = "領域最長100字元")]
    public string? FieldOfStudy { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [StringLength(1000, ErrorMessage = "描述最長1000字元")]
    public string? Description { get; set; }

    public bool IsPublic { get; set; } = true;

    public int SortOrder { get; set; } = 0;
}