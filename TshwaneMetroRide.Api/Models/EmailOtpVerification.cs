namespace TshwaneMetroRide.Api.Models;

public class EmailOtpVerification
{
    public int Id { get; set; }

    public Guid VerificationId { get; set; } =
        Guid.NewGuid();

    public required string OtpHash { get; set; }

    public string Purpose { get; set; } =
        "EmailVerification";

    public DateTime CreatedAtUtc { get; set; } =
        DateTime.UtcNow;

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? VerifiedAtUtc { get; set; }

    public int FailedAttempts { get; set; }

    public int MaximumAttempts { get; set; } = 5;

    public bool IsUsed { get; set; }

    public int PassengerId { get; set; }

    public Passenger Passenger { get; set; } = null!;
}