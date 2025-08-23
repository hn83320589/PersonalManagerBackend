using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.DTOs.BlogPost;

/// <summary>
/// 建立部落格文章的 DTO
/// </summary>
public class CreateBlogPostDto
{
    [Required(ErrorMessage = "使用者ID為必填")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "標題為必填")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "標題長度必須在2-200字元之間")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "內容為必填")]
    [MinLength(10, ErrorMessage = "內容長度至少10字元")]
    public string Content { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "摘要長度不能超過500字元")]
    public string? Summary { get; set; }

    public bool IsPublished { get; set; } = false;

    public bool IsPublic { get; set; } = false;

    [StringLength(200, ErrorMessage = "Slug長度不能超過200字元")]
    public string? Slug { get; set; }

    [Url(ErrorMessage = "特色圖片必須是有效的URL")]
    [StringLength(255, ErrorMessage = "特色圖片URL長度不能超過255字元")]
    public string? FeaturedImageUrl { get; set; }

    [StringLength(1000, ErrorMessage = "標籤長度不能超過1000字元")]
    public string? Tags { get; set; }

    [StringLength(50, ErrorMessage = "分類長度不能超過50字元")]
    public string? Category { get; set; }

    public DateTime? PublishedAt { get; set; }
}