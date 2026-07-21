using TshwaneMetroRide.Api.Models;
using TshwaneMetroRide.Api.DTOs.Auth;

namespace TshwaneMetroRide.Api.Interfaces;

public interface ITokenServices
{
    TokenResult CreateToken(Passenger passenger);
}