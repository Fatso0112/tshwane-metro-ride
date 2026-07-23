namespace TshwaneMetroRide.Api.Options;

public class OtpOptions
{
    public const string SectionName = "Otp";

    public string HashKey { get; set; } = string.Empty;

    public int ExpiryMinutes { get; set; } = 10;

    public int MaximumAttempts { get; set; } = 5;

    public int ResendCooldownSeconds { get; set; } = 60;
}