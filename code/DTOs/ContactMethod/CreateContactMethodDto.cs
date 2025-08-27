using System.ComponentModel.DataAnnotations;
using PersonalManagerAPI.Models;

namespace PersonalManagerAPI.DTOs.ContactMethod;

/// <summary>
/// 建立聯絡方式 DTO
/// </summary>
public class CreateContactMethodDto
{
    /// <summary>
    /// 使用者 ID
    /// </summary>
    [Required(ErrorMessage = "使用者 ID 為必填項目")]
    public int UserId { get; set; }

    /// <summary>
    /// 聯絡方式類型
    /// </summary>
    [Required(ErrorMessage = "聯絡方式類型為必填項目")]
    public ContactType Type { get; set; }

    /// <summary>
    /// 圖示名稱或 CSS 類別
    /// </summary>
    [StringLength(50, ErrorMessage = "圖示長度不能超過 50 個字元")]
    public string? Icon { get; set; }

    /// <summary>
    /// 聯絡方式值
    /// </summary>
    [Required(ErrorMessage = "聯絡方式值為必填項目")]
    [StringLength(255, ErrorMessage = "聯絡方式值長度不能超過 255 個字元")]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 顯示標籤
    /// </summary>
    [StringLength(100, ErrorMessage = "顯示標籤長度不能超過 100 個字元")]
    public string? Label { get; set; }

    /// <summary>
    /// 是否公開顯示
    /// </summary>
    public bool IsPublic { get; set; } = true;

    /// <summary>
    /// 是否為偏好聯絡方式
    /// </summary>
    public bool IsPreferred { get; set; } = false;

    /// <summary>
    /// 排序順序
    /// </summary>
    public int SortOrder { get; set; } = 0;
}