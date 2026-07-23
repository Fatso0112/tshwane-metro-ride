using System.ComponentModel.DataAnnotations;

namespace TshwaneMetroRide.Api.DTOs.Auth;

public class VerifyEmailOtpRequest
{
    [Required]
    public Guid VerificationId { get; set; }

    [Required]
    [RegularExpression(
        @"^\d{6}$",
        ErrorMessage =
            "The OTP must contain exactly six digits.")]
    public string Otp { get; set; } = string.Empty;
}