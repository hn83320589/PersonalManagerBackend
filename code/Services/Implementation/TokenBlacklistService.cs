using PersonalManagerAPI.Services.Interfaces;
using System.Collections.Concurrent;

namespace PersonalManagerAPI.Services.Implementation;

/// <summary>
/// Token 黑名單服務實作 (記憶體快取版本)
/// 生產環境建議使用 Redis 或資料庫
/// </summary>
public class TokenBlacklistService : ITokenBlacklistService
{
    private readonly ConcurrentDictionary<string, DateTime> _blacklistedTokens;
    private readonly ILogger<TokenBlacklistService> _logger;
    private readonly Timer _cleanupTimer;

    public TokenBlacklistService(ILogger<TokenBlacklistService> logger)
    {
        _blacklistedTokens = new ConcurrentDictionary<string, DateTime>();
        _logger = logger;
        
        // 每小時清理一次過期 Token
        _cleanupTimer = new Timer(async _ => await CleanupExpiredTokensAsync(), 
            null, TimeSpan.FromHours(1), TimeSpan.FromHours(1));
    }

    /// <summary>
    /// 將 Access Token 加入黑名單
    /// </summary>
    public Task AddToBlacklistAsync(string jti, DateTime expiryTime)
    {
        try
        {
            _blacklistedTokens.TryAdd(jti, expiryTime);
            _logger.LogInformation("Token 已加入黑名單: {Jti}, 過期時間: {ExpiryTime}", jti, expiryTime);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "將 Token 加入黑名單失敗: {Jti}", jti);
            throw;
        }
    }

    /// <summary>
    /// 檢查 Token 是否在黑名單中
    /// </summary>
    public Task<bool> IsBlacklistedAsync(string jti)
    {
        try
        {
            var isBlacklisted = _blacklistedTokens.ContainsKey(jti);
            if (isBlacklisted)
            {
                _logger.LogWarning("檢測到黑名單 Token 使用: {Jti}", jti);
            }
            return Task.FromResult(isBlacklisted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "檢查 Token 黑名單狀態失敗: {Jti}", jti);
            throw;
        }
    }

    /// <summary>
    /// 清理過期的黑名單 Token
    /// </summary>
    public Task CleanupExpiredTokensAsync()
    {
        try
        {
            var now = DateTime.UtcNow;
            var expiredTokens = _blacklistedTokens
                .Where(kvp => kvp.Value <= now)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var jti in expiredTokens)
            {
                _blacklistedTokens.TryRemove(jti, out _);
            }

            if (expiredTokens.Count > 0)
            {
                _logger.LogInformation("清理了 {Count} 個過期的黑名單 Token", expiredTokens.Count);
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理過期 Token 黑名單失敗");
            throw;
        }
    }

    /// <summary>
    /// 撤銷使用者的所有 Token (需要擴展以支援使用者 ID 追蹤)
    /// </summary>
    /// <remarks>
    /// 注意：此功能的完整實作需要持久化儲存（Redis 或資料庫）來追蹤 userId 與 jti 的對應關係。
    /// 目前的記憶體快取版本無法有效實現跨會話的使用者級別撤銷。
    /// 建議在整合 Redis 或資料庫時一併實作此功能，屆時需要：
    /// 1. 在 AddToBlacklistAsync 中記錄 userId 與 jti 的關聯
    /// 2. 在 RevokeAllUserTokensAsync 中查詢該 userId 的所有 Token 並全部撤銷
    /// 3. 考慮實作 Token 撤銷事件通知機制（發布/訂閱模式）
    /// </remarks>
    public Task RevokeAllUserTokensAsync(int userId)
    {
        _logger.LogInformation("嘗試撤銷使用者所有 Token: {UserId} (功能待實作 - 需要 Redis/資料庫支援)", userId);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }
}