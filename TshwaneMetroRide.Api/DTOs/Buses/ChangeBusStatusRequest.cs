using System.ComponentModel.DataAnnotations;

namespace TshwaneMetroRide.Api.DTOs.Buses;

public class ChangeBusStatusRequest
{
    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = string.Empty;
}
