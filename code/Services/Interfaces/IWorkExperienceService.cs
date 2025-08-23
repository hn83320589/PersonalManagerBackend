using PersonalManagerAPI.DTOs.WorkExperience;
using PersonalManagerAPI.Models;

namespace PersonalManagerAPI.Services.Interfaces;

/// <summary>
/// 工作經歷服務介面
/// </summary>
public interface IWorkExperienceService
{
    /// <summary>
    /// 取得所有工作經歷（分頁）
    /// </summary>
    Task<IEnumerable<WorkExperienceResponseDto>> GetAllWorkExperiencesAsync(int skip = 0, int take = 50);

    /// <summary>
    /// 根據ID取得工作經歷
    /// </summary>
    Task<WorkExperienceResponseDto?> GetWorkExperienceByIdAsync(int id);

    /// <summary>
    /// 根據使用者ID取得工作經歷列表
    /// </summary>
    Task<IEnumerable<WorkExperienceResponseDto>> GetWorkExperiencesByUserIdAsync(int userId);

    /// <summary>
    /// 取得公開的工作經歷（分頁）
    /// </summary>
    Task<IEnumerable<WorkExperienceResponseDto>> GetPublicWorkExperiencesAsync(int skip = 0, int take = 50);

    /// <summary>
    /// 取得目前在職的工作經歷
    /// </summary>
    Task<IEnumerable<WorkExperienceResponseDto>> GetCurrentWorkExperiencesAsync(bool publicOnly = true);

    /// <summary>
    /// 根據公司名稱搜尋工作經歷
    /// </summary>
    Task<IEnumerable<WorkExperienceResponseDto>> SearchByCompanyAsync(string company, bool publicOnly = true);

    /// <summary>
    /// 根據職位標題搜尋工作經歷
    /// </summary>
    Task<IEnumerable<WorkExperienceResponseDto>> SearchByPositionAsync(string position, bool publicOnly = true);

    /// <summary>
    /// 根據關鍵字搜尋工作經歷（公司、職位、描述）
    /// </summary>
    Task<IEnumerable<WorkExperienceResponseDto>> SearchWorkExperiencesAsync(string keyword, bool publicOnly = true);

    /// <summary>
    /// 根據日期範圍查詢工作經歷
    /// </summary>
    Task<IEnumerable<WorkExperienceResponseDto>> GetWorkExperiencesByDateRangeAsync(DateTime? startDate, DateTime? endDate, bool publicOnly = true);

    /// <summary>
    /// 建立新的工作經歷
    /// </summary>
    Task<WorkExperienceResponseDto> CreateWorkExperienceAsync(CreateWorkExperienceDto createWorkExperienceDto);

    /// <summary>
    /// 更新工作經歷
    /// </summary>
    Task<WorkExperienceResponseDto?> UpdateWorkExperienceAsync(int id, UpdateWorkExperienceDto updateWorkExperienceDto);

    /// <summary>
    /// 刪除工作經歷
    /// </summary>
    Task<bool> DeleteWorkExperienceAsync(int id);

    /// <summary>
    /// 更新工作經歷排序
    /// </summary>
    Task<bool> UpdateWorkExperienceOrderAsync(int id, int newSortOrder);

    /// <summary>
    /// 取得工作經歷統計資訊
    /// </summary>
    Task<object> GetWorkExperienceStatsAsync();

    /// <summary>
    /// 驗證工作經歷日期
    /// </summary>
    bool ValidateWorkExperienceDates(DateTime startDate, DateTime? endDate);
}