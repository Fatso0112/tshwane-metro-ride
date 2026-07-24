using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TshwaneMetroRide.Api.DTOs.Auth;
using TshwaneMetroRide.Api.Interfaces;
using TshwaneMetroRide.Api.Models;

namespace TshwaneMetroRide.Api.Services;

public class TokenService : ITokenServices
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public TokenResult CreateToken(Passenger passenger)
    {
        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "The JWT signing key is missing.");

        var issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "The JWT issuer is missing.");

        var audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "The JWT audience is missing.");

        var expiresInMinutes =
            _configuration.GetValue<int?>(
                "Jwt:ExpiresInMinutes") ?? 60;

        var expiresAtUtc =
            DateTime.UtcNow.AddMinutes(expiresInMinutes);

        var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                passenger.Id.ToString()),

            new(
                ClaimTypes.Name,
                passenger.FullName),

            new(
                ClaimTypes.Email,
                passenger.Email),

            new(
                ClaimTypes.Role,
                passenger.Role),

            new(
                "email_verified",
                passenger.IsEmailVerified
                    ? "true"
                    : "false"),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        };
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey));

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        var tokenValue =
            new JwtSecurityTokenHandler()
                .WriteToken(token);

        return new TokenResult(
            tokenValue,
            expiresAtUtc);
    }
}