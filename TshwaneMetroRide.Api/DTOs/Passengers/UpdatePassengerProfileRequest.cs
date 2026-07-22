using System.ComponentModel.DataAnnotations;

namespace TshwaneMetroRide.Api.DTOs.Passengers;

public class UpdatePassengerProfileRequest
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
}