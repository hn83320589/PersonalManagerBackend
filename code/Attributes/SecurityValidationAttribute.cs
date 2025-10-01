using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PersonalManagerAPI.DTOs;
using System.Text.RegularExpressions;
using System.Web;

namespace PersonalManagerAPI.Attributes;

/// <summary>
/// 安全驗證屬性 - 防止惡意輸入和攻擊
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class SecurityValidationAttribute : ActionFilterAttribute
{
    /// <summary>
    /// 是否啟用 SQL 注入檢查
    /// </summary>
    public bool EnableSqlInjectionCheck { get; set; } = true;

    /// <summary>
    /// 是否啟用 XSS 檢查
    /// </summary>
    public bool EnableXssCheck { get; set; } = true;

    /// <summary>
    /// 是否啟用 HTML 標籤檢查
    /// </summary>
    public bool EnableHtmlTagCheck { get; set; } = true;

    /// <summary>
    /// 是否啟用腳本注入檢查
    /// </summary>
    public bool EnableScriptInjectionCheck { get; set; } = true;

    /// <summary>
    /// 允許的 HTML 標籤 (用於部落格內容等)
    /// </summary>
    public string[] AllowedHtmlTags { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 最大字串長度限制
    /// </summary>
    public int MaxStringLength { get; set; } = 10000;

    private readonly string[] _sqlInjectionPatterns = new[]
    {
        @"('|(\\'))+.*(;|--|\n|\r)",
        @"(;|\s)+drop\s+table",
        @"(;|\s)+delete\s+from",
        @"(;|\s)+insert\s+into",
        @"(;|\s)+update\s+.*\s+set",
        @"(;|\s)+create\s+table",
        @"(;|\s)+alter\s+table",
        @"union\s+select",
        @"select\s+.*\s+from",
        @"exec\s*\(",
        @"execute\s*\(",
        @"sp_executesql",
        @"xp_cmdshell",
        @"script\s*:",
        @"javascript\s*:",
        @"vbscript\s*:"
    };

    private readonly string[] _xssPatterns = new[]
    {
        @"<script[^>]*>.*?</script>",
        @"<iframe[^>]*>.*?</iframe>",
        @"<object[^>]*>.*?</object>",
        @"<embed[^>]*>.*?</embed>",
        @"<link[^>]*>",
        @"<meta[^>]*>",
        @"<form[^>]*>.*?</form>",
        @"on\w+\s*=\s*[""'][^""']*[""']",
        @"javascript\s*:",
        @"data\s*:\s*text/html",
        @"vbscript\s*:",
        @"expression\s*\(",
        @"@import",
        @"behaviour\s*:",
        @"-moz-binding\s*:"
    };

    private readonly string[] _scriptInjectionPatterns = new[]
    {
        @"<\s*script",
        @"</\s*script\s*>",
        @"eval\s*\(",
        @"setTimeout\s*\(",
        @"setInterval\s*\(",
        @"Function\s*\(",
        @"alert\s*\(",
        @"confirm\s*\(",
        @"prompt\s*\(",
        @"document\.",
        @"window\.",
        @"location\.",
        @"navigator\.",
        @"history\."
    };

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var validationErrors = new List<string>();

        foreach (var parameter in context.ActionArguments)
        {
            var validationResult = ValidateParameter(parameter.Key, parameter.Value);
            if (validationResult.Any())
            {
                validationErrors.AddRange(validationResult);
            }
        }

        if (validationErrors.Any())
        {
            context.Result = new BadRequestObjectResult(
                ApiResponse<object>.Failure("輸入驗證失敗：檢測到潛在的安全威脅", validationErrors));
            return;
        }

        base.OnActionExecuting(context);
    }

    private List<string> ValidateParameter(string parameterName, object? value)
    {
        var errors = new List<string>();

        if (value == null) return errors;

        // 處理字串類型
        if (value is string stringValue)
        {
            errors.AddRange(ValidateString(parameterName, stringValue));
        }
        // 處理物件類型 (DTOs)
        else if (value.GetType().IsClass && !value.GetType().IsPrimitive)
        {
            errors.AddRange(ValidateObject(parameterName, value));
        }

        return errors;
    }

    private List<string> ValidateString(string fieldName, string value)
    {
        var errors = new List<string>();

        // 檢查長度限制
        if (value.Length > MaxStringLength)
        {
            errors.Add($"{fieldName}: 字串長度超過限制 ({MaxStringLength} 字元)");
        }

        // SQL 注入檢查
        if (EnableSqlInjectionCheck && ContainsSqlInjection(value))
        {
            errors.Add($"{fieldName}: 檢測到潛在的 SQL 注入攻擊");
        }

        // XSS 檢查
        if (EnableXssCheck && ContainsXss(value))
        {
            errors.Add($"{fieldName}: 檢測到潛在的 XSS 攻擊");
        }

        // HTML 標籤檢查
        if (EnableHtmlTagCheck && ContainsDisallowedHtmlTags(value))
        {
            errors.Add($"{fieldName}: 包含不允許的 HTML 標籤");
        }

        // 腳本注入檢查
        if (EnableScriptInjectionCheck && ContainsScriptInjection(value))
        {
            errors.Add($"{fieldName}: 檢測到潛在的腳本注入攻擊");
        }

        return errors;
    }

    private List<string> ValidateObject(string objectName, object obj)
    {
        var errors = new List<string>();
        var properties = obj.GetType().GetProperties();

        foreach (var property in properties)
        {
            var value = property.GetValue(obj);
            if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
            {
                var fieldName = $"{objectName}.{property.Name}";
                errors.AddRange(ValidateString(fieldName, stringValue));
            }
            else if (value != null && value.GetType().IsClass && !value.GetType().IsPrimitive && value is not string)
            {
                var fieldName = $"{objectName}.{property.Name}";
                errors.AddRange(ValidateObject(fieldName, value));
            }
        }

        return errors;
    }

    private bool ContainsSqlInjection(string input)
    {
        var normalizedInput = input.ToLowerInvariant();
        return _sqlInjectionPatterns.Any(pattern => 
            Regex.IsMatch(normalizedInput, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline));
    }

    private bool ContainsXss(string input)
    {
        var decodedInput = HttpUtility.HtmlDecode(input);
        var normalizedInput = decodedInput.ToLowerInvariant();
        
        return _xssPatterns.Any(pattern => 
            Regex.IsMatch(normalizedInput, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline));
    }

    private bool ContainsScriptInjection(string input)
    {
        var normalizedInput = input.ToLowerInvariant();
        return _scriptInjectionPatterns.Any(pattern => 
            Regex.IsMatch(normalizedInput, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline));
    }

    private bool ContainsDisallowedHtmlTags(string input)
    {
        // 如果有允許的標籤，檢查是否只包含允許的標籤
        if (AllowedHtmlTags.Length > 0)
        {
            var htmlTagPattern = @"<\s*(\w+)[^>]*>";
            var matches = Regex.Matches(input, htmlTagPattern, RegexOptions.IgnoreCase);
            
            foreach (Match match in matches)
            {
                var tagName = match.Groups[1].Value.ToLowerInvariant();
                if (!AllowedHtmlTags.Contains(tagName))
                {
                    return true; // 包含不允許的標籤
                }
            }
        }
        else
        {
            // 如果沒有允許的標籤，檢查是否包含任何 HTML 標籤
            return Regex.IsMatch(input, @"<[^>]+>", RegexOptions.IgnoreCase);
        }

        return false;
    }
}

