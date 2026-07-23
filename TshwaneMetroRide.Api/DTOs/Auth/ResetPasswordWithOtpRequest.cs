using System.ComponentModel.DataAnnotations;

namespace TshwaneMetroRide.Api.DTOs.Auth;

public class ResetPasswordWithOtpRequest
{
    [Required]
    public Guid VerificationId { get; set; }

    [Required]
    [RegularExpression(
        @"^\d{6}$",
        ErrorMessage =
            "The OTP must contain exactly six digits.")]
    public string Otp { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; } =
        string.Empty;

    [Required]
    [Compare(
        nameof(NewPassword),
        ErrorMessage =
            "The password confirmation does not match.")]
    public string ConfirmNewPassword { get; set; } =
        string.Empty;
}