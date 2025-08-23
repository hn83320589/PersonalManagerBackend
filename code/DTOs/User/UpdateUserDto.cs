using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.DTOs.User;

public class UpdateUserDto
{
    [StringLength(50, MinimumLength = 3, ErrorMessage = "使用者名稱長度需在 3-50 字元")]
    public string? Username { get; set; }

    [EmailAddress(ErrorMessage = "電子郵件格式不正確")]
    [StringLength(100, ErrorMessage = "電子郵件長度不能超過 100 字元")]
    public string? Email { get; set; }

    [StringLength(50, ErrorMessage = "名字長度不能超過 50 字元")]
    public string? FirstName { get; set; }

    [StringLength(50, ErrorMessage = "姓氏長度不能超過 50 字元")]
    public string? LastName { get; set; }

    [Phone(ErrorMessage = "電話號碼格式不正確")]
    [StringLength(20, ErrorMessage = "電話號碼長度不能超過 20 字元")]
    public string? Phone { get; set; }

    public bool? IsActive { get; set; }

    [StringLength(20, ErrorMessage = "角色長度不能超過 20 字元")]
    public string? Role { get; set; }
}