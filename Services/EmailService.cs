using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using PersonalManager.Api.Settings;

namespace PersonalManager.Api.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody);
}

/// <summary>
/// Sends real emails via SMTP when Email settings are configured.
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<EmailSettings> settings, ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
        {
            EnableSsl = _settings.UseSsl,
            Credentials = new NetworkCredential(_settings.Username, _settings.Password)
        };

        var message = new MailMessage
        {
            From = new MailAddress(_settings.FromAddress, _settings.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(to);

        await client.SendMailAsync(message);
        _logger.LogInformation("Email sent to {To} with subject: {Subject}", to, subject);
    }
}

/// <summary>
/// No-op email service used when SMTP is not configured.
/// Logs the email content to console so developers can use the reset link locally.
/// </summary>
public class NoOpEmailService : IEmailService
{
    private readonly ILogger<NoOpEmailService> _logger;

    public NoOpEmailService(ILogger<NoOpEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        _logger.LogWarning(
            "[NoOp Email] To: {To} | Subject: {Subject}\n--- Body ---\n{Body}\n---",
            to, subject, htmlBody);
        return Task.CompletedTask;
    }
}
