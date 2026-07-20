using System.ComponentModel.DataAnnotations;

namespace TshwaneMetroRide.Api.DTOs.Auth;


public class RegisterPassengerRequest
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}