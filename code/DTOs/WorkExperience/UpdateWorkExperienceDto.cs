using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.DTOs.WorkExperience;

/// <summary>
/// 更新工作經歷請求 DTO
/// </summary>
public class UpdateWorkExperienceDto
{
    [StringLength(200, MinimumLength = 2, ErrorMessage = "公司名稱長度必須在2-200字元之間")]
    public string? Company { get; set; }

    [StringLength(100, MinimumLength = 2, ErrorMessage = "職位名稱長度必須在2-100字元之間")]
    public string? Position { get; set; }

    [StringLength(1000, ErrorMessage = "工作描述最長1000字元")]
    public string? Description { get; set; }

    [StringLength(100, ErrorMessage = "部門名稱最長100字元")]
    public string? Department { get; set; }

    [StringLength(100, ErrorMessage = "工作地點最長100字元")]
    public string? Location { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Range(0, 99999999.99, ErrorMessage = "薪資範圍必須為正數")]
    public decimal? Salary { get; set; }

    [StringLength(20, ErrorMessage = "薪資幣別最長20字元")]
    public string? SalaryCurrency { get; set; }

    public bool? IsCurrent { get; set; }

    public bool? IsPublic { get; set; }

    [Range(0, 999, ErrorMessage = "排序值必須為非負整數")]
    public int? SortOrder { get; set; }
}