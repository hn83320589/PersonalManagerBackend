using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.DTOs.User;

public class CreateUserDto
{
    [Required(ErrorMessage = "使用者名稱為必填")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "使用者名稱長度需在 3-50 字元")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "電子郵件為必填")]
    [EmailAddress(ErrorMessage = "電子郵件格式不正確")]
    [StringLength(100, ErrorMessage = "電子郵件長度不能超過 100 字元")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "密碼為必填")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度需在 6-100 字元")]
    public string Password { get; set; } = string.Empty;

    [Compare("Password", ErrorMessage = "確認密碼與密碼不匹配")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "名字長度不能超過 50 字元")]
    public string? FirstName { get; set; }

    [StringLength(50, ErrorMessage = "姓氏長度不能超過 50 字元")]
    public string? LastName { get; set; }

    [Phone(ErrorMessage = "電話號碼格式不正確")]
    [StringLength(20, ErrorMessage = "電話號碼長度不能超過 20 字元")]
    public string? Phone { get; set; }

    public bool IsActive { get; set; } = true;

    [StringLength(20, ErrorMessage = "角色長度不能超過 20 字元")]
    public string Role { get; set; } = "User";
}