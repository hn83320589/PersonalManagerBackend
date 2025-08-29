using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;

namespace PersonalManagerAPI.Middleware;

/// <summary>
/// 簡化版 API 限流中介軟體
/// </summary>
public class SimpleRateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SimpleRateLimitingMiddleware> _logger;
    private readonly IConfiguration _configuration;

    // 儲存每個 IP 的請求記錄
    private static readonly ConcurrentDictionary<string, List<DateTime>> ClientRequests = new();
    
    // 儲存被封鎖的 IP
    private static readonly ConcurrentDictionary<string, DateTime> BlockedIPs = new();

    // Rate Limiting 配置
    private readonly int _requestLimit;
    private readonly int _windowMinutes;
    private readonly int _blockDurationMinutes;

    public SimpleRateLimitingMiddleware(RequestDelegate next, ILogger<SimpleRateLimitingMiddleware> logger, IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
        
        _requestLimit = _configuration.GetValue<int>("RateLimit:EndpointRequestLimit", 100);
        _windowMinutes = _configuration.GetValue<int>("RateLimit:EndpointWindowMinutes", 5);
        _blockDurationMinutes = _configuration.GetValue<int>("RateLimit:BlockDurationMinutes", 60);

        _logger.LogInformation("Simple Rate Limiting enabled: {RequestLimit} requests per {WindowMinutes} minutes", 
            _requestLimit, _windowMinutes);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var clientIp = GetClientIpAddress(context);

            // 檢查 IP 是否被封鎖
            if (IsIpBlocked(clientIp))
            {
                await HandleBlockedRequest(context, clientIp);
                return;
            }

            // 檢查 Rate Limiting
            if (!IsRequestAllowed(clientIp))
            {
                await HandleRateLimitExceeded(context, clientIp);
                return;
            }

            // 記錄請求
            RecordRequest(clientIp);

            // 定期清理過期記錄
            CleanupExpiredRecords();

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SimpleRateLimitingMiddleware");
            await _next(context); // 發生錯誤時繼續執行，避免中斷服務
        }
    }

    private bool IsRequestAllowed(string clientIp)
    {
        if (!ClientRequests.TryGetValue(clientIp, out var requests))
        {
            return true;
        }

        var now = DateTime.UtcNow;
        var windowStart = now.AddMinutes(-_windowMinutes);

        // 清理過期請求
        requests.RemoveAll(r => r < windowStart);

        // 檢查是否超過限制
        if (requests.Count >= _requestLimit)
        {
            _logger.LogWarning("Rate limit exceeded for IP: {ClientIp}, Requests: {RequestCount}", 
                clientIp, requests.Count);

            // 自動封鎖 IP
            BlockedIPs[clientIp] = now.AddMinutes(_blockDurationMinutes);
            
            return false;
        }

        return true;
    }

    private void RecordRequest(string clientIp)
    {
        var now = DateTime.UtcNow;
        var requests = ClientRequests.GetOrAdd(clientIp, _ => new List<DateTime>());
        
        lock (requests)
        {
            requests.Add(now);
        }
    }

    private bool IsIpBlocked(string clientIp)
    {
        if (BlockedIPs.TryGetValue(clientIp, out var blockExpiry))
        {
            if (DateTime.UtcNow < blockExpiry)
            {
                return true;
            }
            else
            {
                // 封鎖已過期，移除
                BlockedIPs.TryRemove(clientIp, out _);
                _logger.LogInformation("IP {ClientIp} block has expired", clientIp);
            }
        }
        return false;
    }

    private async Task HandleBlockedRequest(HttpContext context, string clientIp)
    {
        var blockExpiry = BlockedIPs[clientIp];
        var remainingTime = blockExpiry - DateTime.UtcNow;

        _logger.LogWarning("Blocked request from IP: {ClientIp}, Remaining time: {Minutes} minutes", 
            clientIp, remainingTime.TotalMinutes);

        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
        context.Response.ContentType = "application/json";

        var response = new
        {
            success = false,
            message = "Your IP address has been temporarily blocked due to too many requests",
            data = new
            {
                blockedUntil = blockExpiry,
                remainingMinutes = Math.Ceiling(remainingTime.TotalMinutes)
            },
            errors = new[] { "IP_BLOCKED", "TOO_MANY_REQUESTS" }
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private async Task HandleRateLimitExceeded(HttpContext context, string clientIp)
    {
        _logger.LogWarning("Rate limit exceeded for IP: {ClientIp}", clientIp);

        context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
        context.Response.ContentType = "application/json";
        
        // 添加 Rate Limiting 標頭
        context.Response.Headers["X-RateLimit-Limit"] = _requestLimit.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = "0";
        context.Response.Headers["X-RateLimit-Reset"] = DateTimeOffset.UtcNow.AddMinutes(_windowMinutes).ToUnixTimeSeconds().ToString();

        var response = new
        {
            success = false,
            message = "Rate limit exceeded. Please slow down your requests.",
            data = new
            {
                requestLimit = _requestLimit,
                windowMinutes = _windowMinutes,
                retryAfterSeconds = _windowMinutes * 60
            },
            errors = new[] { "RATE_LIMIT_EXCEEDED", "TOO_MANY_REQUESTS" }
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private void CleanupExpiredRecords()
    {
        // 每 50 個請求清理一次
        if (ClientRequests.Count % 50 == 0)
        {
            var cutoffTime = DateTime.UtcNow.AddMinutes(-(_windowMinutes * 2)); // 保留雙倍時間
            var expiredIps = new List<string>();

            foreach (var kvp in ClientRequests)
            {
                var requests = kvp.Value;
                lock (requests)
                {
                    requests.RemoveAll(r => r < cutoffTime);
                    if (requests.Count == 0)
                    {
                        expiredIps.Add(kvp.Key);
                    }
                }
            }

            foreach (var ip in expiredIps)
            {
                ClientRequests.TryRemove(ip, out _);
            }

            if (expiredIps.Any())
            {
                _logger.LogDebug("Cleaned up {Count} expired IP records", expiredIps.Count);
            }
        }
    }

    private string GetClientIpAddress(HttpContext context)
    {
        // 檢查反向代理標頭
        var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xForwardedFor))
        {
            return xForwardedFor.Split(',')[0].Trim();
        }

        var xRealIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xRealIp))
        {
            return xRealIp;
        }

        // 回退到連接的遠端 IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}