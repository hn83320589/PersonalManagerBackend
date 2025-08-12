using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.Models;

public class User
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string? FirstName { get; set; }
    
    [StringLength(50)]
    public string? LastName { get; set; }
    
    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public PersonalProfile? PersonalProfile { get; set; }
    public ICollection<Education> Educations { get; set; } = new List<Education>();
    public ICollection<WorkExperience> WorkExperiences { get; set; } = new List<WorkExperience>();
    public ICollection<Skill> Skills { get; set; } = new List<Skill>();
    public ICollection<Portfolio> Portfolios { get; set; } = new List<Portfolio>();
    public ICollection<CalendarEvent> CalendarEvents { get; set; } = new List<CalendarEvent>();
    public ICollection<WorkTask> WorkTasks { get; set; } = new List<WorkTask>();
    public ICollection<TodoItem> TodoItems { get; set; } = new List<TodoItem>();
    public ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
    public ICollection<ContactMethod> ContactMethods { get; set; } = new List<ContactMethod>();
}