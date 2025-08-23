using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.Models;

public class WorkExperience
{
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Company { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Position { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [StringLength(100)]
    public string? Department { get; set; }
    
    [StringLength(100)]
    public string? Location { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    public decimal? Salary { get; set; }
    
    [StringLength(20)]
    public string? SalaryCurrency { get; set; } = "TWD";
    
    public bool IsCurrent { get; set; } = false;
    
    public string? Achievements { get; set; }
    
    public bool IsPublic { get; set; } = true;
    
    public int SortOrder { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public User User { get; set; } = null!;
}