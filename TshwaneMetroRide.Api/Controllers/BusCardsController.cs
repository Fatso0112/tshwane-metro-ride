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

        var passenger = await _context.Passengers
            .Include(passenger => passenger.TravelWallet)
            .SingleOrDefaultAsync(passenger =>
                passenger.Id == passengerId.Value);

        if (passenger is null)
        {
            return NotFound(new
            {
                message = "Passenger account not found."
            });
        }

        // Existing passengers may not yet have a wallet.
        if (passenger.TravelWallet is null)
        {
            passenger.TravelWallet = new TravelWallet
            {
                Balance = 0.00m,
                Status = "Active",
                CreatedAtUtc = DateTime.UtcNow
            };
        }

        if (passenger.TravelWallet.Status != "Active")
        {
            return BadRequest(new
            {
                message =
                    "A bus card cannot be linked because the travel wallet is not active."
            });
        }

        var busCard = new BusCard
        {
            CardNumber = normalizedCardNumber,

            // Temporary legacy balance.
            // The shared wallet balance is now used.
            Balance = 0.00m,

            IsActive = true,
            Status = "Active",
            LinkedAtUtc = DateTime.UtcNow,
            PassengerId = passenger.Id,
            TravelWallet = passenger.TravelWallet
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

                    // Return the shared wallet balance,
                    // not the legacy card balance.
                    balance =
                        passenger.TravelWallet.Balance,

                    travelWalletId =
                        passenger.TravelWallet.Id,

                    walletStatus =
                        passenger.TravelWallet.Status,

                    busCard.IsActive,
                    busCard.Status,
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
                balance = card.TravelWallet != null
                    ? card.TravelWallet.Balance
                    : card.Balance,
                card.TravelWalletId,
                walletStatus = card.TravelWallet != null
                    ? card.TravelWallet.Status
                    : null,
                card.IsActive,
                card.LinkedAtUtc,
                card.Status,
                card.BlockedAtUtc,
                card.CancelledAtUtc
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

    [HttpPost("{cardId:int}/block")]
    public async Task<IActionResult> BlockCard(int cardId)
    {
        var passengerId = GetPassengerId();

        if (passengerId is null)
        {
            return Unauthorized(new
            {
                message = "The access token is invalid."
            });
        }

        var card = await _context.BusCards
            .SingleOrDefaultAsync(card =>
                card.Id == cardId &&
                card.PassengerId == passengerId.Value);

        if (card is null)
        {
            return NotFound(new
            {
                message =
                    "The bus card was not found for this passenger."
            });
        }

        if (card.Status == "Cancelled")
        {
            return BadRequest(new
            {
                message =
                    "A cancelled card cannot be blocked."
            });
        }

        if (card.Status == "Blocked")
        {
            return BadRequest(new
            {
                message =
                    "This bus card is already blocked."
            });
        }

        card.Status = "Blocked";
        card.IsActive = false;
        card.BlockedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Bus card blocked successfully.",

            busCard = new
            {
                card.Id,
                card.CardNumber,
                card.Status,
                card.IsActive,
                card.BlockedAtUtc
            }
        });
    }

    [HttpPost("{cardId:int}/reactivate")]
    public async Task<IActionResult> ReactivateCard(int cardId)
    {
        var passengerId = GetPassengerId();

        if (passengerId is null)
        {
            return Unauthorized(new
            {
                message = "The access token is invalid."
            });
        }

        var card = await _context.BusCards
            .SingleOrDefaultAsync(card =>
                card.Id == cardId &&
                card.PassengerId == passengerId.Value);

        if (card is null)
        {
            return NotFound(new
            {
                message =
                    "The bus card was not found for this passenger."
            });
        }

        if (card.Status == "Cancelled")
        {
            return BadRequest(new
            {
                message =
                    "A cancelled card cannot be reactivated."
            });
        }

        if (card.Status == "Active")
        {
            return BadRequest(new
            {
                message =
                    "This bus card is already active."
            });
        }

        card.Status = "Active";
        card.IsActive = true;
        card.BlockedAtUtc = null;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Bus card reactivated successfully.",

            busCard = new
            {
                card.Id,
                card.CardNumber,
                card.Status,
                card.IsActive
            }
        });
    }

    [HttpPost("{cardId:int}/cancel")]
    public async Task<IActionResult> CancelCard(int cardId)
    {
        var passengerId = GetPassengerId();

        if (passengerId is null)
        {
            return Unauthorized(new
            {
                message = "The access token is invalid."
            });
        }

        var card = await _context.BusCards
            .SingleOrDefaultAsync(card =>
                card.Id == cardId &&
                card.PassengerId == passengerId.Value);

        if (card is null)
        {
            return NotFound(new
            {
                message =
                    "The bus card was not found for this passenger."
            });
        }

        if (card.Status == "Cancelled")
        {
            return BadRequest(new
            {
                message =
                    "This bus card is already cancelled."
            });
        }

        card.Status = "Cancelled";
        card.IsActive = false;
        card.CancelledAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Bus card cancelled successfully.",

            busCard = new
            {
                card.Id,
                card.CardNumber,
                card.Status,
                card.IsActive,
                card.CancelledAtUtc
            }
        });
    }
}