using PersonalManagerAPI.DTOs.Education;

namespace PersonalManagerAPI.Services.Interfaces;

/// <summary>
/// 學歷服務介面
/// </summary>
public interface IEducationService
{
    /// <summary>
    /// 取得所有學歷（分頁）
    /// </summary>
    Task<IEnumerable<EducationResponseDto>> GetAllEducationsAsync(int skip = 0, int take = 50);

    /// <summary>
    /// 根據ID取得學歷
    /// </summary>
    Task<EducationResponseDto?> GetEducationByIdAsync(int id);

    /// <summary>
    /// 根據使用者ID取得學歷列表
    /// </summary>
    Task<IEnumerable<EducationResponseDto>> GetEducationsByUserIdAsync(int userId);

    /// <summary>
    /// 取得公開的學歷
    /// </summary>
    Task<IEnumerable<EducationResponseDto>> GetPublicEducationsAsync(int skip = 0, int take = 50);

    /// <summary>
    /// 建立新的學歷
    /// </summary>
    Task<EducationResponseDto> CreateEducationAsync(CreateEducationDto createEducationDto);

    /// <summary>
    /// 更新學歷
    /// </summary>
    Task<EducationResponseDto?> UpdateEducationAsync(int id, UpdateEducationDto updateEducationDto);

    /// <summary>
    /// 刪除學歷
    /// </summary>
    Task<bool> DeleteEducationAsync(int id);

    /// <summary>
    /// 根據學校名稱搜尋學歷
    /// </summary>
    Task<IEnumerable<EducationResponseDto>> SearchBySchoolAsync(string schoolName, bool publicOnly = true);

    /// <summary>
    /// 根據學位搜尋學歷
    /// </summary>
    Task<IEnumerable<EducationResponseDto>> SearchByDegreeAsync(string degree, bool publicOnly = true);

    /// <summary>
    /// 取得指定時期的學歷（根據開始和結束年份）
    /// </summary>
    Task<IEnumerable<EducationResponseDto>> GetEducationsByPeriodAsync(int startYear, int? endYear = null);

    /// <summary>
    /// 取得學歷統計資料
    /// </summary>
    Task<object> GetEducationStatsAsync();

    /// <summary>
    /// 驗證學歷日期邏輯
    /// </summary>
    bool ValidateEducationDates(DateTime startDate, DateTime? endDate);
}