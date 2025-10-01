using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services.Interfaces;
using PersonalManagerAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PersonalManagerAPI.Services.Implementation;

/// <summary>
/// 設備安全服務 Entity Framework 實作 - 提供設備檢測、指紋驗證、可疑活動檢測等功能，支援 Redis 快取
/// </summary>
public class DeviceSecurityServiceEF : IDeviceSecurityService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DeviceSecurityServiceEF> _logger;
    private readonly IUserSessionService _sessionService;
    private readonly ICacheService _cacheService;

    // 快取鍵前綴
    private const string TRUSTED_DEVICES_CACHE_PREFIX = "trusted_devices:";
    private const string DEVICE_RISK_CACHE_PREFIX = "device_risk:";
    private const string USER_SESSIONS_CACHE_PREFIX = "user_sessions:";
    
    // 快取過期時間
    private static readonly TimeSpan TrustedDevicesCacheExpiry = TimeSpan.FromHours(6);
    private static readonly TimeSpan DeviceRiskCacheExpiry = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan UserSessionsCacheExpiry = TimeSpan.FromMinutes(15);

    // 已知的機器人和爬蟲 User-Agent 模式
    private readonly HashSet<string> _botUserAgents = new(StringComparer.OrdinalIgnoreCase)
    {
        "bot", "crawler", "spider", "scraper", "wget", "curl", "python", "java", "nodejs"
    };

    // 高風險國家/地區 IP 範圍 (簡化示例)
    private readonly HashSet<string> _highRiskCountries = new(StringComparer.OrdinalIgnoreCase)
    {
        "unknown", "tor", "proxy", "vpn"
    };

    public DeviceSecurityServiceEF(
        ApplicationDbContext context,
        ILogger<DeviceSecurityServiceEF> logger,
        IUserSessionService sessionService,
        ICacheService cacheService)
    {
        _context = context;
        _logger = logger;
        _sessionService = sessionService;
        _cacheService = cacheService;
    }

    /// <summary>
    /// 解析並提取設備資訊
    /// </summary>
    public DeviceInfoDto ExtractDeviceInfo(string? userAgent, string? ipAddress, Dictionary<string, string>? additionalHeaders = null)
    {
        try
        {
            var deviceInfo = new DeviceInfoDto();

            // 解析 User-Agent
            if (!string.IsNullOrEmpty(userAgent))
            {
                deviceInfo.DeviceType = DetectDeviceType(userAgent);
                deviceInfo.OperatingSystem = ExtractOperatingSystem(userAgent);
                deviceInfo.DeviceName = GenerateDeviceName(deviceInfo.DeviceType, deviceInfo.OperatingSystem);
            }
            
            // 生成設備指紋
            deviceInfo.DeviceFingerprint = GenerateDeviceFingerprint(deviceInfo, userAgent, ipAddress, additionalHeaders);

            _logger.LogInformation("設備資訊提取成功: {DeviceType}, {OS}", deviceInfo.DeviceType, deviceInfo.OperatingSystem);

            return deviceInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "提取設備資訊時發生錯誤");
            return new DeviceInfoDto();
        }
    }

    /// <summary>
    /// 生成設備指紋
    /// </summary>
    public string GenerateDeviceFingerprint(DeviceInfoDto deviceInfo, string? userAgent, string? ipAddress, Dictionary<string, string>? additionalData = null)
    {
        try
        {
            var fingerprintData = new StringBuilder();
            
            // 基本設備資訊
            fingerprintData.Append(userAgent ?? "");
            fingerprintData.Append(deviceInfo.DeviceType ?? "");
            fingerprintData.Append(deviceInfo.OperatingSystem ?? "");
            
            // 網路資訊
            fingerprintData.Append(ipAddress ?? "");
            
            // 額外標頭資訊
            if (additionalData != null)
            {
                foreach (var kvp in additionalData.OrderBy(x => x.Key))
                {
                    fingerprintData.Append($"{kvp.Key}:{kvp.Value}");
                }
            }

            // 計算 SHA256 雜湊
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(fingerprintData.ToString()));
            var fullHash = Convert.ToBase64String(hashBytes);
            
            // 返回前16字符作為短指紋
            return fullHash[..Math.Min(16, fullHash.Length)];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成設備指紋時發生錯誤");
            return Guid.NewGuid().ToString()[..16]; // 備援方案
        }
    }

    /// <summary>
    /// 檢測可疑登入活動
    /// </summary>
    public async Task<SecurityRiskAssessment> AssessLoginRiskAsync(int userId, DeviceInfoDto deviceInfo, string? ipAddress, string? location)
    {
        try
        {
            var assessment = new SecurityRiskAssessment();
            var riskFactors = new List<string>();
            int riskScore = 0;

            // 1. 檢查設備是否受信任
            var deviceFingerprint = GenerateDeviceFingerprint(deviceInfo, null, ipAddress);
            var isDeviceTrusted = await IsDeviceTrustedAsync(userId, deviceFingerprint);
            
            if (!isDeviceTrusted)
            {
                riskFactors.Add("新設備或未受信任的設備");
                riskScore += 20;
            }

            // 2. 檢查最近的會話記錄
            var recentSessions = await _sessionService.GetActiveSessionsAsync(userId);
            var lastSession = recentSessions.OrderByDescending(s => s.LastActiveAt).FirstOrDefault();

            if (lastSession != null)
            {
                // 地理位置檢查
                if (!string.IsNullOrEmpty(location) && !string.IsNullOrEmpty(lastSession.Location) &&
                    !location.Equals(lastSession.Location, StringComparison.OrdinalIgnoreCase) &&
                    !location.Contains("本地") && !lastSession.Location.Contains("本地"))
                {
                    riskFactors.Add($"地理位置變化: {lastSession.Location} -> {location}");
                    riskScore += 30;
                }

                // IP地址檢查
                if (!string.IsNullOrEmpty(ipAddress) && !string.IsNullOrEmpty(lastSession.IpAddress) &&
                    !ipAddress.Equals(lastSession.IpAddress))
                {
                    riskFactors.Add($"IP地址變化: {lastSession.IpAddress} -> {ipAddress}");
                    riskScore += 15;
                }

                // 時間模式檢查 (快速連續登入)
                var timeSinceLastLogin = DateTime.UtcNow - lastSession.LastActiveAt;
                if (timeSinceLastLogin < TimeSpan.FromMinutes(5))
                {
                    riskFactors.Add($"快速重新登入 ({timeSinceLastLogin.TotalMinutes:F1}分鐘內)");
                    riskScore += 25;
                }
            }

            // 3. 檢查並發會話數量
            var activeSessions = recentSessions.Where(s => s.IsActive).ToList();
            if (activeSessions.Count >= 5)
            {
                riskFactors.Add($"過多活躍會話 ({activeSessions.Count}個)");
                riskScore += 20;
            }

            // 4. 檢查高風險地區
            if (!string.IsNullOrEmpty(location) && _highRiskCountries.Any(country => 
                location.Contains(country, StringComparison.OrdinalIgnoreCase)))
            {
                riskFactors.Add($"高風險地區: {location}");
                riskScore += 35;
            }

            // 5. 檢查可疑的 User-Agent
            var userAgent = deviceInfo.DeviceName ?? "";
            if (_botUserAgents.Any(bot => userAgent.Contains(bot, StringComparison.OrdinalIgnoreCase)))
            {
                riskFactors.Add("可疑的用戶代理");
                riskScore += 40;
            }

            // 設定評估結果
            assessment.RiskScore = Math.Min(riskScore, 100);
            assessment.RiskFactors = riskFactors;
            
            // 判斷風險等級
            assessment.RiskLevel = assessment.RiskScore switch
            {
                <= 20 => RiskLevel.Low,
                <= 50 => RiskLevel.Medium,
                <= 80 => RiskLevel.High,
                _ => RiskLevel.Critical
            };

            // 生成建議操作
            assessment.RecommendedActions = GenerateSecurityRecommendations(assessment.RiskLevel, riskFactors);
            
            // 決定是否需要額外驗證或阻止登入
            assessment.RequiresAdditionalVerification = assessment.RiskLevel >= RiskLevel.Medium;
            assessment.ShouldBlockLogin = assessment.RiskLevel >= RiskLevel.Critical;

            // 記錄安全活動
            await LogSecurityActivityAsync(userId, "LoginRiskAssessment", 
                $"風險評估完成 - 等級: {assessment.RiskLevel}, 分數: {assessment.RiskScore}",
                deviceFingerprint, ipAddress, location, assessment.RiskLevel.ToString(), assessment.RiskScore);

            _logger.LogInformation("使用者 {UserId} 的登入風險評估完成: {RiskLevel} ({RiskScore}分)", 
                userId, assessment.RiskLevel, assessment.RiskScore);

            return assessment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "評估登入風險時發生錯誤");
            return new SecurityRiskAssessment
            {
                RiskLevel = RiskLevel.Medium,
                RiskScore = 50,
                RiskFactors = new List<string> { "風險評估系統錯誤" },
                RecommendedActions = new List<string> { "請稍後重試或聯絡系統管理員" }
            };
        }
    }

    /// <summary>
    /// 檢查設備是否受信任 (支援快取)
    /// </summary>
    public async Task<bool> IsDeviceTrustedAsync(int userId, string deviceFingerprint)
    {
        try
        {
            // 構建快取鍵
            var cacheKey = $"{TRUSTED_DEVICES_CACHE_PREFIX}{userId}:{deviceFingerprint}";
            
            // 先從快取查詢
            var cachedResult = await _cacheService.GetAsync<bool?>(cacheKey);
            if (cachedResult.HasValue)
            {
                _logger.LogDebug("從快取獲取設備信任狀態: UserId={UserId}, Fingerprint={Fingerprint}, Trusted={Trusted}", 
                    userId, deviceFingerprint, cachedResult.Value);
                return cachedResult.Value;
            }

            // 快取未命中，從資料庫查詢
            var trustedDevice = await _context.TrustedDevices
                .FirstOrDefaultAsync(d => d.UserId == userId && 
                                         d.DeviceFingerprint == deviceFingerprint &&
                                         d.IsTrusted &&
                                         d.RevokedAt == null);

            var isTrusted = trustedDevice != null;
            
            // 存入快取
            await _cacheService.SetAsync(cacheKey, isTrusted, TrustedDevicesCacheExpiry);
            
            _logger.LogDebug("從資料庫查詢設備信任狀態: UserId={UserId}, Fingerprint={Fingerprint}, Trusted={Trusted}", 
                userId, deviceFingerprint, isTrusted);

            return isTrusted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "檢查設備信任狀態時發生錯誤");
            return false;
        }
    }

    /// <summary>
    /// 將設備標記為受信任
    /// </summary>
    public async Task<bool> TrustDeviceAsync(int userId, string deviceFingerprint, DeviceInfoDto deviceInfo)
    {
        try
        {
            // 檢查是否已存在
            var existingDevice = await _context.TrustedDevices
                .FirstOrDefaultAsync(d => d.UserId == userId && d.DeviceFingerprint == deviceFingerprint);

            var now = DateTime.UtcNow;

            if (existingDevice != null)
            {
                // 更新現有設備
                existingDevice.IsTrusted = true;
                existingDevice.TrustedAt = now;
                existingDevice.RevokedAt = null;
                existingDevice.LastActiveAt = now;
                existingDevice.UpdatedAt = now;
                
                // 更新設備資訊
                if (!string.IsNullOrEmpty(deviceInfo.DeviceName))
                    existingDevice.DeviceName = deviceInfo.DeviceName;
                if (!string.IsNullOrEmpty(deviceInfo.DeviceType))
                    existingDevice.DeviceType = deviceInfo.DeviceType;
                if (!string.IsNullOrEmpty(deviceInfo.OperatingSystem))
                    existingDevice.OperatingSystem = deviceInfo.OperatingSystem;
            }
            else
            {
                // 創建新的受信任設備
                var trustedDevice = new TrustedDevice
                {
                    UserId = userId,
                    DeviceFingerprint = deviceFingerprint,
                    DeviceName = deviceInfo.DeviceName,
                    DeviceType = deviceInfo.DeviceType,
                    OperatingSystem = deviceInfo.OperatingSystem,
                    IsTrusted = true,
                    FirstUsedAt = now,
                    LastActiveAt = now,
                    TrustedAt = now,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                _context.TrustedDevices.Add(trustedDevice);
            }

            await _context.SaveChangesAsync();

            // 清除相關快取
            await InvalidateTrustedDeviceCacheAsync(userId, deviceFingerprint);

            // 記錄安全活動
            await LogSecurityActivityAsync(userId, "DeviceTrusted", 
                $"設備已標記為受信任: {deviceInfo.DeviceName} ({deviceInfo.DeviceType})",
                deviceFingerprint, null, null, "Low", 0);

            _logger.LogInformation("設備 {DeviceFingerprint} 已成功標記為使用者 {UserId} 的受信任設備", 
                deviceFingerprint, userId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "標記受信任設備時發生錯誤");
            return false;
        }
    }

    /// <summary>
    /// 撤銷設備信任
    /// </summary>
    public async Task<bool> RevokeTrustAsync(int userId, string deviceFingerprint)
    {
        try
        {
            var trustedDevice = await _context.TrustedDevices
                .FirstOrDefaultAsync(d => d.UserId == userId && d.DeviceFingerprint == deviceFingerprint);

            if (trustedDevice == null)
            {
                return false;
            }

            trustedDevice.IsTrusted = false;
            trustedDevice.RevokedAt = DateTime.UtcNow;
            trustedDevice.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // 清除相關快取
            await InvalidateTrustedDeviceCacheAsync(userId, deviceFingerprint);

            // 記錄安全活動
            await LogSecurityActivityAsync(userId, "DeviceTrustRevoked", 
                $"設備信任已撤銷: {trustedDevice.DeviceName} ({trustedDevice.DeviceType})",
                deviceFingerprint, null, null, "Medium", 20);

            _logger.LogInformation("設備 {DeviceFingerprint} 的信任狀態已撤銷", deviceFingerprint);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "撤銷設備信任時發生錯誤");
            return false;
        }
    }

    /// <summary>
    /// 獲取用戶的受信任設備列表 (支援快取)
    /// </summary>
    public async Task<List<TrustedDeviceDto>> GetTrustedDevicesAsync(int userId)
    {
        try
        {
            // 構建快取鍵
            var cacheKey = $"{TRUSTED_DEVICES_CACHE_PREFIX}list:{userId}";
            
            // 先從快取查詢
            var cachedDevices = await _cacheService.GetAsync<List<TrustedDeviceDto>>(cacheKey);
            if (cachedDevices != null && cachedDevices.Any())
            {
                _logger.LogDebug("從快取獲取受信任設備列表: UserId={UserId}, Count={Count}", userId, cachedDevices.Count);
                return cachedDevices;
            }

            // 快取未命中，從資料庫查詢
            var devices = await _context.TrustedDevices
                .Where(d => d.UserId == userId && d.IsTrusted && d.RevokedAt == null)
                .OrderByDescending(d => d.LastActiveAt)
                .Select(d => new TrustedDeviceDto
                {
                    Id = d.Id,
                    DeviceFingerprint = d.DeviceFingerprint,
                    DeviceName = d.DeviceName ?? "未知設備",
                    DeviceType = d.DeviceType ?? "Unknown",
                    OperatingSystem = d.OperatingSystem,
                    FirstSeenAt = d.FirstUsedAt,
                    LastSeenAt = d.LastActiveAt,
                    TrustedAt = d.TrustedAt ?? d.CreatedAt,
                    IsActive = d.LastActiveAt > DateTime.UtcNow.AddDays(-7), // 7天內有活動
                    LastIpAddress = d.IpAddress,
                    LastLocation = d.Location
                })
                .ToListAsync();

            // 存入快取
            if (devices.Any())
            {
                await _cacheService.SetAsync(cacheKey, devices, TrustedDevicesCacheExpiry);
            }
            
            _logger.LogDebug("從資料庫查詢受信任設備列表: UserId={UserId}, Count={Count}", userId, devices.Count);

            return devices;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "獲取受信任設備列表時發生錯誤");
            return new List<TrustedDeviceDto>();
        }
    }

    /// <summary>
    /// 檢測同時登入的異常活動
    /// </summary>
    public async Task<List<SuspiciousActivityDto>> DetectSuspiciousActivityAsync(int userId)
    {
        try
        {
            var suspiciousActivities = new List<SuspiciousActivityDto>();
            var now = DateTime.UtcNow;

            // 獲取最近24小時的安全活動日誌
            var recentActivities = await _context.SecurityActivityLogs
                .Where(log => log.UserId == userId && 
                             log.ActivityAt > now.AddHours(-24) &&
                             (log.IsSuspicious || log.RiskScore > 50))
                .OrderByDescending(log => log.ActivityAt)
                .ToListAsync();

            // 轉換為 DTO
            foreach (var activity in recentActivities)
            {
                suspiciousActivities.Add(new SuspiciousActivityDto
                {
                    Id = activity.Id,
                    ActivityType = activity.ActivityType,
                    Description = activity.Description,
                    RiskLevel = Enum.TryParse<RiskLevel>(activity.RiskLevel, out var level) ? level : RiskLevel.Medium,
                    DetectedAt = activity.ActivityAt,
                    IpAddress = activity.IpAddress,
                    Location = activity.Location,
                    DeviceInfo = activity.DeviceFingerprint,
                    IsResolved = activity.IsHandled,
                    AffectedSessions = new List<string>() // 可以根據需要擴展
                });
            }

            // 額外檢測邏輯
            var activeSessions = await _sessionService.GetActiveSessionsAsync(userId);
            
            // 檢測多重活躍會話
            if (activeSessions.Count() >= 5)
            {
                suspiciousActivities.Add(new SuspiciousActivityDto
                {
                    ActivityType = "MultipleActiveSessions",
                    Description = $"檢測到異常多的活躍會話 ({activeSessions.Count()}個)",
                    RiskLevel = RiskLevel.High,
                    DetectedAt = now,
                    IsResolved = false,
                    AffectedSessions = activeSessions.Select(s => s.SessionId).ToList()
                });
            }

            // 檢測多地理位置登入
            var distinctLocations = activeSessions
                .Where(s => !string.IsNullOrEmpty(s.Location))
                .Select(s => s.Location)
                .Distinct()
                .Count();

            if (distinctLocations >= 3)
            {
                suspiciousActivities.Add(new SuspiciousActivityDto
                {
                    ActivityType = "MultipleGeographicLocations",
                    Description = $"檢測到來自多個地理位置的登入 ({distinctLocations}個不同位置)",
                    RiskLevel = RiskLevel.High,
                    DetectedAt = now,
                    IsResolved = false,
                    AffectedSessions = activeSessions.Select(s => s.SessionId).ToList()
                });
            }

            // 檢測快速多次登入
            var recentLogins = recentActivities
                .Where(a => a.ActivityType == "Login" && a.ActivityAt > now.AddMinutes(-10))
                .Count();

            if (recentLogins >= 3)
            {
                suspiciousActivities.Add(new SuspiciousActivityDto
                {
                    ActivityType = "RapidMultipleLogins",
                    Description = $"檢測到10分鐘內多次登入嘗試 ({recentLogins}次)",
                    RiskLevel = RiskLevel.Medium,
                    DetectedAt = now,
                    IsResolved = false
                });
            }

            _logger.LogInformation("使用者 {UserId} 的可疑活動檢測完成，發現 {Count} 個可疑活動", 
                userId, suspiciousActivities.Count);

            return suspiciousActivities.OrderByDescending(a => a.DetectedAt).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "檢測可疑活動時發生錯誤");
            return new List<SuspiciousActivityDto>();
        }
    }

    /// <summary>
    /// 強制終止所有可疑會話
    /// </summary>
    public async Task<int> TerminateSuspiciousSessionsAsync(int userId, string reason = "Suspicious activity detected")
    {
        try
        {
            var terminatedCount = 0;
            var suspiciousActivities = await DetectSuspiciousActivityAsync(userId);

            // 獲取所有受影響的會話
            var affectedSessionIds = suspiciousActivities
                .SelectMany(a => a.AffectedSessions)
                .Distinct()
                .ToList();

            foreach (var sessionId in affectedSessionIds)
            {
                var terminated = await _sessionService.EndSessionAsync(sessionId, reason);
                if (terminated)
                {
                    terminatedCount++;
                }
            }

            // 記錄安全活動
            if (terminatedCount > 0)
            {
                await LogSecurityActivityAsync(userId, "SuspiciousSessionsTerminated", 
                    $"已終止 {terminatedCount} 個可疑會話",
                    null, null, null, "High", 80);
            }

            _logger.LogWarning("使用者 {UserId} 的 {Count} 個可疑會話已被終止", userId, terminatedCount);

            return terminatedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "終止可疑會話時發生錯誤");
            return 0;
        }
    }

    #region 私有輔助方法

    /// <summary>
    /// 記錄安全活動
    /// </summary>
    private async Task LogSecurityActivityAsync(int userId, string activityType, string description, 
        string? deviceFingerprint, string? ipAddress, string? location, string? riskLevel, int riskScore)
    {
        try
        {
            var log = new SecurityActivityLog
            {
                UserId = userId,
                ActivityType = activityType,
                Description = description,
                DeviceFingerprint = deviceFingerprint,
                IpAddress = ipAddress,
                Location = location,
                RiskLevel = riskLevel,
                RiskScore = riskScore,
                IsSuspicious = riskScore > 50,
                IsHandled = false,
                ActivityAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.SecurityActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "記錄安全活動時發生錯誤");
        }
    }

    /// <summary>
    /// 檢測設備類型
    /// </summary>
    private string DetectDeviceType(string? userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
            return "Unknown";
            
        var ua = userAgent.ToLowerInvariant();

        if (ua.Contains("mobile") || ua.Contains("android") || ua.Contains("iphone"))
            return "Mobile";
        if (ua.Contains("tablet") || ua.Contains("ipad"))
            return "Tablet";
        if (ua.Contains("windows") || ua.Contains("macintosh") || ua.Contains("linux"))
            return "Desktop";
        if (_botUserAgents.Any(bot => ua.Contains(bot)))
            return "API";

        return "Unknown";
    }

    /// <summary>
    /// 提取作業系統資訊
    /// </summary>
    private string ExtractOperatingSystem(string? userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
            return "Unknown";
            
        var ua = userAgent.ToLowerInvariant();

        if (ua.Contains("windows nt"))
            return ExtractWindowsVersion(ua);
        if (ua.Contains("mac os x") || ua.Contains("macos"))
            return "macOS";
        if (ua.Contains("android"))
            return ExtractAndroidVersion(ua);
        if (ua.Contains("ios") || ua.Contains("iphone os"))
            return "iOS";
        if (ua.Contains("ubuntu"))
            return "Ubuntu";
        if (ua.Contains("linux"))
            return "Linux";

        return "Unknown";
    }

    /// <summary>
    /// 提取 Windows 版本
    /// </summary>
    private string ExtractWindowsVersion(string userAgent)
    {
        var windowsVersions = new Dictionary<string, string>
        {
            { "windows nt 10.0", "Windows 10/11" },
            { "windows nt 6.3", "Windows 8.1" },
            { "windows nt 6.2", "Windows 8" },
            { "windows nt 6.1", "Windows 7" }
        };

        foreach (var version in windowsVersions)
        {
            if (userAgent.Contains(version.Key))
                return version.Value;
        }

        return "Windows";
    }

    /// <summary>
    /// 提取 Android 版本
    /// </summary>
    private string ExtractAndroidVersion(string userAgent)
    {
        var match = Regex.Match(userAgent, @"android (\d+\.?\d*)", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return $"Android {match.Groups[1].Value}";
        }
        return "Android";
    }

    /// <summary>
    /// 生成設備名稱
    /// </summary>
    private string GenerateDeviceName(string? deviceType, string? os)
    {
        var parts = new List<string>();
        
        if (!string.IsNullOrEmpty(os) && os != "Unknown")
            parts.Add(os);
        if (!string.IsNullOrEmpty(deviceType) && deviceType != "Unknown")
            parts.Add(deviceType);

        return parts.Any() ? string.Join(" ", parts) : "未知設備";
    }

    /// <summary>
    /// 生成安全建議
    /// </summary>
    private List<string> GenerateSecurityRecommendations(RiskLevel riskLevel, List<string> riskFactors)
    {
        var recommendations = new List<string>();

        switch (riskLevel)
        {
            case RiskLevel.Low:
                recommendations.Add("設備安全狀態良好，建議定期更新密碼");
                break;

            case RiskLevel.Medium:
                recommendations.Add("建議開啟雙因素認證以提升帳戶安全性");
                if (riskFactors.Any(f => f.Contains("新設備")))
                    recommendations.Add("如這是您的新設備，建議將其標記為受信任");
                break;

            case RiskLevel.High:
                recommendations.Add("檢測到異常登入模式，建議立即檢查帳戶活動");
                recommendations.Add("考慮變更密碼並檢查其他活躍會話");
                if (riskFactors.Any(f => f.Contains("地理位置")))
                    recommendations.Add("如您不在此地理位置，請立即保護帳戶");
                break;

            case RiskLevel.Critical:
                recommendations.Add("緊急：檢測到高風險活動，建議立即終止所有會話");
                recommendations.Add("立即更改密碼並開啟所有安全選項");
                recommendations.Add("檢查帳戶是否有未授權的變更");
                break;
        }

        return recommendations;
    }

    /// <summary>
    /// 清除受信任設備相關快取
    /// </summary>
    private async Task InvalidateTrustedDeviceCacheAsync(int userId, string deviceFingerprint)
    {
        try
        {
            // 清除單一設備信任狀態快取
            var deviceCacheKey = $"{TRUSTED_DEVICES_CACHE_PREFIX}{userId}:{deviceFingerprint}";
            await _cacheService.RemoveAsync(deviceCacheKey);
            
            // 清除受信任設備列表快取
            var listCacheKey = $"{TRUSTED_DEVICES_CACHE_PREFIX}list:{userId}";
            await _cacheService.RemoveAsync(listCacheKey);
            
            // 清除風險評估相關快取
            var riskCachePattern = $"{DEVICE_RISK_CACHE_PREFIX}{userId}:*";
            await _cacheService.RemoveByPatternAsync(riskCachePattern);
            
            _logger.LogDebug("已清除使用者 {UserId} 設備 {DeviceFingerprint} 的相關快取", userId, deviceFingerprint);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "清除快取時發生錯誤: UserId={UserId}, DeviceFingerprint={DeviceFingerprint}", 
                userId, deviceFingerprint);
        }
    }

    #endregion
}