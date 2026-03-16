using System.ComponentModel.DataAnnotations;

namespace PersonalManager.Api.Models;

public class RefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; }

    [Required, StringLength(256)]
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
