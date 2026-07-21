using System.ComponentModel.DataAnnotations;

namespace TshwaneMetroRide.Api.DTOs.BusCards;

public class LinkBusCardRequest
{
    [Required]
    [MinLength(6)]
    [MaxLength(30)]
    public string CardNumber { get; set; } = string.Empty;
}