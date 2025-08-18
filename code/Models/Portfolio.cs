using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.Models;

public class Portfolio
{
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public string? TechnologyUsed { get; set; }
    
    public string? Technologies { get; set; }
    
    [Url]
    [StringLength(255)]
    public string? RepositoryUrl { get; set; }
    
    [Url]
    [StringLength(255)]
    public string? ProjectUrl { get; set; }
    
    [Url]
    [StringLength(255)]
    public string? GithubUrl { get; set; }
    
    [Url]
    [StringLength(255)]
    public string? ImageUrl { get; set; }
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    public bool IsPublic { get; set; } = true;
    
    public bool IsFeatured { get; set; } = false;
    
    public int SortOrder { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public User User { get; set; } = null!;
}