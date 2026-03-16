using System.ComponentModel.DataAnnotations;

namespace PersonalManager.Api.Models;

public class Tag
{
    public int Id { get; set; }
    public int UserId { get; set; }

    [Required, StringLength(50)]
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
