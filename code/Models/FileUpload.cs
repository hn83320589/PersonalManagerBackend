using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.Models;

public class FileUpload
{
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string OriginalFileName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public int? UserId { get; set; }

    [StringLength(50)]
    public string Category { get; set; } = string.Empty; // images, videos, documents

    [StringLength(200)]
    public string Description { get; set; } = string.Empty;

    public bool IsPublic { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User? User { get; set; }
}

public enum FileCategory
{
    Image,
    Video,
    Document,
    Audio,
    Other
}

public enum FileStatus
{
    Uploaded,
    Processing,
    Completed,
    Failed
}