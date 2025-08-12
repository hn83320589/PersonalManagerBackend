using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.Models;

public class WorkExperience
{
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Company { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Position { get; set; } = string.Empty;
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    public bool IsCurrent { get; set; } = false;
    
    public string? Description { get; set; }
    
    public string? Achievements { get; set; }
    
    public bool IsPublic { get; set; } = true;
    
    public int SortOrder { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public User User { get; set; } = null!;
}