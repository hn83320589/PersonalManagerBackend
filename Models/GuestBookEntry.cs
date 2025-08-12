using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.Models;

public class GuestBookEntry
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }
    
    [Url]
    [StringLength(255)]
    public string? Website { get; set; }
    
    [Required]
    public string Message { get; set; } = string.Empty;
    
    public int? ParentId { get; set; }
    
    public bool IsApproved { get; set; } = false;
    
    public bool IsPublic { get; set; } = true;
    
    [StringLength(45)]
    public string? IpAddress { get; set; }
    
    [StringLength(500)]
    public string? UserAgent { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}