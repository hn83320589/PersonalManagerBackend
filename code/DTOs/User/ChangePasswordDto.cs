using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.DTOs.User;

public class ChangePasswordDto
{
    [Required(ErrorMessage = "舊密碼為必填")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "新密碼為必填")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "新密碼長度需在 6-100 字元")]
    public string NewPassword { get; set; } = string.Empty;

    [Compare("NewPassword", ErrorMessage = "確認新密碼與新密碼不匹配")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}