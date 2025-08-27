using PersonalManagerAPI.Models;
using PersonalManagerAPI.DTOs.User;

namespace PersonalManagerAPI.DTOs.ContactMethod;

/// <summary>
/// 聯絡方式回應 DTO
/// </summary>
public class ContactMethodResponseDto
{
    /// <summary>
    /// 聯絡方式 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 使用者 ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 聯絡方式類型
    /// </summary>
    public ContactType Type { get; set; }

    /// <summary>
    /// 聯絡方式類型名稱
    /// </summary>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// 圖示名稱或 CSS 類別
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 聯絡方式值
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 顯示標籤
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// 是否公開顯示
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// 是否為偏好聯絡方式
    /// </summary>
    public bool IsPreferred { get; set; }

    /// <summary>
    /// 排序順序
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 使用者資訊
    /// </summary>
    public UserResponseDto? User { get; set; }
}