namespace TshwaneMetroRide.Api.DTOs.Routes;

public record RouteResponse(
    int Id,
    string RouteCode,
    string RouteName,
    string Origin,
    string Destination,
    string? Stops,
    decimal FareAmount,
    bool IsActive);