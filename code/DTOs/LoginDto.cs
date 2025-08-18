using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.DTOs
{
    /// <summary>
    /// 登入請求 DTO
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// 使用者名稱
        /// </summary>
        [Required(ErrorMessage = "使用者名稱為必填")]
        [StringLength(50, ErrorMessage = "使用者名稱最多50字元")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// 密碼
        /// </summary>
        [Required(ErrorMessage = "密碼為必填")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度須介於6-100字元")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// 註冊請求 DTO
    /// </summary>
    public class RegisterDto
    {
        /// <summary>
        /// 使用者名稱
        /// </summary>
        [Required(ErrorMessage = "使用者名稱為必填")]
        [StringLength(50, ErrorMessage = "使用者名稱最多50字元")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Email
        /// </summary>
        [Required(ErrorMessage = "Email為必填")]
        [EmailAddress(ErrorMessage = "Email格式不正確")]
        [StringLength(100, ErrorMessage = "Email最多100字元")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 密碼
        /// </summary>
        [Required(ErrorMessage = "密碼為必填")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度須介於6-100字元")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 確認密碼
        /// </summary>
        [Required(ErrorMessage = "確認密碼為必填")]
        [Compare("Password", ErrorMessage = "確認密碼與密碼不符")]
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>
        /// 全名
        /// </summary>
        [StringLength(100, ErrorMessage = "全名最多100字元")]
        public string? FullName { get; set; }
    }

    /// <summary>
    /// JWT Token 回應 DTO
    /// </summary>
    public class TokenResponseDto
    {
        /// <summary>
        /// 存取令牌
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// 重新整理令牌
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// 令牌類型
        /// </summary>
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// 令牌過期時間 (Unix 時間戳)
        /// </summary>
        public long ExpiresIn { get; set; }

        /// <summary>
        /// 使用者資訊
        /// </summary>
        public UserInfoDto User { get; set; } = new();
    }

    /// <summary>
    /// 使用者資訊 DTO
    /// </summary>
    public class UserInfoDto
    {
        /// <summary>
        /// 使用者 ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 使用者名稱
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 全名
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// 角色
        /// </summary>
        public string Role { get; set; } = "User";

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// 重新整理令牌請求 DTO
    /// </summary>
    public class RefreshTokenDto
    {
        /// <summary>
        /// 重新整理令牌
        /// </summary>
        [Required(ErrorMessage = "重新整理令牌為必填")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}