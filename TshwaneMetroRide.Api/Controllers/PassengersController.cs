using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TshwaneMetroRide.Api.Data;
using TshwaneMetroRide.Api.DTOs.Passengers;
using TshwaneMetroRide.Api.Models;

namespace TshwaneMetroRide.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/passengers")]
public class PassengersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    private readonly IPasswordHasher<Passenger>
        _passwordHasher;

    public PassengersController(
        ApplicationDbContext context,
        IPasswordHasher<Passenger> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentPassenger()
    {
        var passengerId = GetPassengerId();

        if (passengerId is null)
        {
            return Unauthorized(new
            {
                message = "The access token is invalid."
            });
        }

        var passenger = await _context.Passengers
            .AsNoTracking()
            .Where(passenger =>
                passenger.Id == passengerId.Value)
            .Select(passenger => new
            {
                passenger.Id,
                passenger.FullName,
                passenger.Email,
                passenger.PhoneNumber,
                passenger.CreatedAt
            })
            .SingleOrDefaultAsync();

        if (passenger is null)
        {
            return NotFound(new
            {
                message = "Passenger account not found."
            });
        }

        return Ok(passenger);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile(
        UpdatePassengerProfileRequest request)
    {
        var passengerId = GetPassengerId();

        if (passengerId is null)
        {
            return Unauthorized(new
            {
                message = "The access token is invalid."
            });
        }

        var passenger = await _context.Passengers
            .SingleOrDefaultAsync(passenger =>
                passenger.Id == passengerId.Value);

        if (passenger is null)
        {
            return NotFound(new
            {
                message = "Passenger account not found."
            });
        }

        var normalizedEmail = request.Email
            .Trim()
            .ToLowerInvariant();

        var emailBelongsToAnotherPassenger =
            await _context.Passengers
                .AnyAsync(otherPassenger =>
                    otherPassenger.Id != passenger.Id &&
                    otherPassenger.Email ==
                    normalizedEmail);

        if (emailBelongsToAnotherPassenger)
        {
            return Conflict(new
            {
                message =
                    "Another account already uses this email address."
            });
        }

        passenger.FullName =
            request.FullName.Trim();

        passenger.Email =
            normalizedEmail;

        passenger.PhoneNumber =
            string.IsNullOrWhiteSpace(
                request.PhoneNumber)
                ? null
                : request.PhoneNumber.Trim();

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message =
                "Passenger profile updated successfully.",

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

    [HttpPost("me/change-password")]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordRequest request)
    {
        var passengerId = GetPassengerId();

        if (passengerId is null)
        {
            return Unauthorized(new
            {
                message = "The access token is invalid."
            });
        }

        if (request.CurrentPassword ==
            request.NewPassword)
        {
            return BadRequest(new
            {
                message =
                    "The new password must be different from the current password."
            });
        }

        var passenger = await _context.Passengers
            .SingleOrDefaultAsync(passenger =>
                passenger.Id == passengerId.Value);

        if (passenger is null)
        {
            return NotFound(new
            {
                message = "Passenger account not found."
            });
        }

        var passwordResult =
            _passwordHasher.VerifyHashedPassword(
                passenger,
                passenger.PasswordHash,
                request.CurrentPassword);

        if (passwordResult ==
            PasswordVerificationResult.Failed)
        {
            return BadRequest(new
            {
                message =
                    "The current password is incorrect."
            });
        }

        passenger.PasswordHash =
            _passwordHasher.HashPassword(
                passenger,
                request.NewPassword);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message =
                "Password changed successfully."
        });
    }

    private int? GetPassengerId()
    {
        var passengerIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        return int.TryParse(
            passengerIdValue,
            out var passengerId)
            ? passengerId
            : null;
    }
}