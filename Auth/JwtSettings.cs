namespace PersonalManager.Api.Auth;

public class JwtSettings
{
    public string SecretKey { get; set; } = "PersonalManager_SuperSecret_Key_2025_Must_Be_Long_Enough_256bits!";
    public string Issuer { get; set; } = "PersonalManagerAPI";
    public string Audience { get; set; } = "PersonalManagerClient";
    public int ExpiryHours { get; set; } = 24;
}
