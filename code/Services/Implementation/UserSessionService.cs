using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services.Interfaces;

namespace PersonalManagerAPI.Services.Implementation;

/// <summary>
/// 用戶會話管理服務實作
/// </summary>
public class UserSessionService : IUserSessionService
{
    private readonly JsonDataService _dataService;
    private readonly ILogger<UserSessionService> _logger;

    public UserSessionService(JsonDataService dataService, ILogger<UserSessionService> logger)
    {
        _dataService = dataService;
        _logger = logger;
    }

    /// <summary>
    /// 創建新的用戶會話
    /// </summary>
    public async Task<UserSession> CreateSessionAsync(int userId, string sessionId, string refreshToken, 
        DateTime expiresAt, DeviceInfoDto? deviceInfo = null, string? userAgent = null, string? ipAddress = null)
    {
        try
        {
            // 檢查並執行設備限制
            await EnforceDeviceLimitAsync(userId);

            var session = new UserSession
            {
                Id = await GetNextIdAsync(),
                UserId = userId,
                SessionId = sessionId,
                RefreshToken = refreshToken,
                DeviceName = deviceInfo?.DeviceName ?? ExtractDeviceFromUserAgent(userAgent),
                DeviceType = deviceInfo?.DeviceType ?? DetectDeviceType(userAgent),
                OperatingSystem = deviceInfo?.OperatingSystem ?? ExtractOSFromUserAgent(userAgent),
                UserAgent = userAgent,
                IpAddress = ipAddress,
                Location = await GetLocationFromIpAsync(ipAddress),
                CreatedAt = DateTime.UtcNow,
                LastActiveAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                IsActive = true,
                IsCurrent = true
            };

            // 將其他會話設為非當前
            await SetOtherSessionsAsNonCurrentAsync(userId, session.SessionId);

            var sessions = await GetAllSessionsAsync();
            sessions.Add(session);
            await SaveSessionsAsync(sessions);

            _logger.LogInformation("新會話已創建: UserId={UserId}, SessionId={SessionId}, Device={Device}", 
                userId, sessionId, session.DeviceName);

            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "創建會話失敗: UserId={UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 更新會話活躍時間
    /// </summary>
    public async Task<bool> UpdateLastActiveAsync(string sessionId)
    {
        try
        {
            var sessions = await GetAllSessionsAsync();
            var session = sessions.FirstOrDefault(s => s.SessionId == sessionId && s.IsActive);

            if (session != null)
            {
                session.LastActiveAt = DateTime.UtcNow;
                await SaveSessionsAsync(sessions);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新會話活躍時間失敗: SessionId={SessionId}", sessionId);
            return false;
        }
    }

    /// <summary>
    /// 獲取用戶的所有會話
    /// </summary>
    public async Task<List<UserSessionDto>> GetUserSessionsAsync(int userId)
    {
        try
        {
            var sessions = await GetAllSessionsAsync();
            return sessions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.LastActiveAt)
                .Select(MapToDto)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "獲取用戶會話失敗: UserId={UserId}", userId);
            return new List<UserSessionDto>();
        }
    }

    /// <summary>
    /// 獲取用戶的活躍會話
    /// </summary>
    public async Task<List<UserSessionDto>> GetActiveSessionsAsync(int userId)
    {
        try
        {
            var sessions = await GetAllSessionsAsync();
            return sessions
                .Where(s => s.UserId == userId && s.IsActive && s.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(s => s.LastActiveAt)
                .Select(MapToDto)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "獲取活躍會話失敗: UserId={UserId}", userId);
            return new List<UserSessionDto>();
        }
    }

    /// <summary>
    /// 根據會話ID獲取會話
    /// </summary>
    public async Task<UserSession?> GetSessionByIdAsync(string sessionId)
    {
        try
        {
            var sessions = await GetAllSessionsAsync();
            return sessions.FirstOrDefault(s => s.SessionId == sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根據ID獲取會話失敗: SessionId={SessionId}", sessionId);
            return null;
        }
    }

    /// <summary>
    /// 根據Refresh Token獲取會話
    /// </summary>
    public async Task<UserSession?> GetSessionByRefreshTokenAsync(string refreshToken)
    {
        try
        {
            var sessions = await GetAllSessionsAsync();
            return sessions.FirstOrDefault(s => s.RefreshToken == refreshToken && s.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根據RefreshToken獲取會話失敗");
            return null;
        }
    }

    /// <summary>
    /// 結束會話 (登出)
    /// </summary>
    public async Task<bool> EndSessionAsync(string sessionId, string reason = "Logout")
    {
        try
        {
            var sessions = await GetAllSessionsAsync();
            var session = sessions.FirstOrDefault(s => s.SessionId == sessionId);

            if (session != null && session.IsActive)
            {
                session.IsActive = false;
                session.IsCurrent = false;
                session.EndedAt = DateTime.UtcNow;
                session.EndReason = reason;

                await SaveSessionsAsync(sessions);

                _logger.LogInformation("會話已結束: SessionId={SessionId}, Reason={Reason}", 
                    sessionId, reason);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "結束會話失敗: SessionId={SessionId}", sessionId);
            return false;
        }
    }

    /// <summary>
    /// 結束用戶的所有會話 (全部登出)
    /// </summary>
    public async Task<bool> EndAllUserSessionsAsync(int userId, string reason = "LogoutAll")
    {
        try
        {
            var sessions = await GetAllSessionsAsync();
            var userSessions = sessions.Where(s => s.UserId == userId && s.IsActive).ToList();

            foreach (var session in userSessions)
            {
                session.IsActive = false;
                session.IsCurrent = false;
                session.EndedAt = DateTime.UtcNow;
                session.EndReason = reason;
            }

            await SaveSessionsAsync(sessions);

            _logger.LogInformation("用戶所有會話已結束: UserId={UserId}, Count={Count}, Reason={Reason}", 
                userId, userSessions.Count, reason);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "結束所有會話失敗: UserId={UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// 結束用戶的其他會話 (保留當前會話)
    /// </summary>
    public async Task<bool> EndOtherSessionsAsync(int userId, string currentSessionId, string reason = "LogoutOthers")
    {
        try
        {
            var sessions = await GetAllSessionsAsync();
            var otherSessions = sessions
                .Where(s => s.UserId == userId && s.SessionId != currentSessionId && s.IsActive)
                .ToList();

            foreach (var session in otherSessions)
            {
                session.IsActive = false;
                session.IsCurrent = false;
                session.EndedAt = DateTime.UtcNow;
                session.EndReason = reason;
            }

            await SaveSessionsAsync(sessions);

            _logger.LogInformation("用戶其他會話已結束: UserId={UserId}, Count={Count}, CurrentSession={CurrentSession}", 
                userId, otherSessions.Count, currentSessionId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "結束其他會話失敗: UserId={UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// 撤銷會話 (管理員操作)
    /// </summary>
    public async Task<bool> RevokeSessionAsync(int sessionId, string reason = "Revoked")
    {
        try
        {
            var sessions = await GetAllSessionsAsync();
            var session = sessions.FirstOrDefault(s => s.Id == sessionId);

            if (session != null && session.IsActive)
            {
                session.IsActive = false;
                session.IsCurrent = false;
                session.EndedAt = DateTime.UtcNow;
                session.EndReason = reason;

                await SaveSessionsAsync(sessions);

                _logger.LogInformation("會話已撤銷: SessionId={SessionId}, Reason={Reason}", 
                    sessionId, reason);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "撤銷會話失敗: SessionId={SessionId}", sessionId);
            return false;
        }
    }

    /// <summary>
    /// 清理過期的會話
    /// </summary>
    public async Task<int> CleanupExpiredSessionsAsync()
    {
        try
        {
            var sessions = await GetAllSessionsAsync();
            var expiredSessions = sessions
                .Where(s => s.IsActive && s.ExpiresAt <= DateTime.UtcNow)
                .ToList();

            foreach (var session in expiredSessions)
            {
                session.IsActive = false;
                session.IsCurrent = false;
                session.EndedAt = DateTime.UtcNow;
                session.EndReason = "Expired";
            }

            await SaveSessionsAsync(sessions);

            _logger.LogInformation("已清理過期會話: Count={Count}", expiredSessions.Count);
            return expiredSessions.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理過期會話失敗");
            return 0;
        }
    }

    /// <summary>
    /// 獲取會話統計資訊
    /// </summary>
    public async Task<object> GetSessionStatsAsync(int userId)
    {
        try
        {
            var sessions = await GetAllSessionsAsync();
            var userSessions = sessions.Where(s => s.UserId == userId).ToList();

            var stats = new
            {
                TotalSessions = userSessions.Count,
                ActiveSessions = userSessions.Count(s => s.IsActive),
                DeviceTypes = userSessions.GroupBy(s => s.DeviceType)
                    .ToDictionary(g => g.Key, g => g.Count()),
                RecentActivity = userSessions
                    .Where(s => s.LastActiveAt >= DateTime.UtcNow.AddDays(-7))
                    .Count(),
                MostUsedDevice = userSessions
                    .GroupBy(s => s.DeviceName)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key ?? "Unknown"
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "獲取會話統計失敗: UserId={UserId}", userId);
            return new { Error = "統計資料獲取失敗" };
        }
    }

    /// <summary>
    /// 檢查並執行設備限制 (最大並發會話數)
    /// </summary>
    public async Task<bool> EnforceDeviceLimitAsync(int userId, int maxSessions = 5)
    {
        try
        {
            var sessions = await GetAllSessionsAsync();
            var activeSessions = sessions
                .Where(s => s.UserId == userId && s.IsActive && s.ExpiresAt > DateTime.UtcNow)
                .OrderBy(s => s.LastActiveAt)
                .ToList();

            if (activeSessions.Count >= maxSessions)
            {
                // 結束最舊的會話
                var sessionsToEnd = activeSessions.Take(activeSessions.Count - maxSessions + 1).ToList();
                
                foreach (var session in sessionsToEnd)
                {
                    session.IsActive = false;
                    session.IsCurrent = false;
                    session.EndedAt = DateTime.UtcNow;
                    session.EndReason = "DeviceLimitExceeded";
                }

                await SaveSessionsAsync(sessions);

                _logger.LogInformation("執行設備限制: UserId={UserId}, 結束會話數={Count}", 
                    userId, sessionsToEnd.Count);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "執行設備限制失敗: UserId={UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// 獲取設備使用歷史
    /// </summary>
    public async Task<List<object>> GetDeviceHistoryAsync(int userId, int days = 30)
    {
        try
        {
            var sessions = await GetAllSessionsAsync();
            var cutoffDate = DateTime.UtcNow.AddDays(-days);

            var history = sessions
                .Where(s => s.UserId == userId && s.CreatedAt >= cutoffDate)
                .GroupBy(s => new { s.DeviceName, s.DeviceType, s.OperatingSystem })
                .Select(g => new
                {
                    Device = g.Key.DeviceName,
                    DeviceType = g.Key.DeviceType,
                    OperatingSystem = g.Key.OperatingSystem,
                    SessionCount = g.Count(),
                    FirstSeen = g.Min(s => s.CreatedAt),
                    LastSeen = g.Max(s => s.LastActiveAt),
                    IsActive = g.Any(s => s.IsActive)
                })
                .OrderByDescending(h => h.LastSeen)
                .Cast<object>()
                .ToList();

            return history;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "獲取設備歷史失敗: UserId={UserId}", userId);
            return new List<object>();
        }
    }

    /// <summary>
    /// 檢測可疑登入
    /// </summary>
    public async Task<object> DetectSuspiciousActivityAsync(int userId, DeviceInfoDto? deviceInfo = null, string? ipAddress = null)
    {
        try
        {
            var sessions = await GetAllSessionsAsync();
            var userSessions = sessions.Where(s => s.UserId == userId).ToList();

            var isNewDevice = deviceInfo != null && 
                !userSessions.Any(s => s.DeviceName == deviceInfo.DeviceName || 
                                     s.OperatingSystem == deviceInfo.OperatingSystem);

            var isNewLocation = !string.IsNullOrEmpty(ipAddress) &&
                !userSessions.Any(s => s.IpAddress?.StartsWith(ipAddress?[..^2] ?? "") == true);

            var suspiciousScore = 0;
            var reasons = new List<string>();

            if (isNewDevice)
            {
                suspiciousScore += 3;
                reasons.Add("新設備登入");
            }

            if (isNewLocation)
            {
                suspiciousScore += 2;
                reasons.Add("新位置登入");
            }

            var recentSessions = userSessions.Where(s => s.CreatedAt >= DateTime.UtcNow.AddMinutes(-10)).Count();
            if (recentSessions > 1)
            {
                suspiciousScore += 1;
                reasons.Add("短時間內多次登入");
            }

            return new
            {
                IsSuspicious = suspiciousScore >= 3,
                SuspiciousScore = suspiciousScore,
                Reasons = reasons,
                IsNewDevice = isNewDevice,
                IsNewLocation = isNewLocation,
                RecommendedAction = suspiciousScore >= 5 ? "Block" : suspiciousScore >= 3 ? "Verify" : "Allow"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "檢測可疑活動失敗: UserId={UserId}", userId);
            return new { Error = "檢測失敗" };
        }
    }

    // 私有輔助方法

    private async Task<List<UserSession>> GetAllSessionsAsync()
    {
        try
        {
            return await _dataService.GetAllAsync<UserSession>();
        }
        catch
        {
            // 如果檔案不存在，返回空列表
            return new List<UserSession>();
        }
    }

    private async Task SaveSessionsAsync(List<UserSession> sessions)
    {
        // 直接使用 JsonDataService 的 WriteJsonAsync 方法
        await _dataService.WriteJsonAsync("userSessions.json", sessions);
    }

    private async Task<int> GetNextIdAsync()
    {
        var sessions = await GetAllSessionsAsync();
        return sessions.Count > 0 ? sessions.Max(s => s.Id) + 1 : 1;
    }

    private async Task SetOtherSessionsAsNonCurrentAsync(int userId, string currentSessionId)
    {
        var sessions = await GetAllSessionsAsync();
        var otherSessions = sessions.Where(s => s.UserId == userId && 
            s.SessionId != currentSessionId && s.IsActive).ToList();

        foreach (var session in otherSessions)
        {
            session.IsCurrent = false;
        }
    }

    private static UserSessionDto MapToDto(UserSession session)
    {
        return new UserSessionDto
        {
            Id = session.Id,
            SessionId = session.SessionId,
            DeviceName = session.DeviceName,
            DeviceType = session.DeviceType,
            OperatingSystem = session.OperatingSystem,
            IpAddress = session.IpAddress,
            Location = session.Location,
            CreatedAt = session.CreatedAt,
            LastActiveAt = session.LastActiveAt,
            IsActive = session.IsActive,
            IsCurrent = session.IsCurrent,
            EndedAt = session.EndedAt,
            EndReason = session.EndReason
        };
    }

    private static string ExtractDeviceFromUserAgent(string? userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "Unknown Device";
        
        if (userAgent.Contains("Mobile")) return "Mobile Device";
        if (userAgent.Contains("Tablet")) return "Tablet";
        if (userAgent.Contains("Windows")) return "Windows PC";
        if (userAgent.Contains("Mac")) return "Mac";
        if (userAgent.Contains("Linux")) return "Linux PC";
        
        return "Unknown Device";
    }

    private static string DetectDeviceType(string? userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "Unknown";
        
        if (userAgent.Contains("Mobile") || userAgent.Contains("Android") || userAgent.Contains("iPhone")) 
            return "Mobile";
        if (userAgent.Contains("Tablet") || userAgent.Contains("iPad")) 
            return "Tablet";
        
        return "Desktop";
    }

    private static string ExtractOSFromUserAgent(string? userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "Unknown";
        
        if (userAgent.Contains("Windows NT 10.0")) return "Windows 10/11";
        if (userAgent.Contains("Windows NT")) return "Windows";
        if (userAgent.Contains("Mac OS X")) return "macOS";
        if (userAgent.Contains("Linux")) return "Linux";
        if (userAgent.Contains("Android")) return "Android";
        if (userAgent.Contains("iPhone OS") || userAgent.Contains("iOS")) return "iOS";
        
        return "Unknown";
    }

    private static async Task<string?> GetLocationFromIpAsync(string? ipAddress)
    {
        // 簡化的位置檢測 - 實際應用中可整合第三方 IP 地理位置服務
        if (string.IsNullOrEmpty(ipAddress)) return null;
        
        // 本地 IP 檢測
        if (ipAddress.StartsWith("127.") || ipAddress.StartsWith("192.168.") || 
            ipAddress.StartsWith("10.") || ipAddress == "::1")
        {
            return "本地網路";
        }
        
        // 這裡可以整合真實的地理位置服務
        await Task.Delay(1); // 模擬異步操作
        return "未知位置";
    }
}