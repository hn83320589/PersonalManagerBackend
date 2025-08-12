using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.Models;

public class PersonalProfile
{
    public int Id { get; set; }
    
    public int? UserId { get; set; }
    
    [StringLength(100)]
    public string? Title { get; set; }
    
    public string? Summary { get; set; }
    
    public string? Description { get; set; }
    
    [Url]
    [StringLength(255)]
    public string? ProfileImageUrl { get; set; }
    
    [Url]
    [StringLength(255)]
    public string? Website { get; set; }
    
    [StringLength(100)]
    public string? Location { get; set; }
    
    public DateTime? Birthday { get; set; }
    
    public bool IsPublic { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public User? User { get; set; }
}