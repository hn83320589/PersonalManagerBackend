using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Models;

namespace PersonalManagerAPI.Services.Interfaces;

/// <summary>
/// 用戶會話管理服務介面
/// </summary>
public interface IUserSessionService
{
    /// <summary>
    /// 創建新的用戶會話
    /// </summary>
    Task<UserSession> CreateSessionAsync(int userId, string sessionId, string refreshToken, 
        DateTime expiresAt, DeviceInfoDto? deviceInfo = null, string? userAgent = null, 
        string? ipAddress = null);
    
    /// <summary>
    /// 更新會話活躍時間
    /// </summary>
    Task<bool> UpdateLastActiveAsync(string sessionId);
    
    /// <summary>
    /// 獲取用戶的所有會話
    /// </summary>
    Task<List<UserSessionDto>> GetUserSessionsAsync(int userId);
    
    /// <summary>
    /// 獲取用戶的活躍會話
    /// </summary>
    Task<List<UserSessionDto>> GetActiveSessionsAsync(int userId);
    
    /// <summary>
    /// 根據會話ID獲取會話
    /// </summary>
    Task<UserSession?> GetSessionByIdAsync(string sessionId);
    
    /// <summary>
    /// 根據Refresh Token獲取會話
    /// </summary>
    Task<UserSession?> GetSessionByRefreshTokenAsync(string refreshToken);
    
    /// <summary>
    /// 結束會話 (登出)
    /// </summary>
    Task<bool> EndSessionAsync(string sessionId, string reason = "Logout");
    
    /// <summary>
    /// 結束用戶的所有會話 (全部登出)
    /// </summary>
    Task<bool> EndAllUserSessionsAsync(int userId, string reason = "LogoutAll");
    
    /// <summary>
    /// 結束用戶的其他會話 (保留當前會話)
    /// </summary>
    Task<bool> EndOtherSessionsAsync(int userId, string currentSessionId, string reason = "LogoutOthers");
    
    /// <summary>
    /// 撤銷會話 (管理員操作)
    /// </summary>
    Task<bool> RevokeSessionAsync(int sessionId, string reason = "Revoked");
    
    /// <summary>
    /// 清理過期的會話
    /// </summary>
    Task<int> CleanupExpiredSessionsAsync();
    
    /// <summary>
    /// 獲取會話統計資訊
    /// </summary>
    Task<object> GetSessionStatsAsync(int userId);
    
    /// <summary>
    /// 檢查並執行設備限制 (最大並發會話數)
    /// </summary>
    Task<bool> EnforceDeviceLimitAsync(int userId, int maxSessions = 5);
    
    /// <summary>
    /// 獲取設備使用歷史
    /// </summary>
    Task<List<object>> GetDeviceHistoryAsync(int userId, int days = 30);
    
    /// <summary>
    /// 檢測可疑登入 (新設備、異常位置等)
    /// </summary>
    Task<object> DetectSuspiciousActivityAsync(int userId, DeviceInfoDto? deviceInfo = null, 
        string? ipAddress = null);
}