namespace TshwaneMetroRide.Api.Models;

public class Passenger
{
    public int Id { get; set; }

    public required string FullName { get; set; }

    public required string Email { get; set; }

    public string? PhoneNumber { get; set; }

    public required string PasswordHash { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}