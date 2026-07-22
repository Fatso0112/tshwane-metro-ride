namespace TshwaneMetroRide.Api.Models;

public class Bus
{
    public int Id { get; set; }

    public required string FleetNumber { get; set; }

    public required string RegistrationNumber { get; set; }

    public int Capacity { get; set; }

    public string Status { get; set; } = "Active";

    public DateTime CreatedAtUtc { get; set; } =
        DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public int? BusRouteId { get; set; }

    public BusRoute? BusRoute { get; set; }
}