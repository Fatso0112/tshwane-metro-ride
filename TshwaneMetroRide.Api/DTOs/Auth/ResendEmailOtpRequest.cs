using System.ComponentModel.DataAnnotations;

namespace TshwaneMetroRide.Api.DTOs.Auth;

public class ResendEmailOtpRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;
}