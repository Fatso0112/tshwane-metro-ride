namespace TshwaneMetroRide.Api.Models;

public class WalletTransaction
{
    public int Id { get; set; }

    public decimal Amount { get; set; }

    public required string TransactionType { get; set; }

    public required string Reference { get; set; }

    public decimal BalanceAfter { get; set; }

    public string? PaymentMethod { get; set; }

    public DateTime CreatedAtUtc { get; set; } =
        DateTime.UtcNow;

    public int BusCardId { get; set; }

    public BusCard BusCard { get; set; } = null!;
}
