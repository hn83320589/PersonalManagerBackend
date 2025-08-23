namespace PersonalManagerAPI.DTOs.BlogPost;

/// <summary>
/// 部落格文章回應 DTO
/// </summary>
public class BlogPostResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public bool IsPublished { get; set; }
    public bool IsPublic { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? PublishedDate { get; set; }
    public string? Slug { get; set; }
    public string? FeaturedImageUrl { get; set; }
    public string? Tags { get; set; }
    public string? Category { get; set; }
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}