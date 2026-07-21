namespace TshwaneMetroRide.Api.Models;

public class BusCard
{
    public int Id { get; set; }

    public required string CardNumber { get; set; }

    public decimal Balance { get; set; } = 0.00m;

    public bool IsActive { get; set; } = true;

    public DateTime LinkedAtUtc { get; set; } = DateTime.UtcNow;

    public int PassengerId { get; set; }

    public Passenger Passenger { get; set; } = null!;

    public ICollection<WalletTransaction> Transactions { get; set; } = 
        new List<WalletTransaction>();

    public ICollection<Ticket> Tickets { get; set; } = 
        new List<Ticket>();
}