using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.Models;

public enum ContactType
{
    Email,
    Phone,
    LinkedIn,
    GitHub,
    Facebook,
    Twitter,
    Instagram,
    Other
}

public class ContactMethod
{
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [Required]
    public ContactType Type { get; set; }
    
    [StringLength(50)]
    public string? Icon { get; set; }
    
    [Required]
    [StringLength(255)]
    public string Value { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? Label { get; set; }
    
    public bool IsPublic { get; set; } = true;
    
    public bool IsPreferred { get; set; } = false;
    
    public int SortOrder { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public User User { get; set; } = null!;
}