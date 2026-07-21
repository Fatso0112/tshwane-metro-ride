using System.ComponentModel.DataAnnotations;

namespace TshwaneMetroRide.Api.DTOs.Tickets;

public class PurchaseTicketRequest
{
    [Range(1, int.MaxValue)]
    public int CardId { get; set; }

    [Range(1, int.MaxValue)]
    public int RouteId { get; set; }

    [Phone]
    [MaxLength(20)]
    public string? PassengerPhone { get; set; }
}
