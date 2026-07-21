namespace TshwaneMetroRide.Api.DTOs.Auth;

public record TokenResult(
    string Token,
    DateTime ExpiresAtUtc);