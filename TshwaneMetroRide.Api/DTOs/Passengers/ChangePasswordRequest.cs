using System.ComponentModel.DataAnnotations;

namespace TshwaneMetroRide.Api.DTOs.Passengers;

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } =
        string.Empty;

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