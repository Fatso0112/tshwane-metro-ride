using System.ComponentModel.DataAnnotations;

namespace TshwaneMetroRide.Api.DTOs.Support;

public class CreateSupportRequest
{
    [Required]
    [MaxLength(150)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [MinLength(10)]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;
}