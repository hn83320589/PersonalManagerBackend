namespace PersonalManager.Api.Settings;

public class EmailSettings
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "Personal Manager";
    public string FrontendBaseUrl { get; set; } = "http://localhost:5173";

    public bool IsConfigured =>
        !string.IsNullOrEmpty(SmtpHost) &&
        !string.IsNullOrEmpty(Username) &&
        !string.IsNullOrEmpty(FromAddress);
}
