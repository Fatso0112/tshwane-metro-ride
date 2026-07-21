using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TshwaneMetroRide.Api.Data;
using TshwaneMetroRide.Api.DTOs.Auth;
using TshwaneMetroRide.Api.Interfaces;
using TshwaneMetroRide.Api.Models;

namespace TshwaneMetroRide.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    private readonly
        IPasswordHasher<Passenger> _passwordHasher;

    private readonly ITokenServices _tokenService;

    public AuthController(
        ApplicationDbContext context,
        IPasswordHasher<Passenger> passwordHasher,
        ITokenServices tokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterPassengerRequest request)
    {
        var normalizedEmail = request.Email
            .Trim()
            .ToLowerInvariant();

        var emailExists = await _context.Passengers
            .AnyAsync(passenger =>
                passenger.Email == normalizedEmail);

        if (emailExists)
        {
            return Conflict(new
            {
                message =
                    "An account with this email address already exists."
            });
        }

        var passenger = new Passenger
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PhoneNumber =
                string.IsNullOrWhiteSpace(
                    request.PhoneNumber)
                    ? null
                    : request.PhoneNumber.Trim(),
            PasswordHash = string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        passenger.PasswordHash =
            _passwordHasher.HashPassword(
                passenger,
                request.Password);

        _context.Passengers.Add(passenger);
        await _context.SaveChangesAsync();

        return Created(
            $"/api/passengers/{passenger.Id}",
            new
            {
                message =
                    "Passenger registered successfully.",

                passenger = new
                {
                    passenger.Id,
                    passenger.FullName,
                    passenger.Email,
                    passenger.PhoneNumber,
                    passenger.CreatedAt
                }
            });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginPassengerRequest request)
    {
        var normalizedEmail = request.Email
            .Trim()
            .ToLowerInvariant();

        var passenger = await _context.Passengers
            .SingleOrDefaultAsync(passenger =>
                passenger.Email == normalizedEmail);

        if (passenger is null)
        {
            return Unauthorized(new
            {
                message =
                    "The email address or password is incorrect."
            });
        }

        var verificationResult =
            _passwordHasher.VerifyHashedPassword(
                passenger,
                passenger.PasswordHash,
                request.Password);

        if (verificationResult ==
            PasswordVerificationResult.Failed)
        {
            return Unauthorized(new
            {
                message =
                    "The email address or password is incorrect."
            });
        }

        if (verificationResult ==
            PasswordVerificationResult.SuccessRehashNeeded)
        {
            passenger.PasswordHash =
                _passwordHasher.HashPassword(
                    passenger,
                    request.Password);

            await _context.SaveChangesAsync();
        }

        var tokenResult =
            _tokenService.CreateToken(passenger);

        return Ok(new
        {
            message = "Login successful.",

            token = tokenResult.Token,

            expiresAtUtc =
                tokenResult.ExpiresAtUtc,

            passenger = new
            {
                passenger.Id,
                passenger.FullName,
                passenger.Email,
                passenger.PhoneNumber
            }
        });
    }
}