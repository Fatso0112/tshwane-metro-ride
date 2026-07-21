using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TshwaneMetroRide.Api.Data;
using TshwaneMetroRide.Api.DTOs.BusCards;
using TshwaneMetroRide.Api.Models;

namespace TshwaneMetroRide.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/bus-cards")]
public class BusCardsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BusCardsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("link")]
    public async Task<IActionResult> LinkBusCard(
        LinkBusCardRequest request)
    {
        var passengerId = GetPassengerId();

        if (passengerId is null)
        {
            return Unauthorized(new
            {
                message = "The access token is invalid."
            });
        }

        var normalizedCardNumber = request.CardNumber
            .Trim()
            .Replace(" ", string.Empty)
            .ToUpperInvariant();

        var cardExists = await _context.BusCards
            .AnyAsync(card =>
                card.CardNumber == normalizedCardNumber);

        if (cardExists)
        {
            return Conflict(new
            {
                message =
                    "This bus card is already linked to an account."
            });
        }

        var passengerExists = await _context.Passengers
            .AnyAsync(passenger =>
                passenger.Id == passengerId.Value);

        if (!passengerExists)
        {
            return NotFound(new
            {
                message = "Passenger account not found."
            });
        }

        var busCard = new BusCard
        {
            CardNumber = normalizedCardNumber,
            Balance = 0.00m,
            IsActive = true,
            LinkedAtUtc = DateTime.UtcNow,
            PassengerId = passengerId.Value
        };

        _context.BusCards.Add(busCard);
        await _context.SaveChangesAsync();

        return Created(
            $"/api/bus-cards/{busCard.Id}",
            new
            {
                message = "Bus card linked successfully.",
                busCard = new
                {
                    busCard.Id,
                    busCard.CardNumber,
                    busCard.Balance,
                    busCard.IsActive,
                    busCard.LinkedAtUtc
                }
            });
    }

    [HttpGet]
    public async Task<IActionResult> GetPassengerCards()
    {
        var passengerId = GetPassengerId();

        if (passengerId is null)
        {
            return Unauthorized(new
            {
                message = "The access token is invalid."
            });
        }

        var cards = await _context.BusCards
            .AsNoTracking()
            .Where(card =>
                card.PassengerId == passengerId.Value)
            .OrderByDescending(card => card.LinkedAtUtc)
            .Select(card => new
            {
                card.Id,
                card.CardNumber,
                card.Balance,
                card.IsActive,
                card.LinkedAtUtc
            })
            .ToListAsync();

        return Ok(new
        {
            count = cards.Count,
            busCards = cards
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