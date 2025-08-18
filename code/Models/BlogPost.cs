using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.Models;

public class BlogPost
{
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public string? Summary { get; set; }
    
    public bool IsPublished { get; set; } = false;
    
    public bool IsPublic { get; set; } = false;
    
    public DateTime? PublishedAt { get; set; }
    
    public DateTime? PublishedDate { get; set; }
    
    [StringLength(200)]
    public string? Slug { get; set; }
    
    [Url]
    [StringLength(255)]
    public string? FeaturedImageUrl { get; set; }
    
    public string? Tags { get; set; }
    
    [StringLength(50)]
    public string? Category { get; set; }
    
    public int ViewCount { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public User User { get; set; } = null!;
}