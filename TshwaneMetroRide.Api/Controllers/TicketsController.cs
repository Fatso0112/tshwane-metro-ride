using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;
using TshwaneMetroRide.Api.Data;
using TshwaneMetroRide.Api.DTOs.Tickets;
using TshwaneMetroRide.Api.Models;

namespace TshwaneMetroRide.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TicketsController(
        ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("purchase")]
    public async Task<IActionResult> PurchaseTicket(
        PurchaseTicketRequest request)
    {
        var passengerId = GetPassengerId();

        if (passengerId is null)
        {
            return Unauthorized(new
            {
                message = "The access token is invalid."
            });
        }

        await using var databaseTransaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        var busCard = await _context.BusCards
            .Include(card => card.TravelWallet)
            .SingleOrDefaultAsync(card =>
                card.Id == request.CardId &&
                card.PassengerId == passengerId.Value);

        if (busCard is null)
        {
            return NotFound(new
            {
                message =
                    "The selected bus card was not found."
            });
        }

        if (!busCard.IsActive)
        {
            return BadRequest(new
            {
                message =
                    "The selected bus card is inactive."
            });
        }

        if (busCard.TravelWallet is null)
        {
            return BadRequest(new
            {
                message =
                    "The selected bus card does not have an associated travel wallet."
            });
        }

        if (busCard.TravelWallet.Status != "Active")
        {
            return BadRequest(new
            {
                message =
                    "The travel wallet associated with the selected bus card is not active."
            });
        }

        var route = await _context.BusRoutes
            .AsNoTracking()
            .SingleOrDefaultAsync(route =>
                route.Id == request.RouteId);

        if (route is null)
        {
            return NotFound(new
            {
                message =
                    "The selected bus route was not found."
            });
        }

        if (!route.IsActive)
        {
            return BadRequest(new
            {
                message =
                    "The selected bus route is currently inactive."
            });
        }

        if (route.FareAmount <= 0)
        {
            return BadRequest(new
            {
                message =
                    "The selected route does not have a valid fare."
            });
        }

        if (busCard.TravelWallet.Balance < route.FareAmount)
        {
            return BadRequest(new
            {
                message =
                    "The bus card has insufficient funds.",

                requiredAmount = route.FareAmount,
                availableBalance = busCard.TravelWallet.Balance
            });
        }

        var purchasedAtUtc = DateTime.UtcNow;

        var uniqueValue =
            Guid.NewGuid()
                .ToString("N")
                .ToUpperInvariant();

        var ticketNumber =
            $"TKT-{purchasedAtUtc:yyyyMMddHHmmss}-" +
            uniqueValue[..8];

        var qrCodeValue =
            $"TMR|{ticketNumber}|{route.RouteCode}|" +
            $"{uniqueValue}";

        busCard.TravelWallet.Balance -= route.FareAmount;
        busCard.TravelWallet.UpdatedAtUtc = purchasedAtUtc;

        var ticket = new Ticket
        {
            TicketNumber = ticketNumber,
            QrCodeValue = qrCodeValue,
            FareAmount = route.FareAmount,
            Status = "Active",

            PassengerPhone =
                string.IsNullOrWhiteSpace(
                    request.PassengerPhone)
                    ? null
                    : request.PassengerPhone.Trim(),

            PurchasedAtUtc = purchasedAtUtc,
            ValidFromUtc = purchasedAtUtc,

            ValidUntilUtc =
                purchasedAtUtc.AddHours(2),

            BusCardId = busCard.Id,
            BusRouteId = route.Id
        };

        var walletTransaction =
            new WalletTransaction
            {
                Amount = -route.FareAmount,
                TransactionType = "TicketPurchase",
                Reference = ticketNumber,
                BalanceAfter = busCard.TravelWallet.Balance,
                CreatedAtUtc = purchasedAtUtc,
                BusCardId = busCard.Id,
                TravelWalletId = busCard.TravelWallet.Id
            };

        _context.Tickets.Add(ticket);

        _context.WalletTransactions.Add(
            walletTransaction);

        await _context.SaveChangesAsync();

        await databaseTransaction.CommitAsync();

        return Ok(new
        {
            message = "Ticket purchased successfully.",

            ticket = new
            {
                ticket.Id,
                ticket.TicketNumber,
                ticket.QrCodeValue,
                ticket.FareAmount,
                ticket.Status,
                ticket.PassengerPhone,
                ticket.PurchasedAtUtc,
                ticket.ValidFromUtc,
                ticket.ValidUntilUtc,

                route = new
                {
                    route.Id,
                    route.RouteCode,
                    route.RouteName,
                    route.Origin,
                    route.Destination
                }
            },

            busCard = new
            {
                busCard.Id,
                busCard.CardNumber,
                busCard.TravelWallet.Balance
            }
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetPassengerTickets()
    {
        var passengerId = GetPassengerId();

        if (passengerId is null)
        {
            return Unauthorized(new
            {
                message = "The access token is invalid."
            });
        }

        var tickets = await _context.Tickets
            .AsNoTracking()
            .Where(ticket =>
                ticket.BusCard.PassengerId ==
                passengerId.Value)
            .OrderByDescending(ticket =>
                ticket.PurchasedAtUtc)
            .Select(ticket => new
            {
                ticket.Id,
                ticket.TicketNumber,
                ticket.QrCodeValue,
                ticket.FareAmount,
                ticket.Status,
                ticket.PassengerPhone,
                ticket.PurchasedAtUtc,
                ticket.ValidFromUtc,
                ticket.ValidUntilUtc,

                route = new
                {
                    ticket.BusRoute.Id,
                    ticket.BusRoute.RouteCode,
                    ticket.BusRoute.RouteName,
                    ticket.BusRoute.Origin,
                    ticket.BusRoute.Destination
                },

                busCard = new
                {
                    ticket.BusCard.Id,
                    ticket.BusCard.CardNumber
                }
            })
            .ToListAsync();

        return Ok(new
        {
            count = tickets.Count,
            tickets
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTicketById(
        int id)
    {
        var passengerId = GetPassengerId();

        if (passengerId is null)
        {
            return Unauthorized(new
            {
                message = "The access token is invalid."
            });
        }

        var ticket = await _context.Tickets
            .AsNoTracking()
            .Where(ticket =>
                ticket.Id == id &&
                ticket.BusCard.PassengerId ==
                passengerId.Value)
            .Select(ticket => new
            {
                ticket.Id,
                ticket.TicketNumber,
                ticket.QrCodeValue,
                ticket.FareAmount,
                ticket.Status,
                ticket.PassengerPhone,
                ticket.PurchasedAtUtc,
                ticket.ValidFromUtc,
                ticket.ValidUntilUtc,

                route = new
                {
                    ticket.BusRoute.Id,
                    ticket.BusRoute.RouteCode,
                    ticket.BusRoute.RouteName,
                    ticket.BusRoute.Origin,
                    ticket.BusRoute.Destination,
                    ticket.BusRoute.Stops
                },

                busCard = new
                {
                    ticket.BusCard.Id,
                    ticket.BusCard.CardNumber
                }
            })
            .SingleOrDefaultAsync();

        if (ticket is null)
        {
            return NotFound(new
            {
                message = "The ticket was not found."
            });
        }

        return Ok(ticket);
    }

    [HttpPost("validate/{ticketNumber}")]
    public async Task<IActionResult> ValidateTicket(
        string ticketNumber)
    {
        if (string.IsNullOrWhiteSpace(ticketNumber))
        {
            return BadRequest(new
            {
                message = "A ticket number is required."
            });
        }

        var normalizedTicketNumber = ticketNumber
            .Trim()
            .ToUpperInvariant();

        var ticket = await _context.Tickets
            .Include(ticket => ticket.BusRoute)
            .Include(ticket => ticket.BusCard)
            .SingleOrDefaultAsync(ticket =>
                ticket.TicketNumber == normalizedTicketNumber);

        if (ticket is null)
        {
            return NotFound(new
            {
                message = "The ticket is invalid or does not exist."
            });
        }

        if (ticket.Status == "Used")
        {
            return BadRequest(new
            {
                message = "This ticket has already been used.",
                ticketNumber = ticket.TicketNumber,
                usedAtUtc = ticket.UsedAtUtc
            });
        }

        if (ticket.Status == "Cancelled")
        {
            return BadRequest(new
            {
                message = "This ticket has been cancelled."
            });
        }

        if (ticket.Status == "Expired")
        {
            return BadRequest(new
            {
                message = "This ticket has expired.",
                validUntilUtc = ticket.ValidUntilUtc
            });
        }

        var currentTimeUtc = DateTime.UtcNow;

        if (currentTimeUtc < ticket.ValidFromUtc)
        {
            return BadRequest(new
            {
                message = "This ticket is not valid yet.",
                validFromUtc = ticket.ValidFromUtc
            });
        }

        if (currentTimeUtc > ticket.ValidUntilUtc)
        {
            ticket.Status = "Expired";

            await _context.SaveChangesAsync();

            return BadRequest(new
            {
                message = "This ticket has expired.",
                validUntilUtc = ticket.ValidUntilUtc
            });
        }

        if (!ticket.BusCard.IsActive)
        {
            return BadRequest(new
            {
                message =
                    "The bus card associated with this ticket is inactive."
            });
        }

        ticket.Status = "Used";
        ticket.UsedAtUtc = currentTimeUtc;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Ticket validated successfully. Passenger may board.",

            ticket = new
            {
                ticket.Id,
                ticket.TicketNumber,
                ticket.Status,
                ticket.FareAmount,
                ticket.PurchasedAtUtc,
                ticket.ValidFromUtc,
                ticket.ValidUntilUtc,
                ticket.UsedAtUtc,

                route = new
                {
                    ticket.BusRoute.Id,
                    ticket.BusRoute.RouteCode,
                    ticket.BusRoute.RouteName,
                    ticket.BusRoute.Origin,
                    ticket.BusRoute.Destination
                },

                busCard = new
                {
                    ticket.BusCard.Id,
                    ticket.BusCard.CardNumber
                }
            }
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