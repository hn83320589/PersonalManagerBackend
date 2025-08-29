namespace PersonalManagerAPI.Models.Enums;

/// <summary>
/// 威脅等級
/// </summary>
public enum ThreatLevel
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

/// <summary>
/// 可疑活動類型
/// </summary>
public enum SuspiciousActivityType
{
    RateLimitExceeded,
    DdosAttack,
    BruteForceLogin,
    SuspiciousUserAgent,
    UnauthorizedAccess,
    SqlInjectionAttempt,
    XssAttempt,
    FileUploadAbuse,
    Other
}