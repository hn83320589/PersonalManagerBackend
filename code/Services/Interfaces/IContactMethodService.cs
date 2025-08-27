using PersonalManagerAPI.DTOs.ContactMethod;
using PersonalManagerAPI.Models;

namespace PersonalManagerAPI.Services.Interfaces;

/// <summary>
/// 聯絡方式服務介面
/// </summary>
public interface IContactMethodService
{
    /// <summary>
    /// 取得所有聯絡方式
    /// </summary>
    Task<IEnumerable<ContactMethodResponseDto>> GetAllContactMethodsAsync(int skip = 0, int take = 50);

    /// <summary>
    /// 根據 ID 取得聯絡方式
    /// </summary>
    Task<ContactMethodResponseDto?> GetContactMethodByIdAsync(int id);

    /// <summary>
    /// 取得指定使用者的聯絡方式
    /// </summary>
    Task<IEnumerable<ContactMethodResponseDto>> GetContactMethodsByUserIdAsync(int userId);

    /// <summary>
    /// 取得公開聯絡方式
    /// </summary>
    Task<IEnumerable<ContactMethodResponseDto>> GetPublicContactMethodsAsync(int? userId = null);

    /// <summary>
    /// 取得偏好聯絡方式
    /// </summary>
    Task<IEnumerable<ContactMethodResponseDto>> GetPreferredContactMethodsAsync(int? userId = null);

    /// <summary>
    /// 根據類型取得聯絡方式
    /// </summary>
    Task<IEnumerable<ContactMethodResponseDto>> GetContactMethodsByTypeAsync(ContactType type, int? userId = null);

    /// <summary>
    /// 建立新聯絡方式
    /// </summary>
    Task<ContactMethodResponseDto> CreateContactMethodAsync(CreateContactMethodDto createContactMethodDto);

    /// <summary>
    /// 更新聯絡方式
    /// </summary>
    Task<ContactMethodResponseDto?> UpdateContactMethodAsync(int id, UpdateContactMethodDto updateContactMethodDto);

    /// <summary>
    /// 刪除聯絡方式
    /// </summary>
    Task<bool> DeleteContactMethodAsync(int id);

    /// <summary>
    /// 設定偏好聯絡方式
    /// </summary>
    Task<bool> SetPreferredContactMethodAsync(int id, int userId);

    /// <summary>
    /// 更新聯絡方式排序
    /// </summary>
    Task<bool> UpdateContactMethodOrderAsync(int id, int newSortOrder);

    /// <summary>
    /// 批量更新聯絡方式排序
    /// </summary>
    Task<bool> BatchUpdateContactMethodOrderAsync(Dictionary<int, int> contactMethodOrders);

    /// <summary>
    /// 切換聯絡方式公開狀態
    /// </summary>
    Task<bool> ToggleContactMethodVisibilityAsync(int id);

    /// <summary>
    /// 驗證聯絡方式值的格式
    /// </summary>
    Task<bool> ValidateContactMethodValueAsync(ContactType type, string value);

    /// <summary>
    /// 根據聯絡方式值搜尋
    /// </summary>
    Task<IEnumerable<ContactMethodResponseDto>> SearchContactMethodsAsync(string searchTerm);

    /// <summary>
    /// 取得聯絡方式類型統計
    /// </summary>
    Task<Dictionary<ContactType, int>> GetContactMethodTypeStatsAsync(int? userId = null);

    /// <summary>
    /// 檢查聯絡方式是否存在
    /// </summary>
    Task<bool> ContactMethodExistsAsync(int id);

    /// <summary>
    /// 檢查使用者是否有特定類型的聯絡方式
    /// </summary>
    Task<bool> UserHasContactMethodTypeAsync(int userId, ContactType type);

    /// <summary>
    /// 取得聯絡方式統計資訊
    /// </summary>
    Task<object> GetContactMethodStatsAsync();
}