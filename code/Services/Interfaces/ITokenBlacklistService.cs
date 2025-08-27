namespace PersonalManagerAPI.Services.Interfaces;

/// <summary>
/// Token 黑名單服務介面
/// </summary>
public interface ITokenBlacklistService
{
    /// <summary>
    /// 將 Access Token 加入黑名單
    /// </summary>
    Task AddToBlacklistAsync(string jti, DateTime expiryTime);
    
    /// <summary>
    /// 檢查 Token 是否在黑名單中
    /// </summary>
    Task<bool> IsBlacklistedAsync(string jti);
    
    /// <summary>
    /// 清理過期的黑名單 Token
    /// </summary>
    Task CleanupExpiredTokensAsync();
    
    /// <summary>
    /// 撤銷使用者的所有 Token
    /// </summary>
    Task RevokeAllUserTokensAsync(int userId);
}