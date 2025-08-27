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
        /// 撤銷 Access Token (加入黑名單)
        /// </summary>
        Task<bool> RevokeAccessTokenAsync(string accessToken);

        /// <summary>
        /// 登出並撤銷所有使用者 Token
        /// </summary>
        Task<bool> LogoutAsync(int userId, string? accessToken = null);

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

        /// <summary>
        /// 檢查 Refresh Token 是否需要自動續期
        /// </summary>
        Task<bool> ShouldAutoRenewRefreshTokenAsync(string refreshToken);

        /// <summary>
        /// 自動續期 Refresh Token
        /// </summary>
        Task<TokenResponseDto?> AutoRenewRefreshTokenAsync(string refreshToken);

        /// <summary>
        /// 取得 Token 剩餘有效時間
        /// </summary>
        Task<TimeSpan?> GetRefreshTokenRemainingTimeAsync(string refreshToken);
    }
}