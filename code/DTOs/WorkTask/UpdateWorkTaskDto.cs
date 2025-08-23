using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.DTOs.WorkTask;

public class UpdateWorkTaskDto
{
    [StringLength(200, MinimumLength = 1, ErrorMessage = "標題長度必須在1-200字元之間")]
    public string? Title { get; set; }

    [StringLength(2000, ErrorMessage = "描述不可超過2000字元")]
    public string? Description { get; set; }

    [StringLength(20, ErrorMessage = "狀態長度不可超過20字元")]
    public string? Status { get; set; }

    [StringLength(20, ErrorMessage = "優先級長度不可超過20字元")]
    public string? Priority { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? DueDate { get; set; }

    [Range(0, 9999.99, ErrorMessage = "預估時數必須在0-9999.99之間")]
    public decimal? EstimatedHours { get; set; }

    [Range(0, 9999.99, ErrorMessage = "實際時數必須在0-9999.99之間")]
    public decimal? ActualHours { get; set; }

    [StringLength(100, ErrorMessage = "專案ID長度不可超過100字元")]
    public string? ProjectId { get; set; }

    [StringLength(100, ErrorMessage = "專案名稱長度不可超過100字元")]
    public string? Project { get; set; }

    [StringLength(500, ErrorMessage = "標籤長度不可超過500字元")]
    public string? Tags { get; set; }
}