using System.ComponentModel.DataAnnotations;

namespace TshwaneMetroRide.Api.DTOs.Buses;

public class CreateBusRequest
{
    [Required]
    [MaxLength(30)]
    public string FleetNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string RegistrationNumber { get; set; } =
        string.Empty;

    [Range(1, 200)]
    public int Capacity { get; set; }

    [Range(1, int.MaxValue)]
    public int? BusRouteId { get; set; }
}