using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PersonalManagerAPI.Services.Implementation;

/// <summary>
/// 設備安全服務實作 - 提供設備檢測、指紋驗證、可疑活動檢測等功能
/// </summary>
public class DeviceSecurityService : IDeviceSecurityService
{
    private readonly ILogger<DeviceSecurityService> _logger;
    private readonly JsonDataService _jsonDataService;
    private readonly IUserSessionService _sessionService;

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

    public DeviceSecurityService(
        ILogger<DeviceSecurityService> logger,
        JsonDataService jsonDataService,
        IUserSessionService sessionService)
    {
        _logger = logger;
        _jsonDataService = jsonDataService;
        _sessionService = sessionService;
    }

    /// <summary>
    /// 解析並提取設備資訊
    /// </summary>
    public DeviceInfoDto ExtractDeviceInfo(string? userAgent, string? ipAddress, Dictionary<string, string>? additionalHeaders = null)
    {
        try
        {
            var deviceInfo = new DeviceInfoDto();

            if (!string.IsNullOrEmpty(userAgent))
            {
                // 解析設備類型
                deviceInfo.DeviceType = DetectDeviceType(userAgent);
                
                // 解析作業系統
                deviceInfo.OperatingSystem = DetectOperatingSystem(userAgent);
                
                // 生成設備名稱
                deviceInfo.DeviceName = GenerateDeviceName(deviceInfo.DeviceType, deviceInfo.OperatingSystem);
            }
            else
            {
                deviceInfo.DeviceType = "Unknown";
                deviceInfo.OperatingSystem = "Unknown";
                deviceInfo.DeviceName = "Unknown Device";
            }

            // 生成設備指紋
            deviceInfo.DeviceFingerprint = GenerateDeviceFingerprint(deviceInfo, userAgent, ipAddress, additionalHeaders);

            return deviceInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "提取設備資訊時發生錯誤");
            return new DeviceInfoDto
            {
                DeviceType = "Unknown",
                OperatingSystem = "Unknown",
                DeviceName = "Unknown Device"
            };
        }
    }

    /// <summary>
    /// 生成設備指紋
    /// </summary>
    public string GenerateDeviceFingerprint(DeviceInfoDto deviceInfo, string? userAgent, string? ipAddress, Dictionary<string, string>? additionalData = null)
    {
        try
        {
            var fingerprintData = new
            {
                DeviceType = deviceInfo.DeviceType ?? "unknown",
                OperatingSystem = deviceInfo.OperatingSystem ?? "unknown",
                UserAgent = userAgent ?? "unknown",
                IpAddressHash = !string.IsNullOrEmpty(ipAddress) ? ComputeHash(ipAddress) : "unknown",
                AcceptLanguage = additionalData?.GetValueOrDefault("Accept-Language", "unknown"),
                AcceptEncoding = additionalData?.GetValueOrDefault("Accept-Encoding", "unknown"),
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd")
            };

            var fingerprintJson = JsonSerializer.Serialize(fingerprintData);
            return ComputeHash(fingerprintJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成設備指紋時發生錯誤");
            return ComputeHash($"{deviceInfo.DeviceType}-{deviceInfo.OperatingSystem}-{DateTime.UtcNow:yyyy-MM-dd}");
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
            var recommendedActions = new List<string>();
            int riskScore = 0;

            // 1. 檢查設備信任狀態
            var isDeviceTrusted = await IsDeviceTrustedAsync(userId, deviceInfo.DeviceFingerprint ?? "");
            if (!isDeviceTrusted)
            {
                riskFactors.Add("新設備或未受信任的設備");
                riskScore += 20;
                recommendedActions.Add("要求額外驗證 (2FA/Email)");
            }

            // 2. 檢查近期登入歷史
            var recentSessions = await _sessionService.GetActiveSessionsAsync(userId);
            if (recentSessions.Any())
            {
                var lastSession = recentSessions.OrderByDescending(s => s.LastActiveAt).First();
                
                // 檢查地理位置變化
                if (!string.IsNullOrEmpty(location) && !string.IsNullOrEmpty(lastSession.Location) 
                    && location != lastSession.Location && !location.Contains("本地"))
                {
                    riskFactors.Add($"地理位置變化: {lastSession.Location} -> {location}");
                    riskScore += 30;
                    recommendedActions.Add("驗證登入位置");
                }

                // 檢查時間間隔異常
                var timeDiff = DateTime.UtcNow - lastSession.LastActiveAt;
                if (timeDiff.TotalMinutes < 5 && lastSession.IpAddress != ipAddress)
                {
                    riskFactors.Add("短時間內從不同位置登入");
                    riskScore += 40;
                    recommendedActions.Add("立即通知用戶");
                }
            }

            // 3. 檢查IP風險
            if (!string.IsNullOrEmpty(ipAddress))
            {
                if (ipAddress.StartsWith("10.") || ipAddress.StartsWith("192.168.") || ipAddress == "127.0.0.1" || ipAddress == "::1")
                {
                    // 內網IP，風險較低
                }
                else if (_highRiskCountries.Any(country => location?.Contains(country, StringComparison.OrdinalIgnoreCase) == true))
                {
                    riskFactors.Add("來自高風險地區的IP");
                    riskScore += 25;
                    recommendedActions.Add("加強驗證流程");
                }
            }

            // 4. 檢查設備類型異常
            if (deviceInfo.DeviceType == "Unknown" || string.IsNullOrEmpty(deviceInfo.OperatingSystem))
            {
                riskFactors.Add("無法識別的設備類型");
                riskScore += 15;
                recommendedActions.Add("記錄詳細設備資訊");
            }

            // 5. 檢查同時活躍會話數量
            var activeSessions = recentSessions.Count(s => s.IsActive);
            if (activeSessions >= 5)
            {
                riskFactors.Add($"同時存在 {activeSessions} 個活躍會話");
                riskScore += 20;
                recommendedActions.Add("檢查是否有未授權存取");
            }

            // 設定風險等級
            assessment.RiskLevel = riskScore switch
            {
                >= 80 => RiskLevel.Critical,
                >= 60 => RiskLevel.High,
                >= 30 => RiskLevel.Medium,
                _ => RiskLevel.Low
            };

            assessment.RiskScore = riskScore;
            assessment.RiskFactors = riskFactors;
            assessment.RecommendedActions = recommendedActions;
            assessment.RequiresAdditionalVerification = riskScore >= 30;
            assessment.ShouldBlockLogin = riskScore >= 80;

            _logger.LogInformation("用戶 {UserId} 登入風險評估完成: 風險等級 {RiskLevel}, 分數 {RiskScore}", 
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
                RiskFactors = new List<string> { "風險評估失敗，採用預設安全等級" },
                RequiresAdditionalVerification = true
            };
        }
    }

    /// <summary>
    /// 檢查設備是否受信任
    /// </summary>
    public async Task<bool> IsDeviceTrustedAsync(int userId, string deviceFingerprint)
    {
        try
        {
            if (string.IsNullOrEmpty(deviceFingerprint))
                return false;

            var trustedDevices = await GetTrustedDevicesAsync(userId);
            return trustedDevices.Any(d => d.DeviceFingerprint == deviceFingerprint && d.IsActive);
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
            var trustedDevices = await GetTrustedDevicesAsync(userId);
            
            // 檢查是否已存在
            var existingDevice = trustedDevices.FirstOrDefault(d => d.DeviceFingerprint == deviceFingerprint);
            if (existingDevice != null)
            {
                existingDevice.IsActive = true;
                existingDevice.LastSeenAt = DateTime.UtcNow;
                existingDevice.TrustedAt = DateTime.UtcNow;
            }
            else
            {
                var newTrustedDevice = new TrustedDeviceDto
                {
                    Id = trustedDevices.Count > 0 ? trustedDevices.Max(d => d.Id) + 1 : 1,
                    DeviceFingerprint = deviceFingerprint,
                    DeviceName = deviceInfo.DeviceName ?? "Unknown Device",
                    DeviceType = deviceInfo.DeviceType ?? "Unknown",
                    OperatingSystem = deviceInfo.OperatingSystem,
                    FirstSeenAt = DateTime.UtcNow,
                    LastSeenAt = DateTime.UtcNow,
                    TrustedAt = DateTime.UtcNow,
                    IsActive = true
                };
                trustedDevices.Add(newTrustedDevice);
            }

            await SaveTrustedDevicesAsync(userId, trustedDevices);
            _logger.LogInformation("用戶 {UserId} 的設備 {DeviceFingerprint} 已標記為受信任", userId, deviceFingerprint);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "標記設備為受信任時發生錯誤");
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
            var trustedDevices = await GetTrustedDevicesAsync(userId);
            var device = trustedDevices.FirstOrDefault(d => d.DeviceFingerprint == deviceFingerprint);
            
            if (device != null)
            {
                device.IsActive = false;
                await SaveTrustedDevicesAsync(userId, trustedDevices);
                _logger.LogInformation("用戶 {UserId} 的設備 {DeviceFingerprint} 信任已撤銷", userId, deviceFingerprint);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "撤銷設備信任時發生錯誤");
            return false;
        }
    }

    /// <summary>
    /// 獲取用戶的受信任設備列表
    /// </summary>
    public async Task<List<TrustedDeviceDto>> GetTrustedDevicesAsync(int userId)
    {
        try
        {
            var fileName = $"trustedDevices_{userId}.json";
            var devices = await _jsonDataService.ReadJsonAsync<TrustedDeviceDto>(fileName);
            return devices ?? new List<TrustedDeviceDto>();
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
            var activeSessions = await _sessionService.GetActiveSessionsAsync(userId);

            // 檢測異常多重登入
            if (activeSessions.Count >= 5)
            {
                suspiciousActivities.Add(new SuspiciousActivityDto
                {
                    Id = suspiciousActivities.Count + 1,
                    ActivityType = "MultipleActiveSessions",
                    Description = $"檢測到 {activeSessions.Count} 個同時活躍的會話",
                    RiskLevel = RiskLevel.High,
                    DetectedAt = DateTime.UtcNow,
                    AffectedSessions = activeSessions.Select(s => s.SessionId).ToList()
                });
            }

            // 檢測異常地理位置
            var uniqueLocations = activeSessions.Where(s => !string.IsNullOrEmpty(s.Location))
                                              .Select(s => s.Location)
                                              .Distinct()
                                              .Count();
            
            if (uniqueLocations >= 3)
            {
                suspiciousActivities.Add(new SuspiciousActivityDto
                {
                    Id = suspiciousActivities.Count + 1,
                    ActivityType = "MultipleGeographicLocations",
                    Description = $"檢測到來自 {uniqueLocations} 個不同地理位置的同時登入",
                    RiskLevel = RiskLevel.Medium,
                    DetectedAt = DateTime.UtcNow,
                    AffectedSessions = activeSessions.Select(s => s.SessionId).ToList()
                });
            }

            // 檢測短時間內的多次登入
            var recentSessions = activeSessions.Where(s => s.CreatedAt > DateTime.UtcNow.AddMinutes(-10)).ToList();
            if (recentSessions.Count >= 3)
            {
                suspiciousActivities.Add(new SuspiciousActivityDto
                {
                    Id = suspiciousActivities.Count + 1,
                    ActivityType = "RapidMultipleLogins",
                    Description = $"10分鐘內檢測到 {recentSessions.Count} 次登入",
                    RiskLevel = RiskLevel.High,
                    DetectedAt = DateTime.UtcNow,
                    AffectedSessions = recentSessions.Select(s => s.SessionId).ToList()
                });
            }

            return suspiciousActivities;
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
            var suspiciousActivities = await DetectSuspiciousActivityAsync(userId);
            var sessionsToTerminate = new HashSet<string>();

            foreach (var activity in suspiciousActivities.Where(a => a.RiskLevel >= RiskLevel.High))
            {
                foreach (var sessionId in activity.AffectedSessions)
                {
                    sessionsToTerminate.Add(sessionId);
                }
            }

            int terminatedCount = 0;
            foreach (var sessionId in sessionsToTerminate)
            {
                var terminated = await _sessionService.EndSessionAsync(sessionId, reason);
                if (terminated)
                {
                    terminatedCount++;
                }
            }

            if (terminatedCount > 0)
            {
                _logger.LogWarning("用戶 {UserId} 的 {Count} 個可疑會話已被終止", userId, terminatedCount);
            }

            return terminatedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "終止可疑會話時發生錯誤");
            return 0;
        }
    }

    #region Private Helper Methods

    private string DetectDeviceType(string userAgent)
    {
        userAgent = userAgent.ToLowerInvariant();

        if (userAgent.Contains("mobile") || userAgent.Contains("android") || userAgent.Contains("iphone"))
            return "Mobile";
        
        if (userAgent.Contains("tablet") || userAgent.Contains("ipad"))
            return "Tablet";
        
        if (userAgent.Contains("curl") || userAgent.Contains("wget") || userAgent.Contains("python"))
            return "API/Script";

        return "Desktop";
    }

    private string DetectOperatingSystem(string userAgent)
    {
        userAgent = userAgent.ToLowerInvariant();

        if (userAgent.Contains("windows nt 10.0"))
            return "Windows 10/11";
        else if (userAgent.Contains("windows nt"))
            return "Windows";
        else if (userAgent.Contains("mac os x") || userAgent.Contains("macos"))
            return "macOS";
        else if (userAgent.Contains("linux"))
            return "Linux";
        else if (userAgent.Contains("android"))
            return Regex.Match(userAgent, @"android (\d+\.?\d*)").Groups[1].Value switch
            {
                var v when !string.IsNullOrEmpty(v) => $"Android {v}",
                _ => "Android"
            };
        else if (userAgent.Contains("iphone os") || userAgent.Contains("ios"))
            return "iOS";

        return "Unknown";
    }

    private string GenerateDeviceName(string deviceType, string operatingSystem)
    {
        return $"{deviceType} ({operatingSystem})";
    }

    private string ComputeHash(string input)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hashBytes)[..16]; // 取前16個字符作為簡短指紋
    }

    private async Task SaveTrustedDevicesAsync(int userId, List<TrustedDeviceDto> devices)
    {
        var fileName = $"trustedDevices_{userId}.json";
        await _jsonDataService.WriteJsonAsync(fileName, devices);
    }

    #endregion
}