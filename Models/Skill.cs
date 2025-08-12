using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.Models;

public enum SkillLevel
{
    Beginner,
    Intermediate,
    Advanced,
    Expert
}

public class Skill
{
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string? Category { get; set; }
    
    public SkillLevel Level { get; set; } = SkillLevel.Intermediate;
    
    public string? Description { get; set; }
    
    public bool IsPublic { get; set; } = true;
    
    public int SortOrder { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public User User { get; set; } = null!;
}