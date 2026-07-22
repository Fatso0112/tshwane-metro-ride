namespace TshwaneMetroRide.Api.Models;

public class SupportRequest
{
    public int Id { get; set; }

    public required string Subject { get; set; }

    public required string Message { get; set; }

    public string Status { get; set; } = "Open";

    public DateTime CreatedAtUtc { get; set; } =
        DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public DateTime? ResolvedAtUtc { get; set; }

    public int PassengerId { get; set; }

    public Passenger Passenger { get; set; } = null!;
}