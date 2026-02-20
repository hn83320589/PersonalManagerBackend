using System.ComponentModel.DataAnnotations;

namespace PersonalManager.Api.Models;

public class Education
{
    public int Id { get; set; }
    public int UserId { get; set; }

    [Required, StringLength(200)]
    public string School { get; set; } = string.Empty;

    [StringLength(100)]
    public string Degree { get; set; } = string.Empty;

    [StringLength(200)]
    public string FieldOfStudy { get; set; } = string.Empty;

    public int? StartYear { get; set; }
    public int? EndYear { get; set; }

    public string Description { get; set; } = string.Empty;

    public bool IsPublic { get; set; } = true;
    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
