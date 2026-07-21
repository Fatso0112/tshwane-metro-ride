using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TshwaneMetroRide.Api.Data;

namespace TshwaneMetroRide.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/passengers")]
public class PassengersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PassengersController(
        ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("me")]
    public async Task<IActionResult>
        GetCurrentPassenger()
    {
        var passengerIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!int.TryParse(
            passengerIdValue,
            out var passengerId))
        {
            return Unauthorized(new
            {
                message =
                    "The access token is invalid."
            });
        }

        var passenger = await _context.Passengers
            .AsNoTracking()
            .Where(passenger =>
                passenger.Id == passengerId)
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
                message =
                    "Passenger account not found."
            });
        }

        return Ok(passenger);
    }
}