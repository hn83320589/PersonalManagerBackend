using PersonalManagerAPI.DTOs.PersonalProfile;

namespace PersonalManagerAPI.Services.Interfaces;

/// <summary>
/// 個人資料服務介面
/// </summary>
public interface IPersonalProfileService
{
    /// <summary>
    /// 取得所有個人資料（分頁）
    /// </summary>
    Task<IEnumerable<PersonalProfileResponseDto>> GetAllProfilesAsync(int skip = 0, int take = 50);

    /// <summary>
    /// 根據ID取得個人資料
    /// </summary>
    Task<PersonalProfileResponseDto?> GetProfileByIdAsync(int id);

    /// <summary>
    /// 根據使用者ID取得個人資料
    /// </summary>
    Task<PersonalProfileResponseDto?> GetProfileByUserIdAsync(int userId);

    /// <summary>
    /// 取得公開的個人資料
    /// </summary>
    Task<IEnumerable<PersonalProfileResponseDto>> GetPublicProfilesAsync(int skip = 0, int take = 50);

    /// <summary>
    /// 建立新的個人資料
    /// </summary>
    Task<PersonalProfileResponseDto> CreateProfileAsync(CreatePersonalProfileDto createProfileDto);

    /// <summary>
    /// 更新個人資料
    /// </summary>
    Task<PersonalProfileResponseDto?> UpdateProfileAsync(int id, UpdatePersonalProfileDto updateProfileDto);

    /// <summary>
    /// 刪除個人資料
    /// </summary>
    Task<bool> DeleteProfileAsync(int id);

    /// <summary>
    /// 切換個人資料公開狀態
    /// </summary>
    Task<bool> TogglePublicStatusAsync(int id);

    /// <summary>
    /// 檢查使用者是否已有個人資料
    /// </summary>
    Task<bool> HasUserProfileAsync(int userId);

    /// <summary>
    /// 搜尋個人資料（根據名稱、標題、自我介紹）
    /// </summary>
    Task<IEnumerable<PersonalProfileResponseDto>> SearchProfilesAsync(string keyword, bool publicOnly = true);

    /// <summary>
    /// 取得個人資料統計資料
    /// </summary>
    Task<object> GetProfileStatsAsync();

    /// <summary>
    /// 驗證社群媒體連結格式
    /// </summary>
    bool ValidateSocialMediaUrls(CreatePersonalProfileDto profileDto);
}