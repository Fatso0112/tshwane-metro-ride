namespace TshwaneMetroRide.Api.Models;

public class Ticket
{
    public int Id { get; set; }

    public required string TicketNumber { get; set; }

    public required string QrCodeValue { get; set; }

    public decimal FareAmount { get; set; }

    public string Status { get; set; } = "Active";

    public string? PassengerPhone { get; set; }

    public DateTime PurchasedAtUtc { get; set; } =
        DateTime.UtcNow;

    public DateTime ValidFromUtc { get; set; }

    public DateTime ValidUntilUtc { get; set; }

    public int BusCardId { get; set; }

    public BusCard BusCard { get; set; } = null!;

    public int BusRouteId { get; set; }

    public BusRoute BusRoute { get; set; } = null!;
}