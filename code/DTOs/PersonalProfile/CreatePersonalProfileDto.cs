using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.DTOs.PersonalProfile;

/// <summary>
/// 建立個人資料 DTO
/// </summary>
public class CreatePersonalProfileDto
{
    [Required(ErrorMessage = "使用者ID為必填")]
    public int UserId { get; set; }

    [StringLength(100, ErrorMessage = "標題最長100字元")]
    public string? Title { get; set; }

    [StringLength(1000, ErrorMessage = "摘要最長1000字元")]
    public string? Summary { get; set; }

    [StringLength(2000, ErrorMessage = "描述最長2000字元")]
    public string? Description { get; set; }

    [StringLength(500, ErrorMessage = "位置資訊最長500字元")]
    public string? Location { get; set; }

    [StringLength(255, ErrorMessage = "個人網站最長255字元")]
    [Url(ErrorMessage = "個人網站格式不正確")]
    public string? Website { get; set; }

    [StringLength(255, ErrorMessage = "個人圖片URL最長255字元")]
    [Url(ErrorMessage = "個人圖片URL格式不正確")]
    public string? ProfileImageUrl { get; set; }

    public DateTime? Birthday { get; set; }

    public bool IsPublic { get; set; } = true;
}