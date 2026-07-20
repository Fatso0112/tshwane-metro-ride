using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TshwaneMetroRide.Api.Data;
using TshwaneMetroRide.Api.DTOs.Auth;
using TshwaneMetroRide.Api.Models;

namespace TshwaneMetroRide.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<Passenger> _passwordHasher;

    public AuthController(
        ApplicationDbContext context,
        IPasswordHasher<Passenger> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
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
                string.IsNullOrWhiteSpace(request.PhoneNumber)
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
                message = "Passenger registered successfully.",
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
}