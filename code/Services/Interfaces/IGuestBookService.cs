using PersonalManagerAPI.DTOs.GuestBookEntry;

namespace PersonalManagerAPI.Services.Interfaces;

/// <summary>
/// 留言板服務介面
/// </summary>
public interface IGuestBookService
{
    /// <summary>
    /// 取得所有留言（分頁）
    /// </summary>
    Task<IEnumerable<GuestBookEntryResponseDto>> GetAllEntriesAsync(int skip = 0, int take = 50, bool includeReplies = true);

    /// <summary>
    /// 根據 ID 取得留言
    /// </summary>
    Task<GuestBookEntryResponseDto?> GetEntryByIdAsync(int id);

    /// <summary>
    /// 取得公開留言
    /// </summary>
    Task<IEnumerable<GuestBookEntryResponseDto>> GetPublicEntriesAsync(int skip = 0, int take = 50);

    /// <summary>
    /// 取得待審核留言
    /// </summary>
    Task<IEnumerable<GuestBookEntryResponseDto>> GetPendingEntriesAsync();

    /// <summary>
    /// 取得指定留言的回覆
    /// </summary>
    Task<IEnumerable<GuestBookEntryResponseDto>> GetRepliesAsync(int parentId);

    /// <summary>
    /// 取得最新留言
    /// </summary>
    Task<IEnumerable<GuestBookEntryResponseDto>> GetRecentEntriesAsync(int limit = 5);

    /// <summary>
    /// 建立新留言
    /// </summary>
    Task<GuestBookEntryResponseDto> CreateEntryAsync(CreateGuestBookEntryDto createEntryDto, string? ipAddress = null, string? userAgent = null);

    /// <summary>
    /// 建立回覆留言
    /// </summary>
    Task<GuestBookEntryResponseDto?> CreateReplyAsync(int parentId, CreateGuestBookEntryDto createEntryDto, string? ipAddress = null, string? userAgent = null);

    /// <summary>
    /// 更新留言
    /// </summary>
    Task<GuestBookEntryResponseDto?> UpdateEntryAsync(int id, UpdateGuestBookEntryDto updateEntryDto);

    /// <summary>
    /// 刪除留言
    /// </summary>
    Task<bool> DeleteEntryAsync(int id);

    /// <summary>
    /// 審核留言
    /// </summary>
    Task<bool> ApproveEntryAsync(int id);

    /// <summary>
    /// 拒絕留言
    /// </summary>
    Task<bool> RejectEntryAsync(int id);

    /// <summary>
    /// 批量審核留言
    /// </summary>
    Task<bool> BatchApproveEntriesAsync(IEnumerable<int> entryIds);

    /// <summary>
    /// 批量刪除留言
    /// </summary>
    Task<bool> BatchDeleteEntriesAsync(IEnumerable<int> entryIds);

    /// <summary>
    /// 搜尋留言
    /// </summary>
    Task<IEnumerable<GuestBookEntryResponseDto>> SearchEntriesAsync(string keyword);

    /// <summary>
    /// 根據日期範圍搜尋留言
    /// </summary>
    Task<IEnumerable<GuestBookEntryResponseDto>> GetEntriesByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// 檢查留言是否存在
    /// </summary>
    Task<bool> EntryExistsAsync(int id);

    /// <summary>
    /// 取得留言統計資訊
    /// </summary>
    Task<object> GetEntryStatsAsync();
}