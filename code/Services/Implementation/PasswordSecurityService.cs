using PersonalManagerAPI.Services.Interfaces;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

namespace PersonalManagerAPI.Services.Implementation;

/// <summary>
/// 密碼安全服務 - 提供密碼強度檢查、生成和相關安全功能
/// </summary>
public class PasswordSecurityService : IPasswordSecurityService
{
    private readonly ILogger<PasswordSecurityService> _logger;
    private readonly PasswordSecurityOptions _options;

    // 常見弱密碼列表 (部分)
    private readonly HashSet<string> _commonPasswords = new(StringComparer.OrdinalIgnoreCase)
    {
        "123456", "password", "123456789", "12345678", "12345", "1234567", "1234567890",
        "qwerty", "abc123", "password123", "admin", "letmein", "welcome", "monkey",
        "master", "shadow", "dragon", "trustno1", "superman", "batman", "football",
        "baseball", "welcome1", "hello123", "test123", "user123", "admin123"
    };

    public PasswordSecurityService(ILogger<PasswordSecurityService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _options = new PasswordSecurityOptions();
        configuration.GetSection("PasswordSecurity").Bind(_options);
    }

    /// <summary>
    /// 檢查密碼強度
    /// </summary>
    public PasswordStrengthResult CheckPasswordStrength(string password)
    {
        try
        {
            if (string.IsNullOrEmpty(password))
            {
                return new PasswordStrengthResult
                {
                    IsValid = false,
                    Strength = PasswordStrength.VeryWeak,
                    Score = 0,
                    Issues = new List<string> { "密碼不能為空" }
                };
            }

            var issues = new List<string>();
            var score = 0;

            // 長度檢查
            if (password.Length < _options.MinLength)
            {
                issues.Add($"密碼長度至少需要 {_options.MinLength} 字元");
            }
            else if (password.Length >= _options.MinLength)
            {
                score += 10;
                if (password.Length >= 12) score += 10;
                if (password.Length >= 16) score += 10;
            }

            // 字符複雜度檢查
            var hasLower = Regex.IsMatch(password, @"[a-z]");
            var hasUpper = Regex.IsMatch(password, @"[A-Z]");
            var hasDigit = Regex.IsMatch(password, @"\d");
            var hasSpecial = Regex.IsMatch(password, @"[!@#$%^&*(),.?""{}|<>_+=\-\[\]\\;':\/~`]");

            if (!hasLower && _options.RequireLowercase)
                issues.Add("密碼必須包含至少一個小寫字母");
            if (!hasUpper && _options.RequireUppercase)
                issues.Add("密碼必須包含至少一個大寫字母");
            if (!hasDigit && _options.RequireDigit)
                issues.Add("密碼必須包含至少一個數字");
            if (!hasSpecial && _options.RequireSpecialChar)
                issues.Add("密碼必須包含至少一個特殊字符");

            if (hasLower) score += 10;
            if (hasUpper) score += 10;
            if (hasDigit) score += 10;
            if (hasSpecial) score += 15;

            // 字符多樣性檢查
            var uniqueChars = password.ToCharArray().Distinct().Count();
            var diversityRatio = (double)uniqueChars / password.Length;
            if (diversityRatio < 0.5)
            {
                issues.Add("密碼重複字符過多，建議增加字符多樣性");
            }
            else
            {
                score += (int)(diversityRatio * 20);
            }

            // 常見密碼檢查
            if (IsCommonPassword(password))
            {
                issues.Add("這是一個常見的弱密碼，請選擇更安全的密碼");
                score -= 20;
            }

            // 連續字符檢查
            if (HasSequentialCharacters(password))
            {
                issues.Add("密碼包含連續字符序列，建議避免使用");
                score -= 10;
            }

            // 重複模式檢查
            if (HasRepeatingPatterns(password))
            {
                issues.Add("密碼包含重複模式，建議增加複雜性");
                score -= 10;
            }

            // 個人信息檢查 (基本檢查)
            if (ContainsPersonalInfo(password))
            {
                issues.Add("密碼不應包含個人信息");
                score -= 15;
            }

            score = Math.Max(0, Math.Min(100, score)); // 確保分數在 0-100 之間
            var strength = CalculatePasswordStrength(score);
            var isValid = issues.Count == 0 && score >= _options.MinScore;

            return new PasswordStrengthResult
            {
                IsValid = isValid,
                Strength = strength,
                Score = score,
                Issues = issues
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "密碼強度檢查時發生錯誤");
            return new PasswordStrengthResult
            {
                IsValid = false,
                Strength = PasswordStrength.VeryWeak,
                Score = 0,
                Issues = new List<string> { "密碼強度檢查失敗" }
            };
        }
    }

