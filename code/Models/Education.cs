using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.Models;

public class Education
{
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string School { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? Degree { get; set; }
    
    [StringLength(100)]
    public string? FieldOfStudy { get; set; }
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    public string? Description { get; set; }
    
    public bool IsPublic { get; set; } = true;
    
    public int SortOrder { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public User User { get; set; } = null!;
}