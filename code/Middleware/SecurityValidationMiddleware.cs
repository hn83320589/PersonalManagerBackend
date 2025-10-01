using PersonalManagerAPI.DTOs;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PersonalManagerAPI.Middleware;

/// <summary>
/// 安全驗證中介軟體 - 在請求處理之前進行安全檢查
/// </summary>
public class SecurityValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityValidationMiddleware> _logger;
    private readonly SecurityValidationOptions _options;

    public SecurityValidationMiddleware(
        RequestDelegate next, 
        ILogger<SecurityValidationMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _options = new SecurityValidationOptions();
        configuration.GetSection("SecurityValidation").Bind(_options);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 檢查請求頭安全性
        var headerSecurityResult = ValidateRequestHeaders(context.Request);
        if (!headerSecurityResult.IsValid)
        {
            await WriteSecurityErrorResponse(context, headerSecurityResult.ErrorMessage);
            return;
        }

        // 檢查請求 URL 安全性
        var urlSecurityResult = ValidateRequestUrl(context.Request);
        if (!urlSecurityResult.IsValid)
        {
            await WriteSecurityErrorResponse(context, urlSecurityResult.ErrorMessage);
            return;
        }

        // 檢查請求體安全性 (只對 POST/PUT/PATCH 請求)
        if (context.Request.Method is "POST" or "PUT" or "PATCH")
        {
            var bodySecurityResult = await ValidateRequestBodyAsync(context.Request);
            if (!bodySecurityResult.IsValid)
            {
                await WriteSecurityErrorResponse(context, bodySecurityResult.ErrorMessage);
                return;
            }
        }

        await _next(context);
    }

    private ValidationResult ValidateRequestHeaders(HttpRequest request)
    {
        try
        {
            // 檢查 User-Agent
            var userAgent = request.Headers["User-Agent"].FirstOrDefault();
            if (!string.IsNullOrEmpty(userAgent))
            {
                if (ContainsSuspiciousPatterns(userAgent))
                {
                    _logger.LogWarning("可疑的 User-Agent 被攔截: {UserAgent}, IP: {ClientIP}", 
                        userAgent, GetClientIpAddress(request));
                    return ValidationResult.Invalid("檢測到可疑的用戶代理");
                }
            }

            // 檢查 Referer
            var referer = request.Headers["Referer"].FirstOrDefault();
            if (!string.IsNullOrEmpty(referer) && ContainsSuspiciousPatterns(referer))
            {
                _logger.LogWarning("可疑的 Referer 被攔截: {Referer}, IP: {ClientIP}", 
                    referer, GetClientIpAddress(request));
                return ValidationResult.Invalid("檢測到可疑的來源頁面");
            }

            // 檢查自訂頭部
            foreach (var header in request.Headers)
            {
                if (ContainsSuspiciousPatterns(header.Key) || 
                    header.Value.Any(v => ContainsSuspiciousPatterns(v)))
                {
                    _logger.LogWarning("可疑的請求頭被攔截: {HeaderName}, IP: {ClientIP}", 
                        header.Key, GetClientIpAddress(request));
                    return ValidationResult.Invalid("檢測到可疑的請求頭");
                }
            }

            return ValidationResult.Valid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "請求頭驗證時發生錯誤");
            return ValidationResult.Invalid("請求頭驗證失敗");
        }
    }

    private ValidationResult ValidateRequestUrl(HttpRequest request)
    {
        try
        {
            var fullUrl = $"{request.Scheme}://{request.Host}{request.PathBase}{request.Path}{request.QueryString}";

            // 檢查 URL 長度
            if (fullUrl.Length > _options.MaxUrlLength)
            {
                _logger.LogWarning("過長的 URL 被攔截: 長度 {Length}, IP: {ClientIP}", 
                    fullUrl.Length, GetClientIpAddress(request));
                return ValidationResult.Invalid($"URL 長度超過限制 ({_options.MaxUrlLength} 字元)");
            }

            // 檢查路徑穿越攻擊
            var decodedPath = Uri.UnescapeDataString(request.Path.Value ?? "");
            if (decodedPath.Contains("..") || decodedPath.Contains("~") || 
                decodedPath.Contains("%2e%2e") || decodedPath.Contains("%7e"))
            {
                _logger.LogWarning("路徑穿越攻擊被攔截: {Path}, IP: {ClientIP}", 
                    decodedPath, GetClientIpAddress(request));
                return ValidationResult.Invalid("檢測到路徑穿越攻擊");
            }

            // 檢查查詢參數
            foreach (var queryParam in request.Query)
            {
                if (ContainsSuspiciousPatterns(queryParam.Key) || 
                    queryParam.Value.Any(v => ContainsSuspiciousPatterns(v)))
                {
                    _logger.LogWarning("可疑的查詢參數被攔截: {ParamName}, IP: {ClientIP}", 
                        queryParam.Key, GetClientIpAddress(request));
                    return ValidationResult.Invalid("檢測到可疑的查詢參數");
                }
            }

            return ValidationResult.Valid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "URL 驗證時發生錯誤");
            return ValidationResult.Invalid("URL 驗證失敗");
        }
    }

    private async Task<ValidationResult> ValidateRequestBodyAsync(HttpRequest request)
    {
        try
        {
            // 檢查內容長度
            if (request.ContentLength > _options.MaxBodyLength)
            {
                _logger.LogWarning("過大的請求體被攔截: 大小 {Size}, IP: {ClientIP}", 
                    request.ContentLength, GetClientIpAddress(request));
                return ValidationResult.Invalid($"請求體大小超過限制 ({_options.MaxBodyLength / (1024 * 1024)} MB)");
            }

            // 讀取請求體
            request.EnableBuffering();
            var buffer = new byte[request.ContentLength ?? 0];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            request.Body.Position = 0;

            var bodyContent = Encoding.UTF8.GetString(buffer);

            // 檢查請求體內容
            if (ContainsSuspiciousPatterns(bodyContent))
            {
                _logger.LogWarning("可疑的請求體內容被攔截, IP: {ClientIP}", GetClientIpAddress(request));
                return ValidationResult.Invalid("檢測到可疑的請求內容");
            }

            return ValidationResult.Valid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "請求體驗證時發生錯誤");
            return ValidationResult.Invalid("請求體驗證失敗");
        }
    }

    private bool ContainsSuspiciousPatterns(string input)
    {
        if (string.IsNullOrEmpty(input)) return false;

        var normalizedInput = input.ToLowerInvariant();
        
        // SQL 注入模式
        var sqlPatterns = new[]
        {
            @"union\s+select", @"drop\s+table", @"delete\s+from", @"insert\s+into",
            @"update\s+.*\s+set", @"exec\s*\(", @"xp_cmdshell", @"sp_executesql"
        };

        // XSS 模式
        var xssPatterns = new[]
        {
            @"<script", @"javascript:", @"vbscript:", @"onload\s*=", @"onerror\s*=",
            @"onclick\s*=", @"onmouseover\s*=", @"alert\s*\(", @"document\."
        };

        // 命令注入模式
        var cmdPatterns = new[]
        {
            @";\s*(ls|dir|cat|type|more|head|tail)", @"&&\s*(ls|dir|cat|type)",
            @"\|\s*(ls|dir|cat|type)", @"cmd\s*/c", @"powershell", @"bash\s+-c"
        };

        // 檢查所有模式
        var allPatterns = sqlPatterns.Concat(xssPatterns).Concat(cmdPatterns);
        return allPatterns.Any(pattern => 
            Regex.IsMatch(normalizedInput, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline));
    }

    private string GetClientIpAddress(HttpRequest request)
    {
        var forwarded = request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded))
        {
            return forwarded.Split(',').FirstOrDefault()?.Trim() ?? "Unknown";
        }

        var realIp = request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private async Task WriteSecurityErrorResponse(HttpContext context, string errorMessage)
    {
        context.Response.StatusCode = 400;
        context.Response.ContentType = "application/json";

        var response = ApiResponse<object>.Failure(errorMessage);
        var jsonResponse = JsonSerializer.Serialize(response);
        
        await context.Response.WriteAsync(jsonResponse);

        _logger.LogWarning("安全威脅被攔截: {ErrorMessage}, IP: {ClientIP}, URL: {RequestUrl}",
            errorMessage, 
            GetClientIpAddress(context.Request),
            $"{context.Request.Method} {context.Request.Path}{context.Request.QueryString}");
    }
}

/// <summary>
/// 安全驗證配置選項
/// </summary>
public class SecurityValidationOptions
{
    /// <summary>
    /// 最大 URL 長度 (預設 2048 字元)
    /// </summary>
    public int MaxUrlLength { get; set; } = 2048;

    /// <summary>
    /// 最大請求體長度 (預設 10MB)
    /// </summary>
    public long MaxBodyLength { get; set; } = 10 * 1024 * 1024;

    /// <summary>
    /// 是否啟用嚴格模式
    /// </summary>
    public bool StrictMode { get; set; } = true;

    /// <summary>
    /// 白名單 IP 地址 (不進行安全檢查)
    /// </summary>
    public string[] WhitelistedIpAddresses { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 排除的路徑 (不進行安全檢查)
    /// </summary>
    public string[] ExcludedPaths { get; set; } = Array.Empty<string>();
}

/// <summary>
/// 驗證結果
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; private set; }
    public string ErrorMessage { get; private set; } = string.Empty;

    private ValidationResult(bool isValid, string errorMessage = "")
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    public static ValidationResult Valid() => new(true);
    public static ValidationResult Invalid(string errorMessage) => new(false, errorMessage);
}