    /// <summary>
    /// 生成安全密碼
    /// </summary>
    public string GenerateSecurePassword(int length = 16, bool includeSymbols = true)
    {
        if (length < 8) length = 8;

        var lowercase = "abcdefghijklmnopqrstuvwxyz";
        var uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var digits = "0123456789";
        var symbols = includeSymbols ? "!@#$%^&*()_+-=[]{}|;':\",./<>?" : "";

        var allChars = lowercase + uppercase + digits + symbols;
        var password = new StringBuilder();

        using var rng = RandomNumberGenerator.Create();

        // 確保至少包含每種類型的字符
        password.Append(GetRandomChar(lowercase, rng));
        password.Append(GetRandomChar(uppercase, rng));
        password.Append(GetRandomChar(digits, rng));
        if (includeSymbols)
            password.Append(GetRandomChar(symbols, rng));

        // 填充剩餘長度
        for (int i = password.Length; i < length; i++)
        {
            password.Append(GetRandomChar(allChars, rng));
        }

        // 隨機化順序
        return new string(password.ToString().OrderBy(_ => GetRandomNumber(rng)).ToArray());
    }

    /// <summary>
    /// 檢查密碼是否已被洩露 (模擬 HaveIBeenPwned 檢查)
    /// </summary>
    public Task<bool> IsPasswordCompromisedAsync(string password)
    {
        try
        {
            // 這裡實現與 HaveIBeenPwned API 的集成
            // 為了演示，我們只檢查是否為常見密碼
            return Task.FromResult(IsCommonPassword(password));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "檢查密碼洩露狀態時發生錯誤");
            return Task.FromResult(false); // 錯誤時假設密碼是安全的
        }
    }

    /// <summary>
    /// 生成密碼重置令牌
    /// </summary>
    public string GeneratePasswordResetToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var randomBytes = new byte[32];
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    /// <summary>
    /// 驗證密碼重置令牌
    /// </summary>
    public bool ValidatePasswordResetToken(string token, DateTime createdTime, TimeSpan validDuration)
    {
        if (string.IsNullOrEmpty(token)) return false;
        if (DateTime.UtcNow - createdTime > validDuration) return false;

        // 這裡可以添加更多的令牌驗證邏輯
        return token.Length == 44; // Base64 編碼的 32 字節應該是 44 字符
    }

    private bool IsCommonPassword(string password)
    {
        return _commonPasswords.Contains(password) ||
               _commonPasswords.Any(common => password.ToLowerInvariant().Contains(common));
    }

    private bool HasSequentialCharacters(string password)
    {
        var sequences = new[]
        {
            "0123456789", "abcdefghijklmnopqrstuvwxyz", "qwertyuiop", "asdfghjkl", "zxcvbnm"
        };

        foreach (var sequence in sequences)
        {
            for (int i = 0; i <= sequence.Length - 3; i++)
            {
                var subseq = sequence.Substring(i, 3);
                if (password.ToLowerInvariant().Contains(subseq))
                    return true;
            }
        }

        return false;
    }

    private bool HasRepeatingPatterns(string password)
    {
        // 檢查連續重複字符
        for (int i = 0; i < password.Length - 2; i++)
        {
            if (password[i] == password[i + 1] && password[i + 1] == password[i + 2])
                return true;
        }

        // 檢查重複子字串
        for (int patternLength = 2; patternLength <= password.Length / 2; patternLength++)
        {
            var pattern = password.Substring(0, patternLength);
            var expectedLength = (password.Length / patternLength) * patternLength;
            var repeated = string.Concat(Enumerable.Repeat(pattern, password.Length / patternLength));
            
            if (password.StartsWith(repeated) && repeated.Length >= password.Length / 2)
                return true;
        }

        return false;
    }

    private bool ContainsPersonalInfo(string password)
    {
        // 基本的個人信息模式檢查
        var personalInfoPatterns = new[]
        {
            @"\b(admin|user|test|demo)\b",
            @"\b\d{4}\b", // 可能的年份
            @"\b(password|pass|pwd)\b"
        };

        return personalInfoPatterns.Any(pattern =>
            Regex.IsMatch(password, pattern, RegexOptions.IgnoreCase));
    }

    private PasswordStrength CalculatePasswordStrength(int score)
    {
        return score switch
        {
            >= 80 => PasswordStrength.VeryStrong,
            >= 60 => PasswordStrength.Strong,
            >= 40 => PasswordStrength.Medium,
            >= 20 => PasswordStrength.Weak,
            _ => PasswordStrength.VeryWeak
        };
    }

    private char GetRandomChar(string chars, RandomNumberGenerator rng)
    {
        var randomBytes = new byte[1];
        rng.GetBytes(randomBytes);
        return chars[randomBytes[0] % chars.Length];
    }

    private int GetRandomNumber(RandomNumberGenerator rng)
    {
        var randomBytes = new byte[4];
        rng.GetBytes(randomBytes);
        return BitConverter.ToInt32(randomBytes, 0);
    }
}

/// <summary>
/// 密碼安全配置選項
/// </summary>
public class PasswordSecurityOptions
{
    public int MinLength { get; set; } = 8;
    public int MinScore { get; set; } = 60;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecialChar { get; set; } = true;
    public TimeSpan ResetTokenValidDuration { get; set; } = TimeSpan.FromHours(1);
}