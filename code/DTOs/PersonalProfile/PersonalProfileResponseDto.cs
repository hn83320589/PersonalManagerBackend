namespace PersonalManagerAPI.DTOs.PersonalProfile;

/// <summary>
/// 個人資料回應 DTO
/// </summary>
public class PersonalProfileResponseDto
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string? Title { get; set; }
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? Website { get; set; }
    public string? Location { get; set; }
    public DateTime? Birthday { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}