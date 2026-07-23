namespace TshwaneMetroRide.Api.Models;

public class TravelWallet
{
    public int Id { get; set; }

    public decimal Balance { get; set; } = 0.00m;

    public string Status { get; set; } = "Active";

    public DateTime CreatedAtUtc { get; set; } =
        DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public int PassengerId { get; set; }

    public Passenger Passenger { get; set; } = null!;

    public ICollection<BusCard> BusCards { get; set; } =
        new List<BusCard>();

    public ICollection<WalletTransaction> Transactions { get; set; } =
        new List<WalletTransaction>();
}