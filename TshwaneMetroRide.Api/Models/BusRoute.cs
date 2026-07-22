namespace TshwaneMetroRide.Api.Models;

public class BusRoute
{
    public int Id { get; set; }

    public required string RouteCode { get; set; }

    public required string RouteName { get; set; }

    public required string Origin { get; set; }

    public required string Destination { get; set; }

    public string? Stops { get; set; }

    public decimal FareAmount { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Ticket> Tickets { get; set; } = 
        new List<Ticket>();

    public ICollection<Bus> Buses { get; set; } =
    new List<Bus>();
}