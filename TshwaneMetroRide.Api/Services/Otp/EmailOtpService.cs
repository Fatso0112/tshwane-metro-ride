using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using TshwaneMetroRide.Api.Data;
using TshwaneMetroRide.Api.Models;
using TshwaneMetroRide.Api.Options;
using TshwaneMetroRide.Api.Services.Email;
using Microsoft.AspNetCore.Identity;

namespace TshwaneMetroRide.Api.Services.Otp;

public class EmailOtpService : IEmailOtpService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly OtpOptions _options;
    private readonly ILogger<EmailOtpService> _logger;

    private readonly IPasswordHasher<Passenger> _passwordHasher;

    public EmailOtpService(
        ApplicationDbContext context,
        IEmailService emailService,
        IOptions<OtpOptions> options,
        ILogger<EmailOtpService> logger)
    {
        _context = context;
        _emailService = emailService;
        _options = options.Value;
        _logger = logger;
        _passwordHasher = new PasswordHasher<Passenger>();
    }

    public async Task<EmailOtpChallenge>
        CreateAndSendAsync(
            Passenger passenger,
            CancellationToken cancellationToken = default)
    {
        ValidateConfiguration();

        var currentTimeUtc = DateTime.UtcNow;

        var previousVerifications =
            await _context.EmailOtpVerifications
                .Where(verification =>
                    verification.PassengerId ==
                        passenger.Id &&
                    !verification.IsUsed)
                .ToListAsync(cancellationToken);

        foreach (var previousVerification
                 in previousVerifications)
        {
            previousVerification.IsUsed = true;
        }

        var verificationId = Guid.NewGuid();

        var otp = RandomNumberGenerator
            .GetInt32(0, 1_000_000)
            .ToString("D6");

        var expiresAtUtc = currentTimeUtc
            .AddMinutes(_options.ExpiryMinutes);

        var verification =
            new EmailOtpVerification
            {
                VerificationId = verificationId,
                OtpHash = HashOtp(
                    verificationId,
                    otp),

                Purpose = "EmailVerification",
                CreatedAtUtc = currentTimeUtc,
                ExpiresAtUtc = expiresAtUtc,
                FailedAttempts = 0,
                MaximumAttempts =
                    _options.MaximumAttempts,

                IsUsed = false,
                PassengerId = passenger.Id
            };

        _context.EmailOtpVerifications.Add(
            verification);

        await _context.SaveChangesAsync(
            cancellationToken);

        var safeName = WebUtility.HtmlEncode(
            passenger.FullName);

        var htmlBody =
            $$"""
            <!DOCTYPE html>
            <html>
            <body style="font-family: Arial, sans-serif;">
                <h2>Tshwane Metro Ride</h2>

                <p>Hello {{safeName}},</p>

                <p>
                    Use the following one-time password
                    to verify your email address:
                </p>

                <div style="
                    font-size: 32px;
                    font-weight: bold;
                    letter-spacing: 8px;
                    margin: 24px 0;">
                    {{otp}}
                </div>

                <p>
                    This code expires in
                    {{_options.ExpiryMinutes}} minutes.
                </p>

                <p>
                    Do not share this code with anyone.
                </p>
            </body>
            </html>
            """;

        var textBody =
            $"""
            Hello {passenger.FullName},

            Your Tshwane Metro Ride verification code is:

            {otp}

            This code expires in {_options.ExpiryMinutes} minutes.

            Do not share this code with anyone.
            """;

        await _emailService.SendEmailAsync(
            passenger.Email,
            "Verify your Tshwane Metro Ride email",
            htmlBody,
            textBody,
            cancellationToken);

        _logger.LogInformation(
            "Email verification OTP sent for passenger {PassengerId}.",
            passenger.Id);

        return new EmailOtpChallenge(
            verificationId,
            expiresAtUtc);
    }

    public async Task<EmailOtpVerificationResult>
        VerifyAsync(
            Guid verificationId,
            string otp,
            CancellationToken cancellationToken = default)
    {
        var verification =
            await _context.EmailOtpVerifications
                .Include(item => item.Passenger)
                .SingleOrDefaultAsync(
                    item =>
                        item.VerificationId ==
                            verificationId &&
                        item.Purpose ==
                            "EmailVerification",
                    cancellationToken);

        if (verification is null)
        {
            return new EmailOtpVerificationResult(
                EmailOtpVerificationStatus.NotFound);
        }

        if (verification.IsUsed)
        {
            return new EmailOtpVerificationResult(
                EmailOtpVerificationStatus.AlreadyUsed);
        }

        var currentTimeUtc = DateTime.UtcNow;

        if (currentTimeUtc >
            verification.ExpiresAtUtc)
        {
            verification.IsUsed = true;

            await _context.SaveChangesAsync(
                cancellationToken);

            return new EmailOtpVerificationResult(
                EmailOtpVerificationStatus.Expired);
        }

        if (verification.FailedAttempts >=
            verification.MaximumAttempts)
        {
            return new EmailOtpVerificationResult(
                EmailOtpVerificationStatus
                    .MaximumAttemptsReached);
        }

        var submittedHash = HashOtp(
            verification.VerificationId,
            otp.Trim());

        var expectedHashBytes =
            Convert.FromBase64String(
                verification.OtpHash);

        var submittedHashBytes =
            Convert.FromBase64String(
                submittedHash);

        var isValid =
            CryptographicOperations.FixedTimeEquals(
                expectedHashBytes,
                submittedHashBytes);

        if (!isValid)
        {
            verification.FailedAttempts++;

            await _context.SaveChangesAsync(
                cancellationToken);

            var remainingAttempts = Math.Max(
                0,
                verification.MaximumAttempts -
                    verification.FailedAttempts);

            var status = remainingAttempts == 0
                ? EmailOtpVerificationStatus
                    .MaximumAttemptsReached
                : EmailOtpVerificationStatus.Invalid;

            return new EmailOtpVerificationResult(
                status,
                RemainingAttempts:
                    remainingAttempts);
        }

        verification.IsUsed = true;
        verification.VerifiedAtUtc =
            currentTimeUtc;

        verification.Passenger.IsEmailVerified =
            true;

        verification.Passenger
            .EmailVerifiedAtUtc = currentTimeUtc;

        await _context.SaveChangesAsync(
            cancellationToken);

        return new EmailOtpVerificationResult(
            EmailOtpVerificationStatus.Success,
            verification.Passenger);
    }

    public async Task<EmailOtpChallenge>
    CreatePasswordResetAndSendAsync(
        Passenger passenger,
        CancellationToken cancellationToken = default)
    {
        ValidateConfiguration();

        var currentTimeUtc = DateTime.UtcNow;

        var previousResetOtps =
            await _context.EmailOtpVerifications
                .Where(verification =>
                    verification.PassengerId ==
                        passenger.Id &&
                    verification.Purpose ==
                        "PasswordReset" &&
                    !verification.IsUsed)
                .ToListAsync(cancellationToken);

        foreach (var previousOtp in previousResetOtps)
        {
            previousOtp.IsUsed = true;
        }

        var verificationId = Guid.NewGuid();

        var otp = RandomNumberGenerator
            .GetInt32(0, 1_000_000)
            .ToString("D6");

        var expiresAtUtc = currentTimeUtc
            .AddMinutes(_options.ExpiryMinutes);

        var verification =
            new EmailOtpVerification
            {
                VerificationId = verificationId,

                OtpHash = HashOtp(
                    verificationId,
                    otp),

                Purpose = "PasswordReset",

                CreatedAtUtc = currentTimeUtc,

                ExpiresAtUtc = expiresAtUtc,

                FailedAttempts = 0,

                MaximumAttempts =
                    _options.MaximumAttempts,

                IsUsed = false,

                PassengerId = passenger.Id
            };

        _context.EmailOtpVerifications.Add(
            verification);

        await _context.SaveChangesAsync(
            cancellationToken);

        var safeName = WebUtility.HtmlEncode(
            passenger.FullName);

        var htmlBody =
            $$"""
            <!DOCTYPE html>
            <html>
            <body style="font-family: Arial, sans-serif;">
                <h2>Tshwane Metro Ride</h2>

                <p>Hello {{safeName}},</p>

                <p>
                    We received a request to reset your
                    Tshwane Metro Ride password.
                </p>

                <p>Your password-reset code is:</p>

                <div style="
                    font-size: 32px;
                    font-weight: bold;
                    letter-spacing: 8px;
                    margin: 24px 0;">
                    {{otp}}
                </div>

                <p>
                    This code expires in
                    {{_options.ExpiryMinutes}} minutes.
                </p>

                <p>
                    If you did not request a password reset,
                    you can ignore this email.
                </p>

                <p>Do not share this code with anyone.</p>
            </body>
            </html>
            """;

        var textBody =
            $"""
            Hello {passenger.FullName},

            Your Tshwane Metro Ride password-reset code is:

            {otp}

            This code expires in {_options.ExpiryMinutes} minutes.

            If you did not request this reset, ignore this email.
            """;

        await _emailService.SendEmailAsync(
            passenger.Email,
            "Reset your Tshwane Metro Ride password",
            htmlBody,
            textBody,
            cancellationToken);

        _logger.LogInformation(
            "Password-reset OTP sent for passenger {PassengerId}.",
            passenger.Id);

        return new EmailOtpChallenge(
            verificationId,
            expiresAtUtc);
    }

    public async Task<EmailOtpVerificationResult>
    ResetPasswordAsync(
        Guid verificationId,
        string otp,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var verification =
            await _context.EmailOtpVerifications
                .Include(item => item.Passenger)
                .SingleOrDefaultAsync(
                    item =>
                        item.VerificationId ==
                            verificationId &&
                        item.Purpose ==
                            "PasswordReset",
                    cancellationToken);

        if (verification is null)
        {
            return new EmailOtpVerificationResult(
                EmailOtpVerificationStatus.NotFound);
        }

        if (verification.IsUsed)
        {
            return new EmailOtpVerificationResult(
                EmailOtpVerificationStatus.AlreadyUsed);
        }

        var currentTimeUtc = DateTime.UtcNow;

        if (currentTimeUtc > verification.ExpiresAtUtc)
        {
            verification.IsUsed = true;

            await _context.SaveChangesAsync(
                cancellationToken);

            return new EmailOtpVerificationResult(
                EmailOtpVerificationStatus.Expired);
        }

        if (verification.FailedAttempts >=
            verification.MaximumAttempts)
        {
            return new EmailOtpVerificationResult(
                EmailOtpVerificationStatus
                    .MaximumAttemptsReached);
        }

        var submittedHash = HashOtp(
            verification.VerificationId,
            otp.Trim());

        var expectedHashBytes =
            Convert.FromBase64String(
                verification.OtpHash);

        var submittedHashBytes =
            Convert.FromBase64String(
                submittedHash);

        var isValid =
            CryptographicOperations.FixedTimeEquals(
                expectedHashBytes,
                submittedHashBytes);

        if (!isValid)
        {
            verification.FailedAttempts++;

            await _context.SaveChangesAsync(
                cancellationToken);

            var remainingAttempts = Math.Max(
                0,
                verification.MaximumAttempts -
                    verification.FailedAttempts);

            var status = remainingAttempts == 0
                ? EmailOtpVerificationStatus
                    .MaximumAttemptsReached
                : EmailOtpVerificationStatus.Invalid;

            return new EmailOtpVerificationResult(
                status,
                RemainingAttempts:
                    remainingAttempts);
        }

        var passenger = verification.Passenger;

        var passwordResult =
            _passwordHasher.VerifyHashedPassword(
                passenger,
                passenger.PasswordHash,
                newPassword);

        if (passwordResult !=
            PasswordVerificationResult.Failed)
        {
            return new EmailOtpVerificationResult(
                EmailOtpVerificationStatus
                    .NewPasswordMatchesCurrent);
        }

        passenger.PasswordHash =
            _passwordHasher.HashPassword(
                passenger,
                newPassword);

        verification.IsUsed = true;
        verification.VerifiedAtUtc =
            currentTimeUtc;

        await _context.SaveChangesAsync(
            cancellationToken);

        return new EmailOtpVerificationResult(
            EmailOtpVerificationStatus.Success,
            passenger);
    }

    private string HashOtp(
        Guid verificationId,
        string otp)
    {
        var keyBytes = GetHashKeyBytes();

        using var hmac =
            new HMACSHA256(keyBytes);

        var payload = Encoding.UTF8.GetBytes(
            $"{verificationId:N}:{otp}");

        var hash = hmac.ComputeHash(payload);

        return Convert.ToBase64String(hash);
    }

    private byte[] GetHashKeyBytes()
    {
        try
        {
            return Convert.FromBase64String(
                _options.HashKey);
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException(
                "Otp:HashKey must be a valid Base64 value.",
                exception);
        }
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(
            _options.HashKey))
        {
            throw new InvalidOperationException(
                "Otp:HashKey is not configured.");
        }

        if (_options.ExpiryMinutes <= 0)
        {
            throw new InvalidOperationException(
                "OTP expiry must be greater than zero.");
        }

        if (_options.MaximumAttempts <= 0)
        {
            throw new InvalidOperationException(
                "OTP maximum attempts must be greater than zero.");
        }
    }
}