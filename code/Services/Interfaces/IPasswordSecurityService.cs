namespace PersonalManagerAPI.Services.Interfaces;

/// <summary>
/// 密碼安全服務介面
/// </summary>
public interface IPasswordSecurityService
{
    /// <summary>
    /// 檢查密碼強度
    /// </summary>
    PasswordStrengthResult CheckPasswordStrength(string password);

    /// <summary>
    /// 生成安全密碼
    /// </summary>
    string GenerateSecurePassword(int length = 16, bool includeSymbols = true);

    /// <summary>
    /// 檢查密碼是否已被洩露
    /// </summary>
    Task<bool> IsPasswordCompromisedAsync(string password);

    /// <summary>
    /// 生成密碼重置令牌
    /// </summary>
    string GeneratePasswordResetToken();

    /// <summary>
    /// 驗證密碼重置令牌
    /// </summary>
    bool ValidatePasswordResetToken(string token, DateTime createdTime, TimeSpan validDuration);
}

/// <summary>
/// 密碼強度結果
/// </summary>
public class PasswordStrengthResult
{
    /// <summary>
    /// 密碼是否有效
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// 密碼強度等級
    /// </summary>
    public PasswordStrength Strength { get; set; }

    /// <summary>
    /// 密碼強度分數 (0-100)
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// 密碼問題列表
    /// </summary>
    public List<string> Issues { get; set; } = new List<string>();

    /// <summary>
    /// 建議
    /// </summary>
    public List<string> Suggestions { get; set; } = new List<string>();
}

/// <summary>
/// 密碼強度等級
/// </summary>
public enum PasswordStrength
{
    /// <summary>
    /// 非常弱
    /// </summary>
    VeryWeak = 0,

    /// <summary>
    /// 弱
    /// </summary>
    Weak = 1,

    /// <summary>
    /// 中等
    /// </summary>
    Medium = 2,

    /// <summary>
    /// 強
    /// </summary>
    Strong = 3,

    /// <summary>
    /// 非常強
    /// </summary>
    VeryStrong = 4
}