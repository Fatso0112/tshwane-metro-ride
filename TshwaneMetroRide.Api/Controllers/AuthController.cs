using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TshwaneMetroRide.Api.Data;
using TshwaneMetroRide.Api.DTOs.Auth;
using TshwaneMetroRide.Api.Interfaces;
using TshwaneMetroRide.Api.Models;
using TshwaneMetroRide.Api.Options;
using TshwaneMetroRide.Api.Services.Otp;

namespace TshwaneMetroRide.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    private readonly
        IPasswordHasher<Passenger> _passwordHasher;

    private readonly ITokenServices _tokenService;

    private readonly IEmailOtpService _emailOtpService;

    private readonly OtpOptions _otpOptions;

    public AuthController(
        ApplicationDbContext context,
        IPasswordHasher<Passenger> passwordHasher,
        ITokenServices tokenService,
        IEmailOtpService emailOtpService,
        IOptions<OtpOptions> otpOptions)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _emailOtpService = emailOtpService;
        _otpOptions = otpOptions.Value;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterPassengerRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email
            .Trim()
            .ToLowerInvariant();

        var emailExists = await _context.Passengers
            .AnyAsync(passenger =>
                passenger.Email == normalizedEmail);

        if (emailExists)
        {
            return Conflict(new
            {
                message =
                    "An account with this email address already exists."
            });
        }

        var passenger = new Passenger
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PhoneNumber =
                string.IsNullOrWhiteSpace(
                    request.PhoneNumber)
                    ? null
                    : request.PhoneNumber.Trim(),
            PasswordHash = string.Empty,
            IsEmailVerified = false,
            EmailVerifiedAtUtc = null,
            CreatedAt = DateTime.UtcNow
        };

        passenger.PasswordHash =
            _passwordHasher.HashPassword(
                passenger,
                request.Password);

        _context.Passengers.Add(passenger);
        await _context.SaveChangesAsync(cancellationToken);

        var challenge = await _emailOtpService
            .CreateAndSendAsync(passenger, cancellationToken);

        return Created(
            $"/api/passengers/{passenger.Id}",
            new
            {
                message =
                    "Registration successful. An OTP has been sent to your email.",

                requiresEmailVerification = true,
                verificationId = challenge.VerificationId,
                expiresAtUtc = challenge.ExpiresAtUtc,

                passenger = new
                {
                    passenger.Id,
                    passenger.FullName,
                    passenger.Email,
                    passenger.PhoneNumber,
                    passenger.CreatedAt
                }
            });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginPassengerRequest request)
    {
        var normalizedEmail = request.Email
            .Trim()
            .ToLowerInvariant();

        var passenger = await _context.Passengers
            .SingleOrDefaultAsync(passenger =>
                passenger.Email == normalizedEmail);

        if (passenger is null)
        {
            return Unauthorized(new
            {
                message =
                    "The email address or password is incorrect."
            });
        }

        var verificationResult =
            _passwordHasher.VerifyHashedPassword(
                passenger,
                passenger.PasswordHash,
                request.Password);

        if (verificationResult ==
            PasswordVerificationResult.Failed)
        {
            return Unauthorized(new
            {
                message =
                    "The email address or password is incorrect."
            });
        }

        if (verificationResult ==
            PasswordVerificationResult.SuccessRehashNeeded)
        {
            passenger.PasswordHash =
                _passwordHasher.HashPassword(
                    passenger,
                    request.Password);

            await _context.SaveChangesAsync();
        }

        if (!passenger.IsEmailVerified)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new
                {
                    message =
                        "Please verify your email address before logging in.",

                    requiresEmailVerification = true
                });
        }

        var tokenResult =
            _tokenService.CreateToken(passenger);

        return Ok(new
        {
            message = "Login successful.",

            token = tokenResult.Token,

            expiresAtUtc =
                tokenResult.ExpiresAtUtc,

            passenger = new
            {
                passenger.Id,
                passenger.FullName,
                passenger.Email,
                passenger.PhoneNumber
            }
        });
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(
        VerifyEmailOtpRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _emailOtpService.VerifyAsync(
                request.VerificationId,
                request.Otp,
                cancellationToken);

        switch (result.Status)
        {
            case EmailOtpVerificationStatus.NotFound:
                return NotFound(new
                {
                    message =
                        "The verification request was not found."
                });

            case EmailOtpVerificationStatus.Invalid:
                return BadRequest(new
                {
                    message = "The OTP is incorrect.",
                    remainingAttempts = result.RemainingAttempts
                });

            case EmailOtpVerificationStatus.Expired:
                return BadRequest(new
                {
                    message =
                        "The OTP has expired. Request a new OTP."
                });

            case EmailOtpVerificationStatus.AlreadyUsed:
                return BadRequest(new
                {
                    message =
                        "This OTP has already been used."
                });

            case EmailOtpVerificationStatus.MaximumAttemptsReached:
                return BadRequest(new
                {
                    message =
                        "The maximum number of OTP attempts has been reached. Request a new OTP."
                });
        }

        var passenger = result.Passenger!;

        var tokenResult =
            _tokenService.CreateToken(passenger);

        return Ok(new
        {
            message = "Email verified successfully.",

            passenger = new
            {
                passenger.Id,
                passenger.FullName,
                passenger.Email,
                passenger.PhoneNumber,
                passenger.IsEmailVerified,
                passenger.EmailVerifiedAtUtc
            },

            token = tokenResult.Token,
            expiresAtUtc = tokenResult.ExpiresAtUtc
        });
    }

    [HttpPost("resend-email-otp")]
    public async Task<IActionResult> ResendEmailOtp(
        ResendEmailOtpRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email
            .Trim()
            .ToLowerInvariant();

        var passenger = await _context.Passengers
            .SingleOrDefaultAsync(
                passenger => passenger.Email == normalizedEmail,
                cancellationToken);

        if (passenger is null || passenger.IsEmailVerified)
        {
            return Ok(new
            {
                message =
                    "If the account requires verification, a new OTP will be sent."
            });
        }

        var latestVerification =
            await _context.EmailOtpVerifications
                .AsNoTracking()
                .Where(verification =>
                    verification.PassengerId == passenger.Id)
                .OrderByDescending(verification =>
                    verification.CreatedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);

        if (latestVerification is not null)
        {
            var nextAllowedAtUtc =
                latestVerification.CreatedAtUtc.AddSeconds(
                    _otpOptions.ResendCooldownSeconds);

            var currentTimeUtc = DateTime.UtcNow;

            if (currentTimeUtc < nextAllowedAtUtc)
            {
                var retryAfterSeconds =
                    (int)Math.Ceiling(
                        (nextAllowedAtUtc - currentTimeUtc).TotalSeconds);

                return StatusCode(
                    StatusCodes.Status429TooManyRequests,
                    new
                    {
                        message =
                            "Please wait before requesting another OTP.",
                        retryAfterSeconds
                    });
            }
        }

        var challenge =
            await _emailOtpService.CreateAndSendAsync(
                passenger,
                cancellationToken);

        return Ok(new
        {
            message = "A new OTP has been sent.",
            verificationId = challenge.VerificationId,
            expiresAtUtc = challenge.ExpiresAtUtc
        });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email
            .Trim()
            .ToLowerInvariant();

        var passenger = await _context.Passengers
            .SingleOrDefaultAsync(
                passenger => passenger.Email == normalizedEmail,
                cancellationToken);

        const string responseMessage =
            "If an account exists for this email, a password-reset OTP has been sent.";

        // Return a fake challenge for unknown accounts so the
        // response does not reveal whether the email is registered.
        if (passenger is null)
        {
            return Ok(new
            {
                message = responseMessage,
                verificationId = Guid.NewGuid(),
                expiresAtUtc = DateTime.UtcNow.AddMinutes(_otpOptions.ExpiryMinutes)
            });
        }

        var latestResetOtp =
            await _context.EmailOtpVerifications
                .AsNoTracking()
                .Where(verification =>
                    verification.PassengerId == passenger.Id &&
                    verification.Purpose == "PasswordReset" &&
                    !verification.IsUsed)
                .OrderByDescending(verification =>
                    verification.CreatedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);

        if (latestResetOtp is not null)
        {
            var nextAllowedAtUtc =
                latestResetOtp.CreatedAtUtc.AddSeconds(
                    _otpOptions.ResendCooldownSeconds);

            if (DateTime.UtcNow < nextAllowedAtUtc &&
                DateTime.UtcNow < latestResetOtp.ExpiresAtUtc)
            {
                return Ok(new
                {
                    message = responseMessage,
                    verificationId = latestResetOtp.VerificationId,
                    expiresAtUtc = latestResetOtp.ExpiresAtUtc
                });
            }
        }

        var challenge =
            await _emailOtpService.CreatePasswordResetAndSendAsync(
                passenger,
                cancellationToken);

        return Ok(new
        {
            message = responseMessage,
            verificationId = challenge.VerificationId,
            expiresAtUtc = challenge.ExpiresAtUtc
        });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordWithOtpRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _emailOtpService.ResetPasswordAsync(
                request.VerificationId,
                request.Otp,
                request.NewPassword,
                cancellationToken);

        switch (result.Status)
        {
            case EmailOtpVerificationStatus.NotFound:
                return NotFound(new
                {
                    message =
                        "The password-reset request was not found."
                });

            case EmailOtpVerificationStatus.Invalid:
                return BadRequest(new
                {
                    message = "The OTP is incorrect.",
                    remainingAttempts = result.RemainingAttempts
                });

            case EmailOtpVerificationStatus.Expired:
                return BadRequest(new
                {
                    message =
                        "The OTP has expired. Request a new password-reset OTP."
                });

            case EmailOtpVerificationStatus.AlreadyUsed:
                return BadRequest(new
                {
                    message =
                        "This password-reset OTP has already been used."
                });

            case EmailOtpVerificationStatus.MaximumAttemptsReached:
                return BadRequest(new
                {
                    message =
                        "The maximum number of OTP attempts has been reached."
                });

            case EmailOtpVerificationStatus.NewPasswordMatchesCurrent:
                return BadRequest(new
                {
                    message =
                        "The new password must be different from the current password."
                });
        }

        return Ok(new
        {
            message =
                "Password reset successfully. You can now log in using your new password."
        });
    }
}
