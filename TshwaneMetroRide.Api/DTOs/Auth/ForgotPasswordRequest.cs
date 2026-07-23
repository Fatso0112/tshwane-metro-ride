using System.ComponentModel.DataAnnotations;

namespace TshwaneMetroRide.Api.DTOs.Auth;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;
}