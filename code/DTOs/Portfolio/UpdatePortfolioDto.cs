using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.DTOs.Portfolio;

/// <summary>
/// 更新作品請求 DTO
/// </summary>
public class UpdatePortfolioDto
{
    [StringLength(200, MinimumLength = 2, ErrorMessage = "作品標題長度必須在2-200字元之間")]
    public string? Title { get; set; }

    [StringLength(2000, ErrorMessage = "作品描述最長2000字元")]
    public string? Description { get; set; }

    [StringLength(50, ErrorMessage = "作品類型最長50字元")]
    public string? Type { get; set; }

    [StringLength(500, ErrorMessage = "技術清單最長500字元")]
    public string? Technologies { get; set; }

    [Url(ErrorMessage = "專案網址格式不正確")]
    [StringLength(500, ErrorMessage = "專案網址最長500字元")]
    public string? ProjectUrl { get; set; }

    [Url(ErrorMessage = "原始碼網址格式不正確")]
    [StringLength(500, ErrorMessage = "原始碼網址最長500字元")]
    public string? RepositoryUrl { get; set; }

    [StringLength(500, ErrorMessage = "圖片網址最長500字元")]
    public string? ImageUrl { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool? IsFeatured { get; set; }

    public bool? IsPublic { get; set; }

    [Range(0, 999, ErrorMessage = "排序值必須為非負整數")]
    public int? SortOrder { get; set; }
}