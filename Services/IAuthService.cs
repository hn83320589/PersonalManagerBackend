using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Models;

namespace PersonalManagerAPI.Services
{
    /// <summary>
    /// 認證服務介面
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// 使用者登入
        /// </summary>
        Task<TokenResponseDto?> LoginAsync(LoginDto loginDto);

        /// <summary>
        /// 使用者註冊
        /// </summary>
        Task<TokenResponseDto?> RegisterAsync(RegisterDto registerDto);

        /// <summary>
        /// 重新整理令牌
        /// </summary>
        Task<TokenResponseDto?> RefreshTokenAsync(string refreshToken);

        /// <summary>
        /// 撤銷令牌
        /// </summary>
        Task<bool> RevokeTokenAsync(string refreshToken);

        /// <summary>
        /// 驗證密碼
        /// </summary>
        bool VerifyPassword(string password, string hashedPassword);

        /// <summary>
        /// 雜湊密碼
        /// </summary>
        string HashPassword(string password);

        /// <summary>
        /// 產生存取令牌
        /// </summary>
        string GenerateAccessToken(User user);

        /// <summary>
        /// 產生重新整理令牌
        /// </summary>
        string GenerateRefreshToken();

        /// <summary>
        /// 從令牌取得使用者 ID
        /// </summary>
        int? GetUserIdFromToken(string token);
    }
}