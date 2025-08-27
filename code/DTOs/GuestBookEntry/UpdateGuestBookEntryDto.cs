using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.DTOs.GuestBookEntry;

/// <summary>
/// 更新留言 DTO
/// </summary>
public class UpdateGuestBookEntryDto
{
    /// <summary>
    /// 留言內容
    /// </summary>
    [StringLength(1000, ErrorMessage = "留言內容長度不能超過 1000 個字元")]
    public string? Message { get; set; }

    /// <summary>
    /// 是否已審核通過
    /// </summary>
    public bool? IsApproved { get; set; }

    /// <summary>
    /// 是否公開顯示
    /// </summary>
    public bool? IsPublic { get; set; }
}