using System.ComponentModel.DataAnnotations;

namespace PersonalManager.Api.Models;

public class Project
{
    public int Id { get; set; }
    public int UserId { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [StringLength(20)]
    public string Color { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
