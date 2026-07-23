namespace TshwaneMetroRide.Api.Models;

public class Passenger
{
    public int Id { get; set; }

    public required string FullName { get; set; }

    public required string Email { get; set; }

    public string? PhoneNumber { get; set; }

    public required string PasswordHash { get; set; }

    public TravelWallet? TravelWallet { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<BusCard> BusCards { get; set; } = 
        new List<BusCard>();

    public ICollection<SupportRequest> SupportRequests { get; set; } = 
        new List<SupportRequest>();

    public string Role { get; set; } = "Passenger";

    public bool IsEmailVerified { get; set; } = false;

    public DateTime? EmailVerifiedAtUtc { get; set; }

    public ICollection<EmailOtpVerification>
        EmailOtpVerifications { get; set; } =
            new List<EmailOtpVerification>();
}