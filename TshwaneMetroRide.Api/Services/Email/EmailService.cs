using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using TshwaneMetroRide.Api.Options;

namespace TshwaneMetroRide.Api.Services.Email;

public class EmailService : IEmailService
{
    private readonly SmtpOptions _options;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<SmtpOptions> options,
        ILogger<EmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(
        string recipientEmail,
        string subject,
        string htmlBody,
        string? textBody = null,
        CancellationToken cancellationToken = default)
    {
        ValidateConfiguration();

        var message = new MimeMessage();

        message.From.Add(
            new MailboxAddress(
                _options.FromName,
                _options.FromEmail));

        message.To.Add(
            new MailboxAddress(
                string.Empty,
                recipientEmail));

        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody,
            TextBody = textBody ??
                       "Open this email in an HTML-compatible email viewer."
        };

        message.Body = bodyBuilder.ToMessageBody();

        using var smtpClient = new SmtpClient();

        try
        {
            await smtpClient.ConnectAsync(
                _options.Host,
                _options.Port,
                SecureSocketOptions.StartTls,
                cancellationToken);

            await smtpClient.AuthenticateAsync(
                _options.Username,
                _options.Password,
                cancellationToken);

            await smtpClient.SendAsync(
                message,
                cancellationToken);

            await smtpClient.DisconnectAsync(
                true,
                cancellationToken);

            _logger.LogInformation(
                "Email sent successfully to {RecipientEmail}.",
                recipientEmail);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to send email to {RecipientEmail}.",
                recipientEmail);

            throw;
        }
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.Host))
        {
            throw new InvalidOperationException(
                "The SMTP host is not configured.");
        }

        if (_options.Port <= 0)
        {
            throw new InvalidOperationException(
                "The SMTP port is invalid.");
        }

        if (string.IsNullOrWhiteSpace(_options.Username))
        {
            throw new InvalidOperationException(
                "The SMTP username is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.Password))
        {
            throw new InvalidOperationException(
                "The SMTP password is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.FromEmail))
        {
            throw new InvalidOperationException(
                "The SMTP sender email is not configured.");
        }
    }
}