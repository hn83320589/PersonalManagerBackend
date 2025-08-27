using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.DTOs.GuestBookEntry;

/// <summary>
/// 建立留言 DTO
/// </summary>
public class CreateGuestBookEntryDto
{
    /// <summary>
    /// 留言者姓名
    /// </summary>
    [Required(ErrorMessage = "姓名為必填項目")]
    [StringLength(100, ErrorMessage = "姓名長度不能超過 100 個字元")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 留言者電子郵件
    /// </summary>
    [EmailAddress(ErrorMessage = "電子郵件格式不正確")]
    [StringLength(100, ErrorMessage = "電子郵件長度不能超過 100 個字元")]
    public string? Email { get; set; }

    /// <summary>
    /// 留言者網站
    /// </summary>
    [Url(ErrorMessage = "網站 URL 格式不正確")]
    [StringLength(255, ErrorMessage = "網站 URL 長度不能超過 255 個字元")]
    public string? Website { get; set; }

    /// <summary>
    /// 留言內容
    /// </summary>
    [Required(ErrorMessage = "留言內容為必填項目")]
    [StringLength(1000, ErrorMessage = "留言內容長度不能超過 1000 個字元")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 父留言 ID（用於回覆）
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// 是否公開顯示
    /// </summary>
    public bool IsPublic { get; set; } = true;
}