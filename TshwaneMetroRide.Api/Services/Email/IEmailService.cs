namespace TshwaneMetroRide.Api.Services.Email;

public interface IEmailService
{
    Task SendEmailAsync(
        string recipientEmail,
        string subject,
        string htmlBody,
        string? textBody = null,
        CancellationToken cancellationToken = default);
}