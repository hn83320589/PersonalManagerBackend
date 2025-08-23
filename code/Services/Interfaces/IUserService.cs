using PersonalManagerAPI.DTOs.User;

namespace PersonalManagerAPI.Services.Interfaces;

/// <summary>
/// 使用者服務介面
/// </summary>
public interface IUserService
{
    /// <summary>
    /// 取得所有使用者（分頁）
    /// </summary>
    Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(int skip = 0, int take = 50);

    /// <summary>
    /// 根據ID取得使用者
    /// </summary>
    Task<UserResponseDto?> GetUserByIdAsync(int id);

    /// <summary>
    /// 根據使用者名稱取得使用者
    /// </summary>
    Task<UserResponseDto?> GetUserByUsernameAsync(string username);

    /// <summary>
    /// 根據電子郵件取得使用者
    /// </summary>
    Task<UserResponseDto?> GetUserByEmailAsync(string email);

    /// <summary>
    /// 建立新使用者
    /// </summary>
    Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto);

    /// <summary>
    /// 更新使用者資訊
    /// </summary>
    Task<UserResponseDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto);

    /// <summary>
    /// 刪除使用者
    /// </summary>
    Task<bool> DeleteUserAsync(int id);

    /// <summary>
    /// 變更使用者密碼
    /// </summary>
    Task<bool> ChangePasswordAsync(int id, ChangePasswordDto changePasswordDto);

    /// <summary>
    /// 驗證使用者密碼
    /// </summary>
    Task<bool> ValidatePasswordAsync(int id, string password);

    /// <summary>
    /// 檢查使用者名稱是否已存在
    /// </summary>
    Task<bool> IsUsernameExistAsync(string username, int? excludeId = null);

    /// <summary>
    /// 檢查電子郵件是否已存在
    /// </summary>
    Task<bool> IsEmailExistAsync(string email, int? excludeId = null);

    /// <summary>
    /// 更新最後登入時間
    /// </summary>
    Task<bool> UpdateLastLoginAsync(int id);

    /// <summary>
    /// 取得使用者統計資料
    /// </summary>
    Task<object> GetUserStatsAsync();
}