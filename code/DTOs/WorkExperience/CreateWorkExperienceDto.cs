using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.DTOs.WorkExperience;

/// <summary>
/// 建立工作經歷請求 DTO
/// </summary>
public class CreateWorkExperienceDto
{
    [Required(ErrorMessage = "使用者ID為必填")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "公司名稱為必填")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "公司名稱長度必須在2-200字元之間")]
    public string Company { get; set; } = string.Empty;

    [Required(ErrorMessage = "職位名稱為必填")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "職位名稱長度必須在2-100字元之間")]
    public string Position { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "工作描述最長1000字元")]
    public string? Description { get; set; }

    [StringLength(100, ErrorMessage = "部門名稱最長100字元")]
    public string? Department { get; set; }

    [StringLength(100, ErrorMessage = "工作地點最長100字元")]
    public string? Location { get; set; }

    [Required(ErrorMessage = "開始日期為必填")]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Range(0, 99999999.99, ErrorMessage = "薪資範圍必須為正數")]
    public decimal? Salary { get; set; }

    [StringLength(20, ErrorMessage = "薪資幣別最長20字元")]
    public string? SalaryCurrency { get; set; } = "TWD";

    public bool IsCurrent { get; set; } = false;

    public bool IsPublic { get; set; } = true;

    [Range(0, 999, ErrorMessage = "排序值必須為非負整數")]
    public int SortOrder { get; set; } = 0;
}