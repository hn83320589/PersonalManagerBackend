using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PersonalManager.Api.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ContactType
{
    Email,
    Phone,
    LinkedIn,
    GitHub,
    Facebook,
    Twitter,
    Instagram,
    Discord,
    Other
}

public class ContactMethod
{
    public int Id { get; set; }
    public int UserId { get; set; }

    public ContactType Type { get; set; }

    [StringLength(50)]
    public string Label { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Value { get; set; } = string.Empty;

    [StringLength(50)]
    public string Icon { get; set; } = string.Empty;

    public bool IsPublic { get; set; } = true;
    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
