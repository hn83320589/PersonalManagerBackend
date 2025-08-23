using PersonalManagerAPI.DTOs.Skill;
using PersonalManagerAPI.Models;

namespace PersonalManagerAPI.Services.Interfaces;

/// <summary>
/// 技能服務介面
/// </summary>
public interface ISkillService
{
    /// <summary>
    /// 取得所有技能（分頁）
    /// </summary>
    Task<IEnumerable<SkillResponseDto>> GetAllSkillsAsync(int skip = 0, int take = 50);

    /// <summary>
    /// 根據ID取得技能
    /// </summary>
    Task<SkillResponseDto?> GetSkillByIdAsync(int id);

    /// <summary>
    /// 根據使用者ID取得技能列表
    /// </summary>
    Task<IEnumerable<SkillResponseDto>> GetSkillsByUserIdAsync(int userId);

    /// <summary>
    /// 取得公開的技能
    /// </summary>
    Task<IEnumerable<SkillResponseDto>> GetPublicSkillsAsync(int skip = 0, int take = 50);

    /// <summary>
    /// 建立新的技能
    /// </summary>
    Task<SkillResponseDto> CreateSkillAsync(CreateSkillDto createSkillDto);

    /// <summary>
    /// 更新技能
    /// </summary>
    Task<SkillResponseDto?> UpdateSkillAsync(int id, UpdateSkillDto updateSkillDto);

    /// <summary>
    /// 刪除技能
    /// </summary>
    Task<bool> DeleteSkillAsync(int id);

    /// <summary>
    /// 根據技能等級篩選
    /// </summary>
    Task<IEnumerable<SkillResponseDto>> GetSkillsByLevelAsync(SkillLevel level, bool publicOnly = true);

    /// <summary>
    /// 根據分類篩選技能
    /// </summary>
    Task<IEnumerable<SkillResponseDto>> GetSkillsByCategoryAsync(string category, bool publicOnly = true);

    /// <summary>
    /// 搜尋技能（根據名稱、分類、描述）
    /// </summary>
    Task<IEnumerable<SkillResponseDto>> SearchSkillsAsync(string keyword, bool publicOnly = true);

    /// <summary>
    /// 取得所有技能分類
    /// </summary>
    Task<IEnumerable<string>> GetSkillCategoriesAsync(bool publicOnly = true);

    /// <summary>
    /// 更新技能排序
    /// </summary>
    Task<bool> UpdateSkillOrderAsync(int id, int newSortOrder);

    /// <summary>
    /// 批量更新技能排序
    /// </summary>
    Task<bool> BatchUpdateSkillOrderAsync(Dictionary<int, int> skillOrders);

    /// <summary>
    /// 取得技能統計資料
    /// </summary>
    Task<object> GetSkillStatsAsync();

    /// <summary>
    /// 驗證技能分類是否有效
    /// </summary>
    bool ValidateSkillCategory(string category);
}