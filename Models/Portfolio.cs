using System.ComponentModel.DataAnnotations;

namespace PersonalManager.Api.Models;

public class Portfolio
{
    public int Id { get; set; }
    public int UserId { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [StringLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    [StringLength(500)]
    public string ProjectUrl { get; set; } = string.Empty;

    [StringLength(500)]
    public string RepositoryUrl { get; set; } = string.Empty;

    public string Technologies { get; set; } = string.Empty;

    public bool IsFeatured { get; set; }
    public bool IsPublic { get; set; } = true;
    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
