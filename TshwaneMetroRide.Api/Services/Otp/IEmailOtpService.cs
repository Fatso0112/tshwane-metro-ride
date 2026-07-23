using TshwaneMetroRide.Api.Models;

namespace TshwaneMetroRide.Api.Services.Otp;

public interface IEmailOtpService
{
    Task<EmailOtpChallenge> CreateAndSendAsync(
        Passenger passenger,
        CancellationToken cancellationToken = default);

    Task<EmailOtpVerificationResult> VerifyAsync(
        Guid verificationId,
        string otp,
        CancellationToken cancellationToken = default);

    Task<EmailOtpChallenge> CreatePasswordResetAndSendAsync(
        Passenger passenger,
        CancellationToken cancellationToken = default);

    Task<EmailOtpVerificationResult> ResetPasswordAsync(
        Guid verificationId,
        string otp,
        string newPassword,
        CancellationToken cancellationToken = default);
}

public record EmailOtpChallenge(
    Guid VerificationId,
    DateTime ExpiresAtUtc);

public enum EmailOtpVerificationStatus
{
    Success,
    NotFound,
    Invalid,
    Expired,
    AlreadyUsed,
    MaximumAttemptsReached,
    NewPasswordMatchesCurrent
}

public record EmailOtpVerificationResult(
    EmailOtpVerificationStatus Status,
    Passenger? Passenger = null,
    int? RemainingAttempts = null);