/// <summary>
/// 輸入清理和編碼工具
/// </summary>
public static class InputSanitizer
{
    /// <summary>
    /// 清理 HTML 內容，移除危險標籤和屬性
    /// </summary>
    public static string SanitizeHtml(string input, string[]? allowedTags = null)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        var decoded = HttpUtility.HtmlDecode(input);
        
        // 移除所有腳本標籤
        decoded = Regex.Replace(decoded, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        
        // 移除危險的事件處理器
        decoded = Regex.Replace(decoded, @"on\w+\s*=\s*[""'][^""']*[""']", "", RegexOptions.IgnoreCase);
        
        // 移除 JavaScript 和 VBScript 協議
        decoded = Regex.Replace(decoded, @"(javascript|vbscript):", "", RegexOptions.IgnoreCase);
        
        // 如果沒有允許的標籤，移除所有 HTML 標籤
        if (allowedTags == null || !allowedTags.Any())
        {
            decoded = Regex.Replace(decoded, @"<[^>]+>", "");
        }
        
        return HttpUtility.HtmlEncode(decoded);
    }

    /// <summary>
    /// 驗證並清理 SQL 查詢參數
    /// </summary>
    public static string SanitizeSqlParameter(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        // 移除單引號並轉義
        return input.Replace("'", "''").Replace("--", "").Replace(";", "");
    }

    /// <summary>
    /// 驗證電子郵件格式
    /// </summary>
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) return false;
        
        var emailPattern = @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$";
        return Regex.IsMatch(email, emailPattern) && email.Length <= 320; // RFC 5321 限制
    }

    /// <summary>
    /// 驗證用戶名格式 (只允許字母、數字、下劃線、連字號)
    /// </summary>
    public static bool IsValidUsername(string username)
    {
        if (string.IsNullOrEmpty(username)) return false;
        
        var usernamePattern = @"^[a-zA-Z0-9_-]{3,50}$";
        return Regex.IsMatch(username, usernamePattern);
    }

    /// <summary>
    /// 驗證 URL 格式
    /// </summary>
    public static bool IsValidUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return false;
        
        return Uri.TryCreate(url, UriKind.Absolute, out var result) 
               && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}