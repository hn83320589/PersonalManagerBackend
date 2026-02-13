using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PersonalManager.Api.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BlogPostStatus
{
    Draft,
    Published,
    Archived
}

public class BlogPost
{
    public int Id { get; set; }
    public int UserId { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(200)]
    public string Slug { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    [StringLength(500)]
    public string Summary { get; set; } = string.Empty;

    [StringLength(50)]
    public string Category { get; set; } = string.Empty;

    public string Tags { get; set; } = string.Empty;

    public BlogPostStatus Status { get; set; } = BlogPostStatus.Draft;
    public bool IsPublic { get; set; } = true;
    public int ViewCount { get; set; }

    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
