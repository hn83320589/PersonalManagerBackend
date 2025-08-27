namespace PersonalManagerAPI.DTOs.GuestBookEntry;

/// <summary>
/// 留言回應 DTO
/// </summary>
public class GuestBookEntryResponseDto
{
    /// <summary>
    /// 留言 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 留言者姓名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 留言者電子郵件（僅管理員可見）
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 留言者網站
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// 留言內容
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 父留言 ID
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// 是否已審核通過
    /// </summary>
    public bool IsApproved { get; set; }

    /// <summary>
    /// 是否公開顯示
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// IP 位址（僅管理員可見）
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// 瀏覽器資訊（僅管理員可見）
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 回覆留言列表
    /// </summary>
    public List<GuestBookEntryResponseDto> Replies { get; set; } = new();

    /// <summary>
    /// 回覆數量
    /// </summary>
    public int ReplyCount { get; set; }